using System;
using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>The seven delivery mechanisms from GDD §4. Selects the behavior strategy; everything else is data.</summary>
    public enum WeaponType
    {
        Melee,       // arc swing in facing direction
        Projectile,  // straight pooled shots
        Laser,       // sweeping instant beam
        AoE,         // pulsing zone at/near player
        Orbital,     // bodies circling the player
        Boomerang,   // out-and-back pierce
        Missile      // homing with splash
    }

    /// <summary>
    /// One weapon, all levels, one asset (GDD §4). Per-level rows keep balance edits in
    /// one inspector table; evolution is declared here so the draft system (Loop 11)
    /// can discover pairs without a separate registry.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Weapons/Weapon Definition", fileName = "SO_Weapon_")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class LevelStats
        {
            [Min(0f)] public float Damage = 10f;
            [Tooltip("Seconds between activations before attack-speed modifiers.")]
            [Min(0.05f)] public float Cooldown = 1.5f;
            [Tooltip("Radius / arc size / beam width depending on type.")]
            [Min(0f)] public float Area = 1f;
            [Min(1)] public int ProjectileCount = 1;
            [Min(0f)] public float ProjectileSpeed = 10f;
            [Tooltip("Effect duration (orbitals/zones/lasers).")]
            [Min(0f)] public float Duration = 2f;
            [Min(0)] public int Pierce = 0;
            [Min(0f)] public float Knockback = 0f;
        }

        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public WeaponType Type = WeaponType.Projectile;

        [Header("Per-level stats (index 0 = Lv1). GDD: 5 levels, evolution at max.")]
        public LevelStats[] Levels = new LevelStats[5];

        [Header("Visuals")]
        [Tooltip("Pooled prefab: projectile, orbital body, zone or beam segment depending on type.")]
        public GameObject EffectPrefab;
        public string FireSfxId;

        [Header("Evolution (GDD §4)")]
        [Tooltip("Passive (UpgradeDefinition id) required to evolve; empty = cannot evolve.")]
        public string RequiredPassiveId;
        [Tooltip("The evolved WeaponDefinition that replaces this one.")]
        public WeaponDefinition EvolvedForm;

        public int MaxLevel => Levels?.Length ?? 0;

        /// <summary>Clamped accessor: a weapon past max level (evolution pending) keeps max stats.</summary>
        public LevelStats GetLevel(int level) =>
            Levels[Mathf.Clamp(level - 1, 0, Levels.Length - 1)];
    }
}
