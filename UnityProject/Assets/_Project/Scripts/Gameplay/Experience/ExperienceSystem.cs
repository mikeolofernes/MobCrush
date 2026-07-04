using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using MobCrush.Gameplay.Player;
using UnityEngine;

namespace MobCrush.Gameplay.Experience
{
    /// <summary>
    /// In-run progression (GDD §5/§10-adjacent): spawns gems on kills, ticks all gems
    /// centrally (pickup-radius check + magnet attraction), accumulates XP through the
    /// curve SO, and publishes LevelUpEvent — which Loop 11's drafter consumes.
    /// </summary>
    public sealed class ExperienceSystem : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private ExperienceCurve _curve;
        [SerializeField] private GameObject _gemPrefab;

        private readonly List<ExperienceGem> _gems = new(256);
        private IEventBus _events;
        private IPoolService _pool;

        public int Level { get; private set; } = 1;
        public float CurrentXp { get; private set; }
        public float RequiredXp => _curve.GetRequiredXp(Level);

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _pool = ServiceLocator.Get<IPoolService>();
            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDestroy() => _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            var go = _pool.Get(_gemPrefab, evt.Position, Quaternion.identity);
            var gem = go.GetComponent<ExperienceGem>();
            gem.Setup(evt.XpValue);
            _gems.Add(gem);
        }

        private void Update()
        {
            if (_player.State == PlayerState.Dead) return;

            Vector2 playerPos = _player.transform.position;
            float radius = _player.Stats.Get(StatType.PickupRadius);
            float radiusSqr = radius * radius;
            float dt = Time.deltaTime;

            for (int i = _gems.Count - 1; i >= 0; i--)
            {
                var gem = _gems[i];

                if (!gem.Attracting &&
                    ((Vector2)gem.transform.position - playerPos).sqrMagnitude <= radiusSqr)
                {
                    gem.BeginAttract();
                }

                if (gem.TickAttract(playerPos, dt))
                {
                    Collect(gem, i);
                }
            }
        }

        /// <summary>Magnet pickup item (GDD §6): every gem on the field flies to the player.</summary>
        public void AttractAll()
        {
            for (int i = 0; i < _gems.Count; i++)
                _gems[i].BeginAttract();
        }

        private void Collect(ExperienceGem gem, int index)
        {
            float gained = gem.XpValue * (1f + _player.Stats.Get(StatType.XpGainPercent));
            _gems.RemoveAt(index);
            _pool.Release(gem.gameObject);

            CurrentXp += gained;
            _events.Publish(new ExperienceGainedEvent(gained, CurrentXp, RequiredXp));

            // while-loop: one fat gem may bank multiple levels; each publishes its own draft.
            while (CurrentXp >= RequiredXp && Level < _curve.MaxLevel)
            {
                CurrentXp -= RequiredXp;
                Level++;
                _events.Publish(new LevelUpEvent(Level));
            }
        }

        /// <summary>Run teardown: return outstanding gems.</summary>
        public void ClearAll()
        {
            for (int i = 0; i < _gems.Count; i++)
                _pool.Release(_gems[i].gameObject);
            _gems.Clear();
            Level = 1;
            CurrentXp = 0f;
        }
    }
}
