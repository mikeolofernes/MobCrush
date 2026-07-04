using MobCrush.Gameplay.Weapons;
using UnityEngine;
using MobCrush.Data;

namespace MobCrush.Gameplay.Combat
{
    /// <summary>
    /// THE damage formula (GDD §15): final = weaponBase × (1 + DamagePercent), then
    /// crit roll: × CritDamage on success. One implementation, plain C#, unit-tested —
    /// every weapon, orbital tick and explosion routes through here so balance changes
    /// happen in exactly one place.
    /// </summary>
    public sealed class DamageCalculator : IDamageResolver
    {
        private readonly StatSheet _stats;
        private readonly System.Random _rng;

        /// <summary>rng injectable so tests are deterministic.</summary>
        public DamageCalculator(StatSheet stats, System.Random rng = null)
        {
            _stats = stats;
            _rng = rng ?? new System.Random();
        }

        public DamageInfo Resolve(float weaponBaseDamage, Vector2 sourcePosition)
        {
            float amount = weaponBaseDamage * (1f + _stats.Get(StatType.DamagePercent));

            bool crit = _rng.NextDouble() < _stats.Get(StatType.CritChance);
            if (crit)
                amount *= Mathf.Max(1f, _stats.Get(StatType.CritDamage));

            return new DamageInfo(amount, crit, sourcePosition);
        }
    }
}
