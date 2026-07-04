using System;
using System.Collections.Generic;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// Root persistent payload. Plain serializable C# — never a ScriptableObject
    /// (SOs are read-only config; saves are mutable player state, Loop 3 §5).
    /// Add fields freely; REMOVING or RENAMING fields requires an <see cref="ISaveMigration"/>.
    /// </summary>
    [Serializable]
    public sealed class SaveModel
    {
        /// <summary>Schema version. Bump on breaking change and add a migration.</summary>
        public int Version = 1;

        /// <summary>UTC ticks of last write; used for cloud-save conflict resolution and offline progress.</summary>
        public long LastSavedUtcTicks;

        // --- Wallet ---
        public long Coins;
        public long Gems;
        public long Cores;

        // --- Progression ---
        public int HighestChapterCleared;
        public List<string> UnlockedWeaponIds = new();
        public Dictionary<string, int> TalentLevels = new();
        public Dictionary<string, long> LifetimeCounters = new(); // kills, clears, etc. for achievements

        // --- Equipment / inventory (item instances, Loop 14/15) ---
        public List<OwnedEquipment> Inventory = new();
        public Dictionary<string, string> EquippedBySlot = new(); // slot id -> item InstanceId

        // --- Missions ---
        public string DailyMissionDateStamp;          // "yyyy-MM-dd" of current mission set
        public List<MissionProgress> DailyMissions = new();

        [Serializable]
        public sealed class OwnedEquipment
        {
            public string InstanceId;     // GUID string; identity survives sorting/merging
            public string DefinitionId;   // EquipmentDefinition asset id
            public int Rarity;            // index into rarity ladder
            public int EnhancementLevel;
        }

        [Serializable]
        public sealed class MissionProgress
        {
            public string MissionId;
            public int Progress;
            public bool Claimed;
        }
    }
}
