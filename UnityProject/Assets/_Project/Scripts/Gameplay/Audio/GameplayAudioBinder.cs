using MobCrush.Core.Audio;
using MobCrush.Core.Events;
using MobCrush.Core.Services;
using UnityEngine;

namespace MobCrush.Gameplay.Audio
{
    /// <summary>
    /// The single bridge from gameplay events to SFX ids (Loop 18). Gameplay systems
    /// publish domain events and know nothing about sound; this binder is the only
    /// place that maps event → clip id, so the entire soundscape is retunable here
    /// (and mutable ids stay out of a dozen systems). Clip ids resolve via AudioLibrary.
    /// </summary>
    public sealed class GameplayAudioBinder : MonoBehaviour
    {
        [Header("Clip ids (must exist in SO_AudioLibrary)")]
        [SerializeField] private string _enemyDieSfx = "sfx.enemy.die";
        [SerializeField] private string _eliteDieSfx = "sfx.enemy.elite_die";
        [SerializeField] private string _playerHurtSfx = "sfx.player.hurt";
        [SerializeField] private string _levelUpSfx = "sfx.player.levelup";
        [SerializeField] private string _upgradeChosenSfx = "sfx.ui.upgrade";
        [SerializeField] private string _bossSpawnSfx = "sfx.boss.spawn";
        [SerializeField] private string _bossDefeatedSfx = "sfx.boss.defeated";

        private IAudioService _audio;
        private IEventBus _events;

        private void Awake()
        {
            _audio = ServiceLocator.Get<IAudioService>();
            _events = ServiceLocator.Get<IEventBus>();

            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Subscribe<LevelUpEvent>(OnLevelUp);
            _events.Subscribe<UpgradeChosenEvent>(OnUpgradeChosen);
            _events.Subscribe<BossSpawnedEvent>(OnBossSpawned);
            _events.Subscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDestroy()
        {
            _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Unsubscribe<LevelUpEvent>(OnLevelUp);
            _events.Unsubscribe<UpgradeChosenEvent>(OnUpgradeChosen);
            _events.Unsubscribe<BossSpawnedEvent>(OnBossSpawned);
            _events.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        // Positional for world events (spatialized), flat for global ones.
        private void OnEnemyKilled(EnemyKilledEvent e) => _audio.PlaySfxAt(e.WasElite ? _eliteDieSfx : _enemyDieSfx, e.Position);
        private void OnPlayerDamaged(PlayerDamagedEvent e) => _audio.PlaySfx(_playerHurtSfx);
        private void OnLevelUp(LevelUpEvent e) => _audio.PlaySfx(_levelUpSfx);
        private void OnUpgradeChosen(UpgradeChosenEvent e) => _audio.PlaySfx(_upgradeChosenSfx);
        private void OnBossSpawned(BossSpawnedEvent e) => _audio.PlaySfx(_bossSpawnSfx);
        private void OnBossDefeated(BossDefeatedEvent e) => _audio.PlaySfx(_bossDefeatedSfx);
    }
}
