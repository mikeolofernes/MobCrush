using MobCrush.Core.Services;
using UnityEngine;

namespace MobCrush.Core.Pooling
{
    /// <summary>
    /// Put on short-lived pooled VFX (swing arcs, beams, hit flashes): returns itself to
    /// the pool after Seconds. Replaces every "Destroy(go, t)" — which would break pooling.
    /// </summary>
    public sealed class TimedPoolReturn : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _seconds = 0.5f;

        private float _remaining;
        private IPoolService _pool;

        public void OnSpawned()
        {
            _remaining = _seconds;
            _pool ??= ServiceLocator.Get<IPoolService>(); // cached; locator touched once per instance lifetime
        }

        public void OnDespawned() { }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f && _pool != null)
                _pool.Release(gameObject);
        }
    }
}
