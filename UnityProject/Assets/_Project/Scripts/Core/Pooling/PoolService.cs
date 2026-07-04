using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Core.Pooling
{
    /// <summary>
    /// Default <see cref="IPoolService"/>. One stack per prefab; live instances remember
    /// their origin pool via a lookup table (no extra component required on prefabs).
    /// why stacks: LIFO reuse keeps recently-touched objects hot in cache.
    /// </summary>
    public sealed class PoolService : IPoolService
    {
        private sealed class Pool
        {
            public readonly Stack<GameObject> Inactive = new();
            public GameObject Prefab;
        }

        private readonly Dictionary<GameObject, Pool> _poolsByPrefab = new();
        private readonly Dictionary<GameObject, Pool> _poolByInstance = new();
        // HashSet: Release is called per despawn (hundreds/minute); List.Remove was O(n).
        private readonly HashSet<GameObject> _liveInstances = new();
        private readonly Transform _root;

        public PoolService(Transform poolRoot)
        {
            // why: a dedicated inactive-parent keeps the hierarchy readable and avoids
            // reparenting churn under moving objects.
            _root = poolRoot;
        }

        public void WarmUp(GameObject prefab, int count)
        {
            var pool = GetOrCreatePool(prefab);
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance(pool);
                instance.SetActive(false);
                pool.Inactive.Push(instance);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(prefab);
            GameObject instance = pool.Inactive.Count > 0 ? pool.Inactive.Pop() : CreateInstance(pool);

            var t = instance.transform;
            t.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            _liveInstances.Add(instance);

            // why GetComponents into a shared buffer would be premature here: spawn is not per-frame
            // per-object; a single cached call at spawn time is within budget.
            if (instance.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnSpawned();
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (!_poolByInstance.TryGetValue(instance, out var pool))
            {
                Debug.LogWarning($"PoolService.Release: '{instance.name}' was not created by the pool. Destroying.", instance);
                Object.Destroy(instance);
                return;
            }

            if (instance.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnDespawned();

            instance.SetActive(false);
            instance.transform.SetParent(_root, false);
            pool.Inactive.Push(instance);
            _liveInstances.Remove(instance);
        }

        public void Clear()
        {
            // Release live instances first so IPoolable teardown runs.
            // Copy: Release mutates the set. One allocation at teardown is fine.
            var live = new List<GameObject>(_liveInstances);
            for (int i = 0; i < live.Count; i++)
                Release(live[i]);

            foreach (var pair in _poolsByPrefab)
            {
                while (pair.Value.Inactive.Count > 0)
                {
                    var go = pair.Value.Inactive.Pop();
                    _poolByInstance.Remove(go);
                    Object.Destroy(go);
                }
            }
            _poolsByPrefab.Clear();
        }

        private Pool GetOrCreatePool(GameObject prefab)
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new Pool { Prefab = prefab };
                _poolsByPrefab[prefab] = pool;
            }
            return pool;
        }

        private GameObject CreateInstance(Pool pool)
        {
            var instance = Object.Instantiate(pool.Prefab, _root);
            _poolByInstance[instance] = pool;
            return instance;
        }
    }
}
