using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons.Behaviours
{
    /// <summary>
    /// Ground zone (Shock Mine / Quake Field): on Fire, places a zone at a nearby enemy
    /// cluster that pulses damage every PulseInterval for Duration seconds.
    /// Zones are tracked here (not as MonoBehaviour updates) so N zones cost one Tick.
    /// </summary>
    public sealed class AoePulseBehaviour : IWeaponBehaviour
    {
        private const float PulseInterval = 0.5f; // pacing constant; damage-per-second is the balance knob (data)
        private const int MaxZones = 8;

        private struct Zone
        {
            public Vector2 Center;
            public float RemainingDuration;
            public float PulseTimer;
            public GameObject Visual;
            public bool Active;
        }

        private WeaponContext _ctx;
        private WeaponInstance _weapon;
        private readonly Zone[] _zones = new Zone[MaxZones]; // fixed slab: no allocation, no list churn

        public void Equip(WeaponContext context, WeaponInstance instance)
        {
            _ctx = context;
            _weapon = instance;
        }

        public void Fire()
        {
            // Place on the nearest enemy so zones land where the fight is, not on empty floor.
            var target = _ctx.Enemies.FindNearest(_ctx.PlayerPosition, maxRange: 8f);
            Vector2 center = target != null ? target.Position : _ctx.PlayerPosition;

            for (int i = 0; i < _zones.Length; i++)
            {
                if (_zones[i].Active) continue;
                var stats = _weapon.Stats;
                _zones[i] = new Zone
                {
                    Center = center,
                    RemainingDuration = stats.Duration * (1f + _ctx.Player.Stats.Get(StatType.DurationPercent)),
                    PulseTimer = 0f, // first pulse immediately
                    Visual = SpawnVisual(center),
                    Active = true
                };
                return;
            }
            // All slots busy: oldest-slot policy is overkill; skipping a cast is invisible at this cadence.
        }

        public void Tick(float deltaTime)
        {
            var stats = _weapon.Stats;
            float radius = stats.Area * (1f + _ctx.Player.Stats.Get(StatType.AreaPercent));

            for (int i = 0; i < _zones.Length; i++)
            {
                if (!_zones[i].Active) continue;

                _zones[i].RemainingDuration -= deltaTime;
                _zones[i].PulseTimer -= deltaTime;

                if (_zones[i].PulseTimer <= 0f)
                {
                    _zones[i].PulseTimer = PulseInterval;
                    int count = _ctx.Enemies.QueryRadius(_zones[i].Center, radius, _ctx.QueryBuffer);
                    for (int k = 0; k < count; k++)
                        _ctx.Hit(_ctx.QueryBuffer[k], stats.Damage, _zones[i].Center);
                }

                if (_zones[i].RemainingDuration <= 0f)
                {
                    if (_zones[i].Visual != null) _ctx.Pool.Release(_zones[i].Visual);
                    _zones[i].Active = false;
                }
            }
        }

        public void Unequip()
        {
            for (int i = 0; i < _zones.Length; i++)
            {
                if (_zones[i].Active && _zones[i].Visual != null)
                    _ctx.Pool.Release(_zones[i].Visual);
                _zones[i].Active = false;
            }
        }

        private GameObject SpawnVisual(Vector2 center)
        {
            if (_weapon.Definition.EffectPrefab == null) return null;
            var go = _ctx.Pool.Get(_weapon.Definition.EffectPrefab, center, Quaternion.identity);
            go.transform.localScale = Vector3.one * _weapon.Stats.Area;
            return go;
        }
    }
}
