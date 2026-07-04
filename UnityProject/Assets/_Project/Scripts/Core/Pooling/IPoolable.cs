namespace MobCrush.Core.Pooling
{
    /// <summary>
    /// Lifecycle hooks for pooled components (Loop 0 §5 rule 5: everything spawned
    /// repeatedly must pool). Implementors reset ALL mutable state in OnSpawned —
    /// a pooled object must be indistinguishable from a fresh Instantiate.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called right after the instance is taken from the pool and activated.</summary>
        void OnSpawned();

        /// <summary>Called right before the instance is deactivated and returned.</summary>
        void OnDespawned();
    }
}
