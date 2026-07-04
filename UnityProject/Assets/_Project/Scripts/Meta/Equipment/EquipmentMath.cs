using MobCrush.Data;

namespace MobCrush.Meta.Equipment
{
    /// <summary>
    /// Pure equipment math (no Unity, no save access — unit-testable):
    /// itemMainStat = base × rarityMultiplier × (1 + enhancePerLevel × enhancementLevel).
    /// One formula shared by tooltips, stat application and the balance simulator,
    /// so the shop card can never disagree with in-run power.
    /// </summary>
    public static class EquipmentMath
    {
        public static float ComputeMainStat(EquipmentDefinition def, EconomyConfig economy,
                                            EquipmentRarity rarity, int enhancementLevel)
        {
            float rarityScaled = def.MainStatBase * economy.GetRarityMultiplier(rarity);
            return rarityScaled * (1f + economy.EnhanceStatPerLevel * enhancementLevel);
        }

        /// <summary>Gear score: comparable single number across slots for stage recommendations (GDD §10).</summary>
        public static int ComputeGearScore(EconomyConfig economy, EquipmentRarity rarity, int enhancementLevel) =>
            (int)(100f * economy.GetRarityMultiplier(rarity) * (1f + economy.EnhanceStatPerLevel * enhancementLevel));

        public static bool CanEnhance(EconomyConfig economy, int currentLevel, long coins) =>
            currentLevel < economy.MaxEnhancement && coins >= economy.GetEnhanceCost(currentLevel);

        public static bool CanFuseRarity(EquipmentRarity rarity) => rarity < EquipmentRarity.Mythic;
    }
}
