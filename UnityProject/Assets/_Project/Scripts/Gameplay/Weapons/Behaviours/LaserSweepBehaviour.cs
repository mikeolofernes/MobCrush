using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons.Behaviours
{
    /// <summary>
    /// Arc Laser: on Fire, an instant beam toward the nearest enemy damaging everything in a
    /// line corridor (width = Area). Hit test = point-to-segment distance over live enemies:
    /// cheaper and more reliable than physics raycasts against hundreds of trigger colliders.
    /// </summary>
    public sealed class LaserSweepBehaviour : IWeaponBehaviour
    {
        private const float BeamLength = 12f; // reaches past any phone screen edge; damage/width are data knobs

        private WeaponContext _ctx;
        private WeaponInstance _weapon;

        public void Equip(WeaponContext context, WeaponInstance instance)
        {
            _ctx = context;
            _weapon = instance;
        }

        public void Fire()
        {
            var target = _ctx.Enemies.FindNearest(_ctx.PlayerPosition);
            Vector2 origin = _ctx.PlayerPosition;
            Vector2 dir = target != null ? (target.Position - origin).normalized : _ctx.PlayerFacing;

            var stats = _weapon.Stats;
            float halfWidth = stats.Area * 0.5f * (1f + _ctx.Player.Stats.Get(StatType.AreaPercent));
            Vector2 end = origin + dir * BeamLength;

            // Broad-phase: circle around the beam midpoint; narrow-phase: segment distance.
            int count = _ctx.Enemies.QueryRadius((origin + end) * 0.5f, BeamLength * 0.5f + halfWidth, _ctx.QueryBuffer);
            for (int i = 0; i < count; i++)
            {
                var enemy = _ctx.QueryBuffer[i];
                if (DistanceToSegmentSqr(enemy.Position, origin, end) <= halfWidth * halfWidth)
                    _ctx.Hit(enemy, stats.Damage, origin);
            }

            SpawnBeamVfx(origin, dir, halfWidth);
        }

        public void Tick(float deltaTime) { }
        public void Unequip() { }

        private static float DistanceToSegmentSqr(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            Vector2 closest = a + ab * t;
            return (p - closest).sqrMagnitude;
        }

        private void SpawnBeamVfx(Vector2 origin, Vector2 dir, float halfWidth)
        {
            if (_weapon.Definition.EffectPrefab == null) return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var go = _ctx.Pool.Get(_weapon.Definition.EffectPrefab, origin, Quaternion.Euler(0f, 0f, angle));
            // Beam prefab is authored 1 unit long/wide and stretched here; self-releases via TimedPoolReturn.
            go.transform.localScale = new Vector3(BeamLength, halfWidth * 2f, 1f);
        }
    }
}
