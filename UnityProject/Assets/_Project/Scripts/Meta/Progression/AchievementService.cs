using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;

namespace MobCrush.Meta.Progression
{
    /// <summary>
    /// Lifetime achievements (GDD §12 — production-readiness fix: was designed but never
    /// implemented). Tiered thresholds over lifetime counters, gem rewards per tier.
    /// Counters and claim flags persist in SaveModel.LifetimeCounters (no schema change:
    /// claims are stored under "claimed:{id}:{tier}"). Catalog is code-simple like the
    /// mission catalog and remote-config overridable by id later.
    /// </summary>
    public sealed class AchievementService : IDisposable
    {
        public sealed class AchievementSpec
        {
            public string Id;              // also the counter key
            public string Description;
            public long[] TierThresholds;  // bronze/silver/gold
            public long[] TierGemRewards;
        }

        private static readonly AchievementSpec[] Catalog =
        {
            new() { Id = "kills", Description = "Destroy enemies",
                    TierThresholds = new long[] { 1_000, 10_000, 100_000 }, TierGemRewards = new long[] { 10, 25, 60 } },
            new() { Id = "elite_kills", Description = "Destroy elites",
                    TierThresholds = new long[] { 10, 100, 1_000 }, TierGemRewards = new long[] { 10, 25, 60 } },
            new() { Id = "clears", Description = "Clear stages",
                    TierThresholds = new long[] { 1, 10, 50 }, TierGemRewards = new long[] { 15, 30, 80 } },
            new() { Id = "runs", Description = "Finish runs",
                    TierThresholds = new long[] { 5, 50, 500 }, TierGemRewards = new long[] { 10, 25, 60 } },
            new() { Id = "evolutions", Description = "Evolve weapons",
                    TierThresholds = new long[] { 1, 25, 200 }, TierGemRewards = new long[] { 15, 30, 80 } }
        };

        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly MetaProgressionService _wallet;

        public AchievementService(ISaveService save, IEventBus events, MetaProgressionService wallet)
        {
            _save = save;
            _events = events;
            _wallet = wallet;

            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Subscribe<RunEndedEvent>(OnRunEnded);
            _events.Subscribe<WeaponEvolvedEvent>(OnWeaponEvolved);
        }

        public void Dispose()
        {
            _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            _events.Unsubscribe<RunEndedEvent>(OnRunEnded);
            _events.Unsubscribe<WeaponEvolvedEvent>(OnWeaponEvolved);
        }

        private void OnWeaponEvolved(WeaponEvolvedEvent _) => Increment("evolutions", 1);

        public IReadOnlyList<AchievementSpec> Achievements => Catalog;

        public long GetCounter(string id) =>
            _save.Data.LifetimeCounters.TryGetValue(id, out long value) ? value : 0;

        public bool IsTierClaimed(string id, int tier) =>
            _save.Data.LifetimeCounters.ContainsKey(ClaimKey(id, tier));

        public bool IsTierReached(AchievementSpec spec, int tier) =>
            tier >= 0 && tier < spec.TierThresholds.Length && GetCounter(spec.Id) >= spec.TierThresholds[tier];

        /// <summary>Claim a reached, unclaimed tier → gems. Tiers claim independently (skipping bronze is allowed).</summary>
        public bool TryClaim(string achievementId, int tier)
        {
            AchievementSpec spec = null;
            foreach (var s in Catalog)
                if (s.Id == achievementId) { spec = s; break; }
            if (spec == null || !IsTierReached(spec, tier) || IsTierClaimed(achievementId, tier)) return false;

            _save.Data.LifetimeCounters[ClaimKey(achievementId, tier)] = 1;
            _wallet.AddCurrency("gems", spec.TierGemRewards[tier]); // saves + publishes
            return true;
        }

        // --- Counter accrual (batched to disk by the run-end save, same as missions) ---

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            Increment("kills", 1);
            if (e.WasElite) Increment("elite_kills", 1);
        }

        private void OnRunEnded(RunEndedEvent e)
        {
            Increment("runs", 1);
            if (e.Victory) Increment("clears", 1);
            _save.Save();
        }

        private void Increment(string key, long amount)
        {
            _save.Data.LifetimeCounters.TryGetValue(key, out long current);
            _save.Data.LifetimeCounters[key] = current + amount;
        }

        private static string ClaimKey(string id, int tier) => $"claimed:{id}:{tier}";
    }
}
