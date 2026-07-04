using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// All meta-economy tuning in one asset (GDD §7/§8): rarity power ladder,
    /// enhancement costs/gains, fusion rules. One file to audit when the economy sim
    /// (Loop 22) or LiveOps (Loop 23) needs a knob.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Economy/Economy Config", fileName = "SO_EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Header("Rarity stat multipliers (index = EquipmentRarity; GDD: 1/1.6/2.6/4.2/7)")]
        public float[] RarityMultipliers = { 1f, 1.6f, 2.6f, 4.2f, 7f };

        [Header("Enhancement (GDD §7: +1..+30, exponential coins, no failure)")]
        [Min(1)] public int MaxEnhancement = 30;
        [Tooltip("Main stat bonus per enhancement level, as a fraction of the rarity-scaled base.")]
        [Min(0f)] public float EnhanceStatPerLevel = 0.08f;
        [Min(1)] public long EnhanceBaseCost = 100;
        [Tooltip("Cost = BaseCost × Growth^level (exponential sink).")]
        [Min(1f)] public float EnhanceCostGrowth = 1.18f;

        [Header("Run rewards (granted by RunRewardGranter on RunEndedEvent)")]
        [Min(0f)] public float RunCoinsPerKill = 1f;
        [Min(0)] public long VictoryCoinBonus = 500;
        [Min(0)] public long VictoryCoreReward = 5;
        [Tooltip("Chance the victory equipment drop rolls Rare instead of Common.")]
        [Range(0f, 1f)] public float VictoryRareDropChance = 0.25f;

        [Header("Fusion (GDD §7: 3 identical-rarity same-slot → next rarity)")]
        [Min(2)] public int FusionInputCount = 3;
        [Tooltip("Cores charged per fusion, scaled by target rarity index.")]
        [Min(0)] public long FusionCoreCostPerRarity = 10;
        [Tooltip("Rarity index (Epic=2) from which the best input's enhancement is inherited.")]
        [Min(0)] public int KeepEnhancementFromRarity = 2;

        public float GetRarityMultiplier(EquipmentRarity rarity)
        {
            int i = Mathf.Clamp((int)rarity, 0, RarityMultipliers.Length - 1);
            return RarityMultipliers[i];
        }

        public long GetEnhanceCost(int currentLevel) =>
            (long)(EnhanceBaseCost * Mathf.Pow(EnhanceCostGrowth, currentLevel));
    }
}
