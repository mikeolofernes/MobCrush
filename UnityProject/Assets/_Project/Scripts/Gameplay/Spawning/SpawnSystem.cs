using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Enemies;
using UnityEngine;

namespace MobCrush.Gameplay.Spawning
{
    /// <summary>
    /// Executes a StageDefinition's spawn script (Loop 12): per-wave spawn-rate accumulators,
    /// off-screen ring placement, time-scaled difficulty multipliers, elite injections,
    /// live-count throttling, and the boss trigger at stage end.
    /// The spawner holds NO balance numbers — it is a dumb, reusable executor.
    /// </summary>
    public sealed class SpawnSystem : MonoBehaviour
    {
        [SerializeField] private EnemySystem _enemies;
        [SerializeField] private Transform _player;
        [SerializeField] private StageDefinition _stage;

        [Tooltip("Spawn ring radii: just past screen edge so arrivals feel continuous, never teleport-y.")]
        [SerializeField] private float _ringInner = 12f;
        [SerializeField] private float _ringOuter = 15f;

        private readonly System.Random _rng = new();
        private float[] _waveAccumulators;
        private int _nextEliteIndex;
        private float _elapsed;
        private bool _bossTriggered;
        private IEventBus _events;

        public float Elapsed => _elapsed;
        public float Duration => _stage.DurationSeconds;
        public StageDefinition Stage => _stage;
        public bool BossTriggered => _bossTriggered;

        /// <summary>Fired locally so the Boss system (Loop 13) can take over; also published on the bus.</summary>
        public event System.Action<BossDefinition> BossTimeReached;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _waveAccumulators = new float[_stage.Waves.Count];

            // Warm pools during load, not mid-fight (Loop 0 rule).
            var pool = ServiceLocator.Get<IPoolService>();
            foreach (var entry in _stage.WarmUp)
                if (entry.Prefab != null) pool.WarmUp(entry.Prefab, entry.Count);
        }

        private void Update()
        {
            if (_bossTriggered) return; // spawn script ends when the boss takes the stage

            float dt = Time.deltaTime;
            _elapsed += dt;

            TickWaves(dt);
            TickElites();

            if (_elapsed >= _stage.DurationSeconds)
            {
                _bossTriggered = true;
                BossTimeReached?.Invoke(_stage.Boss);
                if (_stage.Boss != null)
                    _events.Publish(new BossSpawnedEvent(_stage.Boss.Id));
            }
        }

        private void TickWaves(float dt)
        {
            bool throttled = _enemies.LiveCount >= _stage.MaxLiveEnemies;

            for (int i = 0; i < _stage.Waves.Count; i++)
            {
                var wave = _stage.Waves[i];
                if (_elapsed < wave.StartTime || _elapsed > wave.EndTime) continue;

                float progress = Mathf.InverseLerp(wave.StartTime, wave.EndTime, _elapsed);
                _waveAccumulators[i] += wave.SpawnsPerSecond.Evaluate(progress) * dt;

                // Accumulator pattern: fractional rates spawn correctly over time.
                while (_waveAccumulators[i] >= 1f)
                {
                    _waveAccumulators[i] -= 1f;
                    if (throttled) continue; // burn the token: pressure resumes when count drops, no backlog explosion
                    SpawnOne(wave.Enemy, elite: false);
                }
            }
        }

        private void TickElites()
        {
            while (_nextEliteIndex < _stage.Elites.Count && _stage.Elites[_nextEliteIndex].Time <= _elapsed)
            {
                var injection = _stage.Elites[_nextEliteIndex];
                for (int i = 0; i < injection.Count; i++)
                    SpawnOne(injection.Enemy, elite: true);
                _nextEliteIndex++;
            }
        }

        private void SpawnOne(EnemyDefinition definition, bool elite)
        {
            float hpMul = _stage.EnemyHpMultiplier.Evaluate(_elapsed);
            float dmgMul = _stage.EnemyDamageMultiplier.Evaluate(_elapsed);
            if (elite)
            {
                hpMul *= _stage.EliteHpMultiplier;
                dmgMul *= _stage.EliteDamageMultiplier;
            }

            _enemies.Spawn(definition, RandomRingPosition(), elite, hpMul, dmgMul);
        }

        private Vector3 RandomRingPosition()
        {
            // Uniform angle, lerped radius: cheap and visually even; area-correct sampling
            // is irrelevant for a 3-unit band.
            float angle = (float)_rng.NextDouble() * Mathf.PI * 2f;
            float radius = Mathf.Lerp(_ringInner, _ringOuter, (float)_rng.NextDouble());
            Vector2 center = _player.position;
            return new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, 0f);
        }
    }
}
