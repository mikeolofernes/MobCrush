using MobCrush.Data;
using MobCrush.Gameplay.Weapons.Behaviours;

namespace MobCrush.Gameplay.Weapons
{
    /// <summary>
    /// Maps WeaponType → a fresh behavior instance. The single place that knows concrete
    /// behavior classes; everything else sees IWeaponBehaviour. Projectile-family types
    /// are wired in Loop 9 (ProjectileBehaviour covers Projectile/Boomerang/Missile via config).
    /// </summary>
    public static class WeaponBehaviourFactory
    {
        public static IWeaponBehaviour Create(WeaponType type) => type switch
        {
            WeaponType.Melee => new MeleeArcBehaviour(),
            WeaponType.AoE => new AoePulseBehaviour(),
            WeaponType.Orbital => new OrbitalBehaviour(),
            WeaponType.Laser => new LaserSweepBehaviour(),
            WeaponType.Projectile => new ProjectileVolleyBehaviour(ProjectileVolleyBehaviour.Mode.Straight),
            WeaponType.Boomerang => new ProjectileVolleyBehaviour(ProjectileVolleyBehaviour.Mode.Boomerang),
            WeaponType.Missile => new ProjectileVolleyBehaviour(ProjectileVolleyBehaviour.Mode.Homing),
            _ => new MeleeArcBehaviour()
        };
    }
}
