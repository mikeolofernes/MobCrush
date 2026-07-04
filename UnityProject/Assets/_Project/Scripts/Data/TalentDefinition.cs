using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// One node of the permanent talent tree (GDD meta progression). Tree shape emerges
    /// from Prerequisite links — no separate graph asset to keep in sync.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Progression/Talent Definition", fileName = "SO_Talent_")]
    public sealed class TalentDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        [Tooltip("Empty = root node. Otherwise this talent needs the prerequisite at level ≥ 1.")]
        public string PrerequisiteId;

        [Header("Effect per level")]
        public StatType Stat = StatType.MaxHp;
        public bool IsPercent;
        public float ValuePerLevel = 5f;
        [Min(1)] public int MaxLevel = 10;

        [Header("Cost (coins): Base × Growth^currentLevel — same sink shape as enhancement")]
        [Min(1)] public long BaseCost = 200;
        [Min(1f)] public float CostGrowth = 1.35f;

        public long GetCost(int currentLevel) => (long)(BaseCost * Mathf.Pow(CostGrowth, currentLevel));
    }
}
