using MobCrush.Gameplay.Combat;
using MobCrush.Gameplay.Projectiles;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons.Behaviours
{
    /// <summary>
    /// One behavior for the three projectile-family weapon types (Rail Pistol, Boomerang
    /// Disc, Seeker Pod) — they differ only in ProjectileMotion and aim policy, so a mode
    /// switch beats three near-identical classes (DRY without over-abstraction).
    /// Fires a fan of ProjectileCount (+ player bonus) shots at the nearest enemy.
    /// </summary>
    public sealed class ProjectileVolleyBehaviour : IWeaponBehaviour
    {
        public enum Mode { Straight, Boomerang, Homing }

        private const float FanSpreadDeg = 12f; // per-extra-shot angle; feel constant, not balance

        private readonly Mode _mode;
        private WeaponContext _ctx;
        private WeaponInstance _weapon;

        public ProjectileVolleyBehaviour(Mode mode) => _mode = mode;

        public void Equip(WeaponContext context, WeaponInstance instance)
        {
            _ctx = context;
            _weapon = instance;
        }

        public void Fire()
        {
            var stats = _weapon.Stats;
            var prefab = _weapon.Definition.EffectPrefab;
            if (prefab == null) return;

            // Aim: nearest enemy, else facing (never waste a shot straight into nothing when idle-aiming is possible).
            var target = _ctx.Enemies.FindNearest(_ctx.PlayerPosition);
            Vector2 origin = _ctx.PlayerPosition;
            Vector2 aim = target != null ? (target.Position - origin).normalized : _ctx.PlayerFacing;

            var config = ProjectileConfig.Default;
            config.Damage = stats.Damage;
            config.Speed = stats.ProjectileSpeed;
            config.Pierce = stats.Pierce;
            config.Range = stats.Area > 0f ? stats.Area * 4f : config.Range; // boomerang reach scales with Area stat
            config.Motion = _mode switch
            {
                Mode.Boomerang => ProjectileMotion.Boomerang,
                Mode.Homing => ProjectileMotion.Homing,
                _ => ProjectileMotion.Straight
            };
            if (_mode == Mode.Homing)
            {
                // Seeker Pod identity: splash on impact (GDD §4); zone size rides the Area stat.
                config.ExplosionRadius = stats.Area;
                config.ExplosionDamage = stats.Damage * 0.5f;
            }

            int shots = stats.ProjectileCount + (int)_ctx.Player.Stats.Get(StatType.ProjectileCountBonus);
            float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            for (int i = 0; i < shots; i++)
            {
                // Symmetric fan: 0, +s, -s, +2s, -2s...
                int offsetIndex = (i + 1) / 2;
                float sign = (i % 2 == 0) ? 1f : -1f;
                float angle = baseAngle + sign * offsetIndex * FanSpreadDeg;
                var dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                var go = _ctx.Pool.Get(prefab, origin, Quaternion.identity);
                go.RegisterPrefabSource(prefab); // enables pooled split-children (see Projectile.Split)
                go.GetComponent<Projectile>().Launch(_ctx, config, origin, dir);
            }
        }

        public void Tick(float deltaTime) { }
        public void Unequip() { }
    }
}
