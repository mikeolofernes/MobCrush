using System;
using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;
using MobCrush.Data;

namespace MobCrush.Meta.Equipment
{
    /// <summary>
    /// Equipment operations over the save model (Loop 14): grant, enhance (coin sink,
    /// no failure), fuse (3 identical-rarity same-slot → next rarity). Definition assets
    /// are resolved through a registry dictionary injected at construction — the service
    /// itself never loads assets. Every mutation saves and publishes events.
    /// </summary>
    public sealed class EquipmentService
    {
        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly EconomyConfig _economy;
        private readonly IReadOnlyDictionary<string, EquipmentDefinition> _definitions;

        public EquipmentService(ISaveService save, IEventBus events, EconomyConfig economy,
                                IReadOnlyDictionary<string, EquipmentDefinition> definitions)
        {
            _save = save;
            _events = events;
            _economy = economy;
            _definitions = definitions;
        }

        public EquipmentDefinition GetDefinition(string definitionId) =>
            _definitions.TryGetValue(definitionId, out var def) ? def : null;

        /// <summary>Drop/reward entry point. InstanceId = GUID so identity survives any list operation.</summary>
        public SaveModel.OwnedEquipment Grant(string definitionId, EquipmentRarity rarity)
        {
            var item = new SaveModel.OwnedEquipment
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                DefinitionId = definitionId,
                Rarity = (int)rarity,
                EnhancementLevel = 0
            };
            _save.Data.Inventory.Add(item);
            _save.Save();
            _events.Publish(new EquipmentChangedEvent());
            return item;
        }

        /// <summary>Enhance +1: spends coins, never fails, capped at MaxEnhancement (GDD §7).</summary>
        public bool TryEnhance(string instanceId)
        {
            var item = FindItem(instanceId);
            if (item == null) return false;
            if (!EquipmentMath.CanEnhance(_economy, item.EnhancementLevel, _save.Data.Coins)) return false;

            _save.Data.Coins -= _economy.GetEnhanceCost(item.EnhancementLevel);
            item.EnhancementLevel++;

            _save.Save();
            _events.Publish(new CurrencyChangedEvent("coins", _save.Data.Coins));
            _events.Publish(new EquipmentChangedEvent());
            return true;
        }

        /// <summary>
        /// Fusion: FusionInputCount items, same slot, same rarity, none equipped →
        /// one item of the first input's definition at next rarity. Enhancement of the
        /// best input carries over from Epic+ (GDD §7). Charges Cores.
        /// </summary>
        public bool TryFuse(IReadOnlyList<string> instanceIds)
        {
            if (instanceIds == null || instanceIds.Count != _economy.FusionInputCount) return false;

            // Validate inputs before mutating anything (all-or-nothing).
            var items = new List<SaveModel.OwnedEquipment>(instanceIds.Count);
            foreach (var id in instanceIds)
            {
                var item = FindItem(id);
                if (item == null || IsEquipped(id)) return false;
                items.Add(item);
            }

            var rarity = (EquipmentRarity)items[0].Rarity;
            if (!EquipmentMath.CanFuseRarity(rarity)) return false;

            var slot = GetDefinition(items[0].DefinitionId)?.Slot;
            foreach (var item in items)
            {
                if (item.Rarity != (int)rarity) return false;
                if (GetDefinition(item.DefinitionId)?.Slot != slot) return false;
            }

            long coreCost = _economy.FusionCoreCostPerRarity * ((int)rarity + 1);
            if (_save.Data.Cores < coreCost) return false;

            int bestEnhance = 0;
            foreach (var item in items)
                if (item.EnhancementLevel > bestEnhance) bestEnhance = item.EnhancementLevel;

            // Commit.
            _save.Data.Cores -= coreCost;
            foreach (var item in items)
                _save.Data.Inventory.Remove(item);

            var fused = new SaveModel.OwnedEquipment
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                DefinitionId = items[0].DefinitionId,
                Rarity = (int)rarity + 1,
                EnhancementLevel = (int)rarity >= _economy.KeepEnhancementFromRarity ? bestEnhance : 0
            };
            _save.Data.Inventory.Add(fused);

            _save.Save();
            _events.Publish(new CurrencyChangedEvent("cores", _save.Data.Cores));
            _events.Publish(new EquipmentChangedEvent());
            return true;
        }

        public float GetItemMainStat(SaveModel.OwnedEquipment item)
        {
            var def = GetDefinition(item.DefinitionId);
            return def == null ? 0f
                : EquipmentMath.ComputeMainStat(def, _economy, (EquipmentRarity)item.Rarity, item.EnhancementLevel);
        }

        private SaveModel.OwnedEquipment FindItem(string instanceId)
        {
            foreach (var item in _save.Data.Inventory)
                if (item.InstanceId == instanceId) return item;
            return null;
        }

        private bool IsEquipped(string instanceId)
        {
            foreach (var pair in _save.Data.EquippedBySlot)
                if (pair.Value == instanceId) return true;
            return false;
        }
    }
}
