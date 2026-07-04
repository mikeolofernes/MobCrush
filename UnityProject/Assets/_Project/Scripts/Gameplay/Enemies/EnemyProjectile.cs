using MobCrush.Core.Pooling;
using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Enemies
{
    /// <summary>
    /// Minimal enemy shot (Spitter): straight line, hits the player by distance check,
    /// despawns on hit or after MaxLifetime. Distinct from the player projectile framework
    /// (Loop 9) on purpose — enemy shots need none of its modifier stack, and coupling them
    /// would drag player-upgrade complexity into enemy balance.
    /// </summary>
    public sealed class EnemyProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _hitRadius = 0.4f;
        [SerializeField] private float _maxLifetime = 6f;

        private Vector2 _velocity;
        private float _damage;
        private IDamageable _target;
        private Transform _targetTransform;
        private IPoolService _pool;
        private float _age;

        public void Launch(Vector2 direction, float speed, float damage, IDamageable target, IPoolService pool)
        {
            _velocity = direction * speed;
            _damage = damage;
            _target = target;
            _targetTransform = (target as MonoBehaviour)?.transform;
            _pool = pool;
            _age = 0f;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age > _maxLifetime) { _pool.Release(gameObject); return; }

            transform.position += (Vector3)(_velocity * Time.deltaTime);

            if (_targetTransform == null) return;
            Vector2 toTarget = (Vector2)_targetTransform.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude <= _hitRadius * _hitRadius)
            {
                _target.TakeDamage(new DamageInfo(_damage, false, transform.position));
                _pool.Release(gameObject);
            }
        }

        public void OnSpawned() { _age = 0f; }
        public void OnDespawned() { _velocity = Vector2.zero; _target = null; _targetTransform = null; }
    }
}
