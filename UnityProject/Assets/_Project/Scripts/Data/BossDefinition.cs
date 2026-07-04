using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>Attack patterns a boss phase can run (GDD §3). Executed by Loop 13's phase runner.</summary>
    public enum BossPattern
    {
        AimedVolley,   // burst of projectiles at the player
        RadialBurst,   // ring of projectiles
        Charge,        // telegraphed dash through the player's position
        SummonMinions, // spawn adds from a referenced EnemyDefinition
        Idle           // recovery window (the player's damage window)
    }

    /// <summary>
    /// A boss = ordered phases triggered by HP thresholds; each phase loops a pattern
    /// sequence. Fully data-driven: new bosses are assets remixing the pattern vocabulary.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Bosses/Boss Definition", fileName = "SO_Boss_")]
    public sealed class BossDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class PatternStep
        {
            public BossPattern Pattern;
            [Min(0.1f)] public float Duration = 2f;
            [Tooltip("Telegraph time before the pattern's damage becomes active.")]
            [Min(0f)] public float Telegraph = 0.6f; // GDD §3: telegraphs ≥ 0.6s
            [Min(0f)] public float Damage = 15f;
            [Min(1)] public int ProjectileCount = 8;
            [Min(0f)] public float ProjectileSpeed = 5f;
            public EnemyDefinition SummonEnemy;
            [Min(0)] public int SummonCount = 4;
        }

        [Serializable]
        public sealed class Phase
        {
            [Tooltip("Phase activates when HP fraction drops to this value (1.0 = from start).")]
            [Range(0f, 1f)] public float HpThreshold = 1f;
            public List<PatternStep> Steps = new();
            [Tooltip("Soft enrage: +damage fraction per 10s in this phase (GDD §3).")]
            [Min(0f)] public float EnrageDamagePer10s = 0.1f;
        }

        [Header("Identity")]
        public string Id;
        public string DisplayName;
        public GameObject Prefab;

        [Header("Stats")]
        [Min(1f)] public float MaxHp = 5000f;
        [Min(0f)] public float MoveSpeed = 1.5f;
        [Min(0f)] public float ContactDamage = 20f;

        [Header("Phases (order by descending HpThreshold; GDD start: 1.0 / 0.6 / 0.25)")]
        public List<Phase> Phases = new();

        [Header("Attacks")]
        public GameObject ProjectilePrefab;

        [Header("Rewards (GDD §3)")]
        [Min(0)] public int CoinReward = 500;
        [Min(0)] public int GemFirstClearReward = 50;
    }
}
