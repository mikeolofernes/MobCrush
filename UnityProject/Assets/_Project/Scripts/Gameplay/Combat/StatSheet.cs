using System;
using System.Collections.Generic;
using MobCrush.Data;

namespace MobCrush.Gameplay.Combat
{
    /// <summary>
    /// Runtime stat container: base values + stacked modifiers from any source
    /// (equipment, talents, in-run passives). Plain C# — fully unit-testable.
    /// Model: final = (base + flatSum) * (1 + percentSum). Simple, predictable,
    /// and communicable to players; no multiplicative stacking surprises (GDD §15.1).
    /// </summary>
    public sealed class StatSheet
    {
        private readonly Dictionary<StatType, float> _base = new();
        private readonly Dictionary<StatType, float> _flat = new();
        private readonly Dictionary<StatType, float> _percent = new();

        /// <summary>Raised when any stat changes; consumers (health bar, move speed) re-read lazily.</summary>
        public event Action<StatType> StatChanged;

        public void SetBase(StatType stat, float value)
        {
            _base[stat] = value;
            StatChanged?.Invoke(stat);
        }

        public void AddFlat(StatType stat, float delta)
        {
            _flat.TryGetValue(stat, out float cur);
            _flat[stat] = cur + delta;
            StatChanged?.Invoke(stat);
        }

        public void AddPercent(StatType stat, float delta)
        {
            _percent.TryGetValue(stat, out float cur);
            _percent[stat] = cur + delta;
            StatChanged?.Invoke(stat);
        }

        public float Get(StatType stat)
        {
            _base.TryGetValue(stat, out float b);
            _flat.TryGetValue(stat, out float f);
            _percent.TryGetValue(stat, out float p);
            return (b + f) * (1f + p);
        }

        /// <summary>Clears all modifiers, keeping bases. Called at run start so meta bonuses re-apply cleanly.</summary>
        public void ClearModifiers()
        {
            _flat.Clear();
            _percent.Clear();
            StatChanged?.Invoke(StatType.MaxHp); // single coarse notification is enough for a full reset
        }
    }
}
