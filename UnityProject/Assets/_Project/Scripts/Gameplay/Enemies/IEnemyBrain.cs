namespace MobCrush.Gameplay.Enemies
{
    /// <summary>
    /// Per-archetype behavior strategy. Brains are STATELESS singletons — all mutable
    /// per-enemy state lives on EnemyController — so spawning an enemy allocates nothing.
    /// </summary>
    public interface IEnemyBrain
    {
        void Tick(EnemyController enemy, float deltaTime);
    }
}
