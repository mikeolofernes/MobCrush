using MobCrush.Gameplay.Weapons;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons.Behaviours
{
    /// <summary>
    /// Melee swing (Pulse Blade): damages every enemy inside an arc facing the
    /// player's movement direction. Area stat = radius. Hitbox is a radius query +
    /// dot-product cone test — no physics casts, no allocations.
    /// </summary>
    public sealed class MeleeArcBehaviour : IWeaponBehaviour
    {
        private WeaponContext _ctx;
        private WeaponInstance _weapon;

        // cos(75°): a generous ~150° swing arc. Constant of geometry, not balance (balance = Area/Damage).
        private const float ArcCosine = 0.2588f;

        public void Equip(WeaponContext context, WeaponInstance instance)
        {
            _ctx = context;
            _weapon = instance;
        }

        public void Fire()
        {
            var stats = _weapon.Stats;
            float radius = stats.Area * (1f + _ctx.Player.Stats.Get(Combat.StatType.AreaPercent));
            Vector2 origin = _ctx.PlayerPosition;
            Vector2 facing = _ctx.PlayerFacing;

            int count = _ctx.Enemies.QueryRadius(origin, radius, _ctx.QueryBuffer);
            for (int i = 0; i < count; i++)
            {
                var enemy = _ctx.QueryBuffer[i];
                Vector2 to = (enemy.Position - origin).normalized;
                if (Vector2.Dot(to, facing) >= ArcCosine)
                    _ctx.Hit(enemy, stats.Damage, origin);
            }

            SpawnSwingVfx(origin, facing, radius);
        }

        public void Tick(float deltaTime) { }
        public void Unequip() { }

        private void SpawnSwingVfx(Vector2 origin, Vector2 facing, float radius)
        {
            if (_weapon.Definition.EffectPrefab == null) return;
            float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            var go = _ctx.Pool.Get(_weapon.Definition.EffectPrefab, origin, Quaternion.Euler(0f, 0f, angle));
            go.transform.localScale = Vector3.one * radius;
            // VFX prefab carries its own TimedPoolReturn (Loop 9 utility) to self-release.
        }
    }
}
