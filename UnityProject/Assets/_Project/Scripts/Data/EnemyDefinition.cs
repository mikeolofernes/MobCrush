using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>Behavior archetypes from GDD §2. The brain strategy is selected from this enum.</summary>
    public enum EnemyBehavior
    {
        Chaser,    // Crawler/Bulwark/Swarmling: walk at player
        Sprinter,  // fast approach + lunge
        Ranged,    // Spitter: hold distance, fire projectile
        Exploder   // run in, telegraph, detonate
    }

    /// <summary>
    /// All designer-tunable data for one enemy type (GDD §2). Multiple archetypes are
    /// pure data variations — no per-enemy-type code.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Enemies/Enemy Definition", fileName = "SO_Enemy_")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public GameObject Prefab;
        public EnemyBehavior Behavior = EnemyBehavior.Chaser;

        [Header("Stats")]
        [Min(1f)] public float MaxHp = 10f;
        [Min(0f)] public float ContactDamage = 5f;
        [Min(0.1f)] public float MoveSpeed = 2f;
        [Tooltip("Seconds between contact damage ticks against the player (GDD §1).")]
        [Min(0.1f)] public float ContactCooldown = 0.5f;
        [Min(0f)] public float ContactRange = 0.6f;

        [Header("Rewards")]
        [Min(0f)] public float XpValue = 1f;
        [Min(0f)] public float CoinDropChance = 0.05f;

        [Header("Ranged (Spitter only)")]
        public GameObject ProjectilePrefab;
        [Min(0.5f)] public float PreferredRange = 5f;
        [Min(0.1f)] public float FireCooldown = 2.5f;
        [Min(0.1f)] public float ProjectileSpeed = 4f;

        [Header("Sprinter only")]
        [Min(0f)] public float LungeRange = 3f;
        [Min(1f)] public float LungeSpeedMultiplier = 3f;
        [Min(0.1f)] public float LungeCooldown = 2f;

        [Header("Exploder only")]
        [Min(0.1f)] public float DetonateRange = 1.2f;
        [Min(0.1f)] public float TelegraphSeconds = 0.8f;
        [Min(0f)] public float ExplosionRadius = 2f;
        [Min(0f)] public float ExplosionDamage = 20f;
    }
}
