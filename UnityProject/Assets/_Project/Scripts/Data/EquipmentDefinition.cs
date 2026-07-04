using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>The six gear slots (GDD §7). Main stat is fixed per slot — legible builds.</summary>
    public enum EquipmentSlot
    {
        Weapon,   // ATK (DamagePercent)
        Armor,    // MaxHp
        Gloves,   // CritChance
        Boots,    // MoveSpeed
        Necklace, // XpGainPercent
        Ring      // CritDamage
    }

    /// <summary>Meta gear rarity ladder (GDD §7) — deliberately separate from the in-run draft Rarity.</summary>
    public enum EquipmentRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Mythic = 4
    }

    /// <summary>
    /// One equipment archetype. An owned item = DefinitionId + rarity + enhancement
    /// (SaveModel.OwnedEquipment); all math derives from this asset + EconomyConfig,
    /// so drops serialize as three small fields.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Equipment/Equipment Definition", fileName = "SO_Equip_")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public EquipmentSlot Slot;

        [Header("Main stat at Common +0 (stat type is implied by Slot)")]
        public float MainStatBase = 10f;

        [Tooltip("True if MainStatBase is a percent-bucket value (0.05 = +5%).")]
        public bool MainStatIsPercent;

        /// <summary>Slot → StatType mapping (GDD §7). One source of truth for gear stat routing.</summary>
        public StatType MainStat => Slot switch
        {
            EquipmentSlot.Weapon => StatType.DamagePercent,
            EquipmentSlot.Armor => StatType.MaxHp,
            EquipmentSlot.Gloves => StatType.CritChance,
            EquipmentSlot.Boots => StatType.MoveSpeed,
            EquipmentSlot.Necklace => StatType.XpGainPercent,
            _ => StatType.CritDamage
        };
    }
}
