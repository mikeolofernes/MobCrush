using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Data
{
    /// <summary>
    /// One stage/chapter (GDD §10 + §12 behaviors): the full spawn script, difficulty
    /// scaling curves, elite injections and the boss. Designers author pressure over time
    /// entirely here; the spawner is a dumb executor.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Stages/Stage Definition", fileName = "SO_Stage_")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class Wave
        {
            public EnemyDefinition Enemy;
            [Tooltip("Active window in seconds since stage start.")]
            public float StartTime;
            public float EndTime = 60f;
            [Tooltip("Spawns/second across the window; curve time 0..1 = window progress.")]
            public AnimationCurve SpawnsPerSecond = AnimationCurve.Constant(0f, 1f, 2f);
        }

        [Serializable]
        public sealed class EliteInjection
        {
            public EnemyDefinition Enemy;
            public float Time;
            [Min(1)] public int Count = 1;
        }

        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [Min(30f)] public float DurationSeconds = 900f; // 15 min (GDD §10)

        [Header("Spawn script")]
        public List<Wave> Waves = new();
        public List<EliteInjection> Elites = new();

        [Header("Difficulty scaling over stage time (x = seconds, y = multiplier)")]
        public AnimationCurve EnemyHpMultiplier = AnimationCurve.Linear(0f, 1f, 900f, 3f);
        public AnimationCurve EnemyDamageMultiplier = AnimationCurve.Linear(0f, 1f, 900f, 2f);

        [Header("Elite stat multipliers (GDD §2: HP 8-15x, dmg 2x)")]
        [Min(1f)] public float EliteHpMultiplier = 10f;
        [Min(1f)] public float EliteDamageMultiplier = 2f;

        [Header("Boss")]
        public BossDefinition Boss;

        [Header("Performance")]
        [Tooltip("Hard cap on live enemies; the spawner throttles instead of tanking the frame rate.")]
        [Min(50)] public int MaxLiveEnemies = 250;
        [Tooltip("Pool warm-up: prefab + count, instantiated during loading.")]
        public List<WarmUpEntry> WarmUp = new();

        [Serializable]
        public sealed class WarmUpEntry
        {
            public GameObject Prefab;
            [Min(1)] public int Count = 32;
        }
    }
}
