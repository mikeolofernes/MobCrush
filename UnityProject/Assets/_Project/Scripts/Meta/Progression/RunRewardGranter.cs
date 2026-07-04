using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Data;
using MobCrush.Meta.Equipment;

namespace MobCrush.Meta.Progression
{
    /// <summary>
    /// Meta-side listener that converts a finished run into rewards (post-review fix —
    /// previously nothing granted anything). Lives here, not in Gameplay: the Loop 3 rule
    /// stands — Gameplay publishes facts (RunEndedEvent), Meta owns wallets and loot.
    /// Rewards (EconomyConfig): coins per kill; on victory: coin bonus + cores + one
    /// random equipment drop (Common, or Rare at the configured chance).
    /// </summary>
    public sealed class RunRewardGranter : IDisposable
    {
        private readonly IEventBus _events;
        private readonly MetaProgressionService _wallet;
        private readonly EquipmentService _equipment;
        private readonly EconomyConfig _economy;
        private readonly IReadOnlyList<EquipmentDefinition> _dropTable;
        private readonly Random _rng;

        public RunRewardGranter(IEventBus events, MetaProgressionService wallet,
                                EquipmentService equipment, EconomyConfig economy,
                                IReadOnlyList<EquipmentDefinition> dropTable, Random rng = null)
        {
            _events = events;
            _wallet = wallet;
            _equipment = equipment;
            _economy = economy;
            _dropTable = dropTable;
            _rng = rng ?? new Random();

            _events.Subscribe<RunEndedEvent>(OnRunEnded);
        }

        public void Dispose() => _events.Unsubscribe<RunEndedEvent>(OnRunEnded);

        private void OnRunEnded(RunEndedEvent e)
        {
            long coins = (long)(e.Kills * _economy.RunCoinsPerKill);
            if (e.Victory) coins += _economy.VictoryCoinBonus;
            if (coins > 0) _wallet.AddCurrency("coins", coins);

            if (!e.Victory) return;

            if (_economy.VictoryCoreReward > 0)
                _wallet.AddCurrency("cores", _economy.VictoryCoreReward);

            // Victory equipment drop (GDD §3: boss guarantees gear). Uniform pick over the
            // drop table keeps it simple; per-chapter tables are a data extension later.
            if (_dropTable != null && _dropTable.Count > 0)
            {
                var definition = _dropTable[_rng.Next(_dropTable.Count)];
                var rarity = _rng.NextDouble() < _economy.VictoryRareDropChance
                    ? EquipmentRarity.Rare
                    : EquipmentRarity.Common;
                _equipment.Grant(definition.Id, rarity);
            }
        }
    }
}
