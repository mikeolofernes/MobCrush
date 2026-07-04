using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;

namespace MobCrush.Meta.Progression
{
    /// <summary>
    /// Daily missions (GDD §11 / Loop 23): 5 per local day, coin rewards, all-5 gem bonus.
    /// Mission catalog is code-simple on purpose (id + target + reward): remote config can
    /// later override targets/rewards per id without a data migration. Progress accrues
    /// from gameplay events; state persists in SaveModel with a date stamp for reset.
    /// </summary>
    public sealed class DailyMissionService : IDisposable
    {
        public sealed class MissionSpec
        {
            public string Id;
            public string Description;
            public int Target;
            public long CoinReward;
        }

        // Default catalog (GDD §11). Remote-config overridable by id at fetch time.
        private static readonly MissionSpec[] Catalog =
        {
            new() { Id = "kill_300", Description = "Destroy 300 enemies", Target = 300, CoinReward = 200 },
            new() { Id = "clear_stage", Description = "Clear any stage", Target = 1, CoinReward = 300 },
            new() { Id = "enhance_once", Description = "Enhance any equipment", Target = 1, CoinReward = 150 },
            new() { Id = "level_15", Description = "Reach level 15 in a run", Target = 1, CoinReward = 200 },
            new() { Id = "kill_elite_3", Description = "Destroy 3 elites", Target = 3, CoinReward = 250 }
        };

        private const long AllClearGemBonus = 20;

        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly MetaProgressionService _wallet;

        public DailyMissionService(ISaveService save, IEventBus events, MetaProgressionService wallet)
        {
            _save = save;
            _events = events;
            _wallet = wallet;

            EnsureTodaySet(DateTime.Now);

            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Subscribe<RunEndedEvent>(OnRunEnded);
            _events.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);
        }

        /// <summary>Unhook from the bus. Required if the service is ever rebuilt (save-slot switch) —
        /// otherwise the orphaned instance keeps accruing mission progress twice.</summary>
        public void Dispose()
        {
            _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Unsubscribe<RunEndedEvent>(OnRunEnded);
            _events.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);
        }

        public IReadOnlyList<SaveModel.MissionProgress> Missions => _save.Data.DailyMissions;

        public MissionSpec GetSpec(string missionId)
        {
            foreach (var spec in Catalog)
                if (spec.Id == missionId) return spec;
            return null;
        }

        /// <summary>Local-midnight reset (GDD §11). Call at boot and on app focus.</summary>
        public void EnsureTodaySet(DateTime now)
        {
            string stamp = now.ToString("yyyy-MM-dd");
            if (_save.Data.DailyMissionDateStamp == stamp) return;

            _save.Data.DailyMissionDateStamp = stamp;
            _save.Data.DailyMissions.Clear();
            foreach (var spec in Catalog)
                _save.Data.DailyMissions.Add(new SaveModel.MissionProgress { MissionId = spec.Id });
            _save.Save();
        }

        public bool TryClaim(string missionId)
        {
            var mission = Find(missionId);
            var spec = GetSpec(missionId);
            if (mission == null || spec == null || mission.Claimed || mission.Progress < spec.Target) return false;

            mission.Claimed = true;
            _wallet.AddCurrency("coins", spec.CoinReward); // saves + publishes

            if (AllClaimed())
                _wallet.AddCurrency("gems", AllClearGemBonus);
            return true;
        }

        // --- Event-driven progress ---

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            Bump("kill_300", 1, save: false); // kills are hot-path: batched to disk by the next flow save
            if (e.WasElite) Bump("kill_elite_3", 1, save: false);
        }

        private void OnRunEnded(RunEndedEvent e)
        {
            if (e.Victory) Bump("clear_stage", 1, save: false);
            if (e.LevelReached >= 15) Bump("level_15", 1, save: false);
            _save.Save(); // one flush covers the whole run's mission progress
        }

        private void OnEquipmentChanged(EquipmentChangedEvent _)
        {
            // Enhancement publishes EquipmentChangedEvent; equip/fuse do too — acceptable
            // over-credit for a QoL mission (spec'd as 'interact with gear today').
            Bump("enhance_once", 1, save: false);
        }

        private void Bump(string missionId, int amount, bool save)
        {
            var mission = Find(missionId);
            var spec = GetSpec(missionId);
            if (mission == null || spec == null || mission.Claimed) return;
            if (mission.Progress >= spec.Target) return;

            mission.Progress = Math.Min(spec.Target, mission.Progress + amount);
            if (save) _save.Save();
        }

        private SaveModel.MissionProgress Find(string missionId)
        {
            foreach (var mission in _save.Data.DailyMissions)
                if (mission.MissionId == missionId) return mission;
            return null;
        }

        private bool AllClaimed()
        {
            foreach (var mission in _save.Data.DailyMissions)
                if (!mission.Claimed) return false;
            return true;
        }
    }
}
