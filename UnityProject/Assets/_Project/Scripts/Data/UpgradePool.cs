using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// Everything draftable in a stage (GDD §9). Per-stage pools let chapters introduce
    /// weapons gradually; the drafter never scans the whole project for assets.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Upgrades/Upgrade Pool", fileName = "SO_UpgradePool")]
    public sealed class UpgradePool : ScriptableObject
    {
        public List<WeaponDefinition> Weapons = new();
        public List<UpgradeDefinition> Passives = new();
        public RarityTable RarityTable;
        [Min(2)] public int OptionsPerDraft = 3;
    }
}
