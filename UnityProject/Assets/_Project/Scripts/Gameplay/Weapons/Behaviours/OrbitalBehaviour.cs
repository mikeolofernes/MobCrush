using System.Collections.Generic;
using MobCrush.Gameplay.Combat;
using UnityEngine;
using MobCrush.Data;

namespace MobCrush.Gameplay.Weapons.Behaviours
{
    /// <summary>
    /// Orbit Drones: ProjectileCount bodies circle the player; touching enemies take damage
    /// with a short per-enemy re-hit lockout so a slow pass doesn't multi-tick the same target.
    /// Continuous weapon: Fire() no-ops; body count/stats refresh on level change.
    /// </summary>
    public sealed class OrbitalBehaviour : IWeaponBehaviour
    {
        private const float RehitLockout = 0.5f;
        private const float AngularSpeedDeg = 180f; // rotation feel; damage/radius are the data knobs

        private WeaponContext _ctx;
        private WeaponInstance _weapon;
        private readonly List<Transform> _bodies = new(8);
        private readonly Dictionary<Enemies.EnemyController, float> _lockouts = new(64);
        private float _angleDeg;
        private int _builtForLevel = -1;

        public void Equip(WeaponContext context, WeaponInstance instance)
        {
            _ctx = context;
            _weapon = instance;
            RebuildBodies();
        }

        public void Fire() { } // continuous

        public void Tick(float deltaTime)
        {
            if (_builtForLevel != _weapon.Level) RebuildBodies(); // level-up may add bodies

            var stats = _weapon.Stats;
            float orbitRadius = stats.Area * (1f + _ctx.Player.Stats.Get(StatType.AreaPercent));
            _angleDeg += AngularSpeedDeg * deltaTime;

            Vector2 center = _ctx.PlayerPosition;
            float step = 360f / Mathf.Max(1, _bodies.Count);

            for (int i = 0; i < _bodies.Count; i++)
            {
                float rad = (_angleDeg + step * i) * Mathf.Deg2Rad;
                Vector2 pos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
                _bodies[i].position = pos;

                // Contact check per body: small radius query around the body.
                int count = _ctx.Enemies.QueryRadius(pos, 0.5f, _ctx.QueryBuffer);
                for (int k = 0; k < count; k++)
                {
                    var enemy = _ctx.QueryBuffer[k];
                    if (_lockouts.TryGetValue(enemy, out float until) && Time.time < until) continue;
                    _lockouts[enemy] = Time.time + RehitLockout;
                    _ctx.Hit(enemy, stats.Damage, pos);
                }
            }

            // Periodic lockout cleanup keeps the dictionary from growing all run.
            if (_lockouts.Count > 128) _lockouts.Clear();
        }

        public void Unequip()
        {
            for (int i = 0; i < _bodies.Count; i++)
                if (_bodies[i] != null) _ctx.Pool.Release(_bodies[i].gameObject);
            _bodies.Clear();
            _lockouts.Clear();
            _builtForLevel = -1;
        }

        private void RebuildBodies()
        {
            Unequip();
            int desired = _weapon.Stats.ProjectileCount + (int)_ctx.Player.Stats.Get(StatType.ProjectileCountBonus);
            for (int i = 0; i < desired; i++)
            {
                var go = _ctx.Pool.Get(_weapon.Definition.EffectPrefab, _ctx.PlayerPosition, Quaternion.identity);
                _bodies.Add(go.transform);
            }
            _builtForLevel = _weapon.Level;
        }
    }
}
