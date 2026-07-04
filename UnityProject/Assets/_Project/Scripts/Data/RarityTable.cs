using System;
using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>Draft option rarities (GDD §9). Order matters: index = power tier.</summary>
    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    /// <summary>
    /// Weighted rarity roll table. Luck shifts weight from Common toward the higher tiers
    /// linearly — transparent to players ("Luck +10%" does what it says) and safe to tune.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Upgrades/Rarity Table", fileName = "SO_RarityTable")]
    public sealed class RarityTable : ScriptableObject
    {
        [Serializable]
        public struct Row
        {
            public Rarity Rarity;
            [Min(0f)] public float Weight;      // GDD start: 55/28/13/4
            [Min(1f)] public float ValueMultiplier; // scales passive magnitudes per tier
        }

        public Row[] Rows =
        {
            new() { Rarity = Rarity.Common, Weight = 55f, ValueMultiplier = 1f },
            new() { Rarity = Rarity.Rare, Weight = 28f, ValueMultiplier = 1.5f },
            new() { Rarity = Rarity.Epic, Weight = 13f, ValueMultiplier = 2.25f },
            new() { Rarity = Rarity.Legendary, Weight = 4f, ValueMultiplier = 3.5f }
        };

        /// <summary>Rolls a rarity. luck (player stat, 0 = neutral) drains Common weight into higher tiers.</summary>
        public Rarity Roll(System.Random rng, float luck)
        {
            Span<float> weights = stackalloc float[Rows.Length];
            float total = 0f;
            for (int i = 0; i < Rows.Length; i++)
            {
                float w = Rows[i].Weight;
                if (Rows[i].Rarity == Rarity.Common) w = Mathf.Max(1f, w * (1f - luck));
                else w *= (1f + luck);
                weights[i] = w;
                total += w;
            }

            float roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < Rows.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return Rows[i].Rarity;
            }
            return Rarity.Common;
        }

        public float GetValueMultiplier(Rarity rarity)
        {
            for (int i = 0; i < Rows.Length; i++)
                if (Rows[i].Rarity == rarity) return Rows[i].ValueMultiplier;
            return 1f;
        }
    }
}
