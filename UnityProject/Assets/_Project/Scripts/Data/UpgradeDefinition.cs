using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// An in-run PASSIVE upgrade (GDD §5): a stat bonus per level, 5 levels.
    /// Weapons are drafted from WeaponDefinition directly; this SO covers the passive half,
    /// including the evolution-catalyst role (WeaponDefinition.RequiredPassiveId points here).
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Upgrades/Passive Definition", fileName = "SO_Passive_")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Effect (applied once per level)")]
        public StatType Stat = StatType.DamagePercent;
        [Tooltip("True: value added to the percent bucket (0.1 = +10%). False: flat add.")]
        public bool IsPercent = true;
        public float ValuePerLevel = 0.1f;
        [Min(1)] public int MaxLevel = 5;
    }
}
