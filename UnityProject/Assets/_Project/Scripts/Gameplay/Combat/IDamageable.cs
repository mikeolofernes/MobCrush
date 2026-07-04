namespace MobCrush.Gameplay.Combat
{
    /// <summary>
    /// Anything that can take a hit — player, enemies, destructibles. Weapons and
    /// projectiles only ever see this interface, never concrete enemy/player types.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(in DamageInfo damage);
    }
}
