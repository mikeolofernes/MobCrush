using UnityEngine;

namespace MobCrush.Core.Pooling
{
    /// <summary>
    /// Central prefab pool. Pools are keyed by the prefab reference itself so callers
    /// never invent string ids that can drift out of sync with assets.
    /// </summary>
    public interface IPoolService
    {
        /// <summary>Pre-instantiates <paramref name="count"/> inactive copies. Call during loading, never mid-run.</summary>
        void WarmUp(GameObject prefab, int count);

        /// <summary>Takes an instance (creating one only if the pool is empty) and activates it at the given pose.</summary>
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);

        /// <summary>Returns an instance to its pool. Destroys it with a warning if it was never pooled.</summary>
        void Release(GameObject instance);

        /// <summary>Despawns every live instance and clears warm-up stock. Called on run teardown.</summary>
        void Clear();
    }
}
