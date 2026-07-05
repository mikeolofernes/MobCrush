using System.Collections;
using MobCrush.Core.Events;
using MobCrush.Core.SceneFlow;
using MobCrush.Core.Services;
using MobCrush.Gameplay.Enemies;
using MobCrush.Gameplay.Experience;
using MobCrush.Gameplay.Player;
using MobCrush.Gameplay.Weapons;
using UnityEngine;

namespace MobCrush.Gameplay.Run
{
    /// <summary>
    /// The run lifecycle orchestrator (post-review fix: previously NOTHING published
    /// RunEndedEvent, so results/autosave/missions/rewards all dead-ended).
    /// Owns: kill/duration/level bookkeeping, the death → revive-window → defeat path,
    /// the boss-defeat → victory path, combat teardown, and the single RunEndedEvent.
    /// Reward GRANTING stays Meta-side (RunRewardGranter) — Gameplay never touches wallets.
    /// </summary>
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private WeaponController _weapons;
        [SerializeField] private EnemySystem _enemies;
        [SerializeField] private ExperienceSystem _experience;

        [Tooltip("Seconds between boss-kill and the Results screen — the victory beat.")]
        [SerializeField] private float _endSequenceSeconds = 1.5f;

        [Tooltip("Seconds the player gets to take the revive offer after dying. The revive UI can end it early via DeclineRevive.")]
        [SerializeField] private float _reviveWindowSeconds = 6f;

        private IEventBus _events;
        private IGameStateMachine _state;
        private int _kills;
        private float _elapsed;
        private bool _ended;
        private bool _deathPending; // true during the revive window
        private Coroutine _endRoutine;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _state = ServiceLocator.Get<IGameStateMachine>();

            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            _events.Subscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDestroy()
        {
            _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            _events.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        private void Update()
        {
            // Scaled time on purpose: pauses/drafts don't count toward run duration.
            if (!_ended && _state.Current == GameState.Playing)
                _elapsed += Time.deltaTime;
        }

        private void OnEnemyKilled(EnemyKilledEvent _) => _kills++;

        private bool _reviveConsumed; // GDD §1: one revive per run

        /// <summary>True while the death revive window is open — the revive UI keys off this.</summary>
        public bool IsReviveWindowOpen => _deathPending && !_reviveConsumed;

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            if (_ended || _deathPending) return;
            _deathPending = true;
            _state.Set(GameState.RunEnding);
            // Defeat waits out the revive window (long enough to watch the prompt and decide,
            // skippable via DeclineRevive). A spent revive gets only the short sequence beat.
            float delay = _reviveConsumed ? _endSequenceSeconds : _reviveWindowSeconds;
            _endRoutine = StartCoroutine(EndAfterDelay(victory: false, delay));
        }

        private void OnBossDefeated(BossDefeatedEvent _)
        {
            if (_ended || _deathPending) return;
            _state.Set(GameState.RunEnding);
            _endRoutine = StartCoroutine(EndAfterDelay(victory: true, _endSequenceSeconds));
        }

        /// <summary>
        /// Revive entry point for the UI/ads flow (GDD §1: once per run, rewarded ad or gems).
        /// The CALLER owns the "was the ad watched / were gems paid" gate; this only checks
        /// that a death is actually pending. Returns false if the window already closed.
        /// </summary>
        public bool TryRevive(float hpFraction = 0.5f)
        {
            if (_ended || !_deathPending || _reviveConsumed) return false;

            if (_endRoutine != null) StopCoroutine(_endRoutine);
            _reviveConsumed = true;
            _deathPending = false;
            _player.Revive(hpFraction);
            _state.Set(GameState.Playing);
            return true;
        }

        /// <summary>Revive UI declined (or the ad failed): skip the rest of the window, land the defeat now.</summary>
        public void DeclineRevive()
        {
            if (_ended || !_deathPending) return;
            if (_endRoutine != null) StopCoroutine(_endRoutine);
            End(victory: false);
        }

        private IEnumerator EndAfterDelay(bool victory, float delay)
        {
            yield return new WaitForSeconds(delay); // RunEnding is not frozen — the beat plays out
            End(victory);
        }

        private void End(bool victory)
        {
            if (_ended) return;
            _ended = true;
            _deathPending = false;

            // Teardown BEFORE the event: listeners (results UI, reward granter, autosave)
            // must observe a quiet battlefield and final numbers.
            _weapons.UnequipAll();
            _enemies.DespawnAll();

            _events.Publish(new RunEndedEvent(victory, _elapsed, _kills, _experience.Level));
            _state.Set(GameState.Results);
        }
    }
}
