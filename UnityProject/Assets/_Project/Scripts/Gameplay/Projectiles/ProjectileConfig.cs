namespace MobCrush.Gameplay.Projectiles
{
    /// <summary>How a projectile travels. Set by the firing weapon; not per-upgrade data.</summary>
    public enum ProjectileMotion
    {
        Straight,
        Boomerang, // out to Range, then returns to the player; despawns on catch
        Homing     // steers toward the nearest living enemy
    }

    /// <summary>
    /// Complete launch recipe for one projectile. Struct (copied at launch) so future
    /// upgrades can tweak a copy per shot without touching weapon data.
    /// All values originate in WeaponDefinition.LevelStats + player stats + upgrade modifiers.
    /// </summary>
    public struct ProjectileConfig
    {
        public float Damage;          // base damage fed to IDamageResolver per hit
        public float Speed;
        public float Lifetime;        // hard despawn safety net
        public float HitRadius;       // contact distance to an enemy center

        public ProjectileMotion Motion;
        public float Range;           // boomerang turnaround distance
        public float HomingTurnSpeed; // deg/sec steering for Homing

        public float Knockback;       // impulse applied to regular enemies per hit (bosses immune)
        public int Pierce;            // enemies passed through after the first hit
        public int Bounce;            // redirects to a new target after a hit
        public int SplitCount;        // children spawned on FIRST hit (children never re-split)
        public float ExplosionRadius; // >0: AoE on final despawn-by-hit
        public float ExplosionDamage;

        public static ProjectileConfig Default => new()
        {
            Damage = 1f,
            Speed = 10f,
            Lifetime = 4f,
            HitRadius = 0.35f,
            Motion = ProjectileMotion.Straight,
            Range = 6f,
            HomingTurnSpeed = 360f,
            Knockback = 0f,
            Pierce = 0,
            Bounce = 0,
            SplitCount = 0,
            ExplosionRadius = 0f,
            ExplosionDamage = 0f
        };
    }
}
