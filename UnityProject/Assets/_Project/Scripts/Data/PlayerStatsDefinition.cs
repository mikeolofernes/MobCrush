using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// Designer-owned base stats for the player character (GDD §1).
    /// Read-only at runtime; the mutable StatSheet is built FROM this at run start.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Player/Stats Definition", fileName = "SO_PlayerStats")]
    public sealed class PlayerStatsDefinition : ScriptableObject
    {
        [Header("Survivability")]
        [Min(1f)] public float MaxHp = 100f;
        [Min(0f)] public float HpRegenPerSecond = 0.5f;
        [Min(0f)] public float Armor = 0f;

        [Header("Mobility")]
        [Min(0.1f)] public float MoveSpeed = 5f;

        [Header("Offense")]
        [Range(0f, 1f)] public float CritChance = 0.05f;
        [Min(1f)] public float CritDamage = 1.5f;

        [Header("Utility")]
        [Min(0f)] public float PickupRadius = 1.5f;

        [Header("Damage intake")]
        [Tooltip("Seconds of invincibility vs the SAME source after a hit (GDD §1).")]
        [Min(0f)] public float InvincibilitySeconds = 0.3f;
    }
}
