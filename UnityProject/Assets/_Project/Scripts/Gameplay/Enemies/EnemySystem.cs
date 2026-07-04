using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Enemies
{
    /// <summary>
    /// Owns all live enemies: single tick loop (one Update for N enemies), spawn/despawn
    /// through the pool, kill events, and nearest-target queries for auto-combat (Loop 8).
    /// Lives in the Game scene; acts as the enemies' composition point (locator use allowed).
    /// </summary>
    public sealed class EnemySystem : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private PlayerHealthRef _playerHealth;

        // Serialized indirection so this asmdef-internal reference survives prefab wiring.
        [System.Serializable]
        public sealed class PlayerHealthRef { public MonoBehaviour Behaviour; public IDamageable Damageable => Behaviour as IDamageable; }

        private readonly List<EnemyController> _live = new(256);
        private IPoolService _pool;
        private IEventBus _events;

        public int LiveCount => _live.Count;

        private void Awake()
        {
            _pool = ServiceLocator.Get<IPoolService>();
            _events = ServiceLocator.Get<IEventBus>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            // Reverse loop: despawns during Tick remove from the list safely.
            for (int i = _live.Count - 1; i >= 0; i--)
                _live[i].Tick(dt);
        }

        /// <summary>Spawns one enemy from its definition. Elite multipliers come from the wave table (Loop 12).</summary>
        public EnemyController Spawn(EnemyDefinition definition, Vector3 position,
                                     bool elite = false, float hpMultiplier = 1f, float damageMultiplier = 1f)
        {
            var go = _pool.Get(definition.Prefab, position, Quaternion.identity);
            var enemy = go.GetComponent<EnemyController>();
            enemy.Initialize(definition, this, _player, _playerHealth.Damageable, elite, hpMultiplier, damageMultiplier);
            _live.Add(enemy);
            return enemy;
        }

        public void Despawn(EnemyController enemy, bool giveRewards)
        {
            _live.Remove(enemy);
            if (giveRewards)
            {
                // Elites are worth 10x XP (GDD §2 reward weighting).
                float xp = enemy.Definition.XpValue * (enemy.IsElite ? 10f : 1f);
                _events.Publish(new EnemyKilledEvent(enemy.Definition.Id, enemy.transform.position, enemy.IsElite, xp));
            }
            _pool.Release(enemy.gameObject);
        }

        public void DespawnAll()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
                _pool.Release(_live[i].gameObject);
            _live.Clear();
        }

        /// <summary>Damage-number/analytics feed without every enemy holding a bus reference.</summary>
        public void NotifyDamageDealt(float amount, bool critical, Vector3 position) =>
            _events.Publish(new DamageDealtEvent(amount, critical, position));

        public void FireEnemyProjectile(EnemyController shooter)
        {
            var def = shooter.Definition;
            if (def.ProjectilePrefab == null) return;

            Vector2 dir = ((Vector2)_player.position - shooter.Position).normalized;
            var go = _pool.Get(def.ProjectilePrefab, shooter.Position, Quaternion.identity);
            go.GetComponent<EnemyProjectile>()
              .Launch(dir, def.ProjectileSpeed, def.ContactDamage, _playerHealth.Damageable, _pool);
        }

        // --- Targeting queries (consumed by Loop 8's TargetingService) ---

        /// <summary>Nearest living enemy to a point, or null. Linear scan: at ≤300 enemies a cache-friendly
        /// list scan beats maintaining a spatial grid; revisit in Loop 21 only if profiling disagrees.</summary>
        public EnemyController FindNearest(Vector2 from, float maxRange = float.PositiveInfinity)
        {
            EnemyController best = null;
            float bestSqr = maxRange * maxRange;
            for (int i = 0; i < _live.Count; i++)
            {
                var e = _live[i];
                if (!e.IsAlive) continue;
                float sqr = (e.Position - from).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = e; }
            }
            return best;
        }

        /// <summary>Fills a caller-owned buffer with enemies inside a radius (no allocation). Returns count.</summary>
        public int QueryRadius(Vector2 center, float radius, List<EnemyController> results)
        {
            results.Clear();
            float sqrRadius = radius * radius;
            for (int i = 0; i < _live.Count; i++)
            {
                var e = _live[i];
                if (e.IsAlive && (e.Position - center).sqrMagnitude <= sqrRadius)
                    results.Add(e);
            }
            return results.Count;
        }
    }
}
