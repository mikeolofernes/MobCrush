using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Enemies
{
    /// <summary>
    /// One live enemy. Deliberately has NO Update(): EnemySystem ticks all enemies from a
    /// single loop (hundreds of Update() calls is a top mobile CPU sink — Loop 0 risk #1).
    /// Holds the mutable state its stateless brain manipulates (TimerA/B, FlagA).
    /// Pooled via IPoolable; damage via IDamageable.
    /// </summary>
    public sealed class EnemyController : MonoBehaviour, ITargetable, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator; // optional

        private static readonly int MovingParam = Animator.StringToHash("Moving");

        // Injected per-spawn by EnemySystem.
        public EnemyDefinition Definition { get; private set; }
        private EnemySystem _system;
        private IEnemyBrain _brain;
        private Transform _playerTransform;
        private IDamageable _playerDamageable;
        private bool _isElite;
        private float _hp;
        private float _contactTimer;

        // Scratch state for brains (meaningless names on purpose: semantics belong to the brain).
        public float TimerA;
        public float TimerB;
        public bool FlagA;

        public bool IsAlive => _hp > 0f;
        public Vector2 Position => transform.position;
        public Vector2 PlayerPosition => _playerTransform.position;
        public float DistanceToPlayerSqr => ((Vector2)_playerTransform.position - (Vector2)transform.position).sqrMagnitude;
        public float CurrentHp => _hp;

        /// <summary>Full per-spawn setup. Elite scaling applied here so brains/weapons never special-case elites.</summary>
        public void Initialize(EnemyDefinition definition, EnemySystem system, Transform player,
                               IDamageable playerDamageable, bool isElite, float hpMultiplier, float damageMultiplier)
        {
            Definition = definition;
            _system = system;
            _playerTransform = player;
            _playerDamageable = playerDamageable;
            _isElite = isElite;
            _hp = definition.MaxHp * hpMultiplier;
            _damageMultiplier = damageMultiplier;
            _brain = SelectBrain(definition.Behavior);
            transform.localScale = isElite ? Vector3.one * 1.4f : Vector3.one;
        }

        private float _damageMultiplier = 1f;

        /// <summary>Called by EnemySystem every frame. Brain moves; contact damage handled here (shared by all archetypes).</summary>
        public void Tick(float dt)
        {
            if (!IsAlive) return;

            _brain.Tick(this, dt);

            _contactTimer -= dt;
            if (_contactTimer <= 0f && DistanceToPlayerSqr <= Definition.ContactRange * Definition.ContactRange)
            {
                DamagePlayer(new DamageInfo(Definition.ContactDamage * _damageMultiplier, false, Position));
                _contactTimer = Definition.ContactCooldown;
            }
        }

        // --- Movement helpers used by brains (transform-based: enemies don't need physics, colliders serve weapon hits only) ---
        public void MoveTowards(Vector2 target, float speed, float dt)
        {
            Vector2 pos = transform.position;
            Vector2 dir = target - pos;
            float sqr = dir.sqrMagnitude;
            if (sqr < 0.0001f) return;
            dir *= 1f / Mathf.Sqrt(sqr); // manual normalize: one sqrt, no Vector2.Normalize branch
            transform.position = pos + dir * (speed * dt);
            UpdateFacing(dir);
        }

        public void MoveAway(Vector2 from, float speed, float dt)
        {
            Vector2 pos = transform.position;
            Vector2 dir = pos - from;
            float sqr = dir.sqrMagnitude;
            if (sqr < 0.0001f) return;
            dir *= 1f / Mathf.Sqrt(sqr);
            transform.position = pos + dir * (speed * dt);
            UpdateFacing(dir);
        }

        private void UpdateFacing(Vector2 dir)
        {
            if (_spriteRenderer != null) _spriteRenderer.flipX = dir.x < 0f;
            if (_animator != null) _animator.SetBool(MovingParam, true);
        }

        // --- Combat ---
        public void DamagePlayer(in DamageInfo info) => _playerDamageable?.TakeDamage(info);

        public void FireProjectileAtPlayer() => _system.FireEnemyProjectile(this);

        public void OnTelegraphStarted()
        {
            // Placeholder telegraph: tint red. Loop 19 replaces with proper VFX.
            if (_spriteRenderer != null) _spriteRenderer.color = Color.red;
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (!IsAlive) return;
            _hp -= damage.Amount;
            _system.NotifyDamageDealt(damage.Amount, damage.Critical, transform.position);
            if (_hp <= 0f) Die();
        }

        /// <summary>Exploder self-destruct: despawns without kill rewards (it spent itself).</summary>
        public void Explode()
        {
            _hp = 0f;
            _system.Despawn(this, giveRewards: false);
        }

        private void Die() => _system.Despawn(this, giveRewards: true);

        public bool IsElite => _isElite;

        // --- IPoolable: hard state reset so reuse == fresh instance ---
        public void OnSpawned()
        {
            TimerA = 0f; TimerB = 0f; FlagA = false;
            _contactTimer = 0f;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
        }

        public void OnDespawned() { }

        private static IEnemyBrain SelectBrain(EnemyBehavior behavior) => behavior switch
        {
            EnemyBehavior.Sprinter => SprinterBrain.Instance,
            EnemyBehavior.Ranged => RangedBrain.Instance,
            EnemyBehavior.Exploder => ExploderBrain.Instance,
            _ => ChaserBrain.Instance
        };
    }
}
