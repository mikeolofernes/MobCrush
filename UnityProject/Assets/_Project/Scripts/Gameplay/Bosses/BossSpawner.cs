using MobCrush.Core.Events;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Enemies;
using MobCrush.Gameplay.Spawning;
using UnityEngine;

namespace MobCrush.Gameplay.Bosses
{
    /// <summary>
    /// Bridges SpawnSystem's boss trigger to a live BossController: clears trash mobs
    /// (the boss IS the fight — also frees the frame budget), spawns the boss off-screen
    /// top, and hands it its dependencies. Kept separate from SpawnSystem so the wave
    /// executor stays boss-agnostic and reusable (e.g. Endless mode has no boss).
    /// </summary>
    public sealed class BossSpawner : MonoBehaviour
    {
        [SerializeField] private SpawnSystem _spawnSystem;
        [SerializeField] private EnemySystem _enemies;
        [SerializeField] private Transform _player;
        [SerializeField] private MonoBehaviour _playerDamageable; // PlayerHealth

        [Tooltip("Spawn offset above the player (just off-screen).")]
        [SerializeField] private Vector2 _spawnOffset = new(0f, 10f);

        public BossController ActiveBoss { get; private set; }

        private void Awake() => _spawnSystem.BossTimeReached += OnBossTime;
        private void OnDestroy() => _spawnSystem.BossTimeReached -= OnBossTime;

        private void OnBossTime(BossDefinition definition)
        {
            if (definition == null) return;

            _enemies.DespawnAll(); // arena reset: clean read of the boss's opening pattern

            Vector3 pos = _player.position + (Vector3)_spawnOffset;
            // Bosses are unique per stage: direct Instantiate is correct, pooling a one-off wastes memory.
            var go = Instantiate(definition.Prefab, pos, Quaternion.identity);
            ActiveBoss = go.GetComponent<BossController>();
            ActiveBoss.Initialize(definition, _enemies, _player,
                (Combat.IDamageable)_playerDamageable, ServiceLocator.Get<IEventBus>(),
                ServiceLocator.Get<Core.Pooling.IPoolService>());

            _enemies.SetBossTarget(ActiveBoss); // player weapons can now aim at and hit the boss
        }
    }
}
