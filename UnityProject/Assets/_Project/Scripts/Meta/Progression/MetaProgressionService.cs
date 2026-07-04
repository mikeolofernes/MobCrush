using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;
using MobCrush.Data;

namespace MobCrush.Meta.Progression
{
    /// <summary>
    /// Permanent progression (Loop 16): the wallet (single authority for all currency
    /// mutation), talent tree purchases, research unlocks (weapons), chapter clears and
    /// offline progress. Everything persists via SaveModel; every mutation publishes
    /// events so UI/analytics observe without coupling.
    /// </summary>
    public sealed class MetaProgressionService
    {
        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly IReadOnlyDictionary<string, TalentDefinition> _talents;

        // Offline progress tuning (GDD: modest drip, capped — a welcome-back, not an idle game).
        private const float OfflineCoinsPerMinute = 2f;
        private const float OfflineCapHours = 8f;

        public MetaProgressionService(ISaveService save, IEventBus events,
                                      IReadOnlyDictionary<string, TalentDefinition> talents)
        {
            _save = save;
            _events = events;
            _talents = talents;
        }

        // --- Wallet: the only place currency fields change ---

        public long Coins => _save.Data.Coins;
        public long Gems => _save.Data.Gems;
        public long Cores => _save.Data.Cores;

        public void AddCurrency(string currencyId, long amount)
        {
            if (amount <= 0) return;
            MutateCurrency(currencyId, amount);
        }

        public bool TrySpend(string currencyId, long amount)
        {
            if (amount <= 0) return false;
            if (GetBalance(currencyId) < amount) return false;
            MutateCurrency(currencyId, -amount);
            return true;
        }

        private long GetBalance(string currencyId) => currencyId switch
        {
            "coins" => _save.Data.Coins,
            "gems" => _save.Data.Gems,
            "cores" => _save.Data.Cores,
            _ => 0
        };

        private void MutateCurrency(string currencyId, long delta)
        {
            switch (currencyId)
            {
                case "coins": _save.Data.Coins += delta; break;
                case "gems": _save.Data.Gems += delta; break;
                case "cores": _save.Data.Cores += delta; break;
                default: return;
            }
            _save.Save();
            _events.Publish(new CurrencyChangedEvent(currencyId, GetBalance(currencyId)));
        }

        // --- Talents ---

        public int GetTalentLevel(string talentId) =>
            _save.Data.TalentLevels.TryGetValue(talentId, out int level) ? level : 0;

        public bool CanBuyTalent(string talentId)
        {
            if (!_talents.TryGetValue(talentId, out var talent)) return false;
            int level = GetTalentLevel(talentId);
            if (level >= talent.MaxLevel) return false;
            if (!string.IsNullOrEmpty(talent.PrerequisiteId) && GetTalentLevel(talent.PrerequisiteId) < 1) return false;
            return Coins >= talent.GetCost(level);
        }

        public bool TryBuyTalent(string talentId)
        {
            if (!CanBuyTalent(talentId)) return false;
            var talent = _talents[talentId];
            int level = GetTalentLevel(talentId);

            if (!TrySpend("coins", talent.GetCost(level))) return false;
            _save.Data.TalentLevels[talentId] = level + 1;
            _save.Save();
            return true;
        }

        /// <summary>Talent bonuses as (stat, value, isPercent) — applied to the StatSheet at run start, same contract as inventory.</summary>
        public List<(StatType Stat, float Value, bool IsPercent)> GetTalentStatBonuses()
        {
            var result = new List<(StatType, float, bool)>();
            foreach (var pair in _save.Data.TalentLevels)
            {
                if (pair.Value <= 0 || !_talents.TryGetValue(pair.Key, out var talent)) continue;
                result.Add((talent.Stat, talent.ValuePerLevel * pair.Value, talent.IsPercent));
            }
            return result;
        }

        // --- Research / unlocks ---

        public bool IsWeaponUnlocked(string weaponId) => _save.Data.UnlockedWeaponIds.Contains(weaponId);

        public bool TryUnlockWeapon(string weaponId, long coreCost)
        {
            if (IsWeaponUnlocked(weaponId)) return false;
            if (!TrySpend("cores", coreCost)) return false;
            _save.Data.UnlockedWeaponIds.Add(weaponId);
            _save.Save();
            return true;
        }

        // --- Chapters ---

        public int HighestChapterCleared => _save.Data.HighestChapterCleared;

        public void RegisterChapterClear(int chapterIndex)
        {
            if (chapterIndex <= _save.Data.HighestChapterCleared) return;
            _save.Data.HighestChapterCleared = chapterIndex;
            _save.Save();
        }

        // --- Offline progress ---

        /// <summary>
        /// Call once at boot AFTER load: converts absence time into a capped coin grant.
        /// Returns coins granted (0 on first launch or clock rollback) so UI can show a
        /// welcome-back toast. Negative deltas ignored — device clock cheats earn nothing.
        /// </summary>
        public long ClaimOfflineProgress(DateTime nowUtc)
        {
            long lastTicks = _save.Data.LastSavedUtcTicks;
            if (lastTicks <= 0) return 0;

            double minutes = (nowUtc - new DateTime(lastTicks, DateTimeKind.Utc)).TotalMinutes;
            if (minutes <= 1) return 0;

            minutes = Math.Min(minutes, OfflineCapHours * 60.0);
            long grant = (long)(minutes * OfflineCoinsPerMinute);
            if (grant > 0) AddCurrency("coins", grant);
            return grant;
        }
    }
}
