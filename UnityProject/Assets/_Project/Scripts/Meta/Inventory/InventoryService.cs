using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Save;
using MobCrush.Data;
using MobCrush.Meta.Equipment;

namespace MobCrush.Meta.Inventory
{
    public enum InventorySort
    {
        ByRarityDesc,
        ByGearScoreDesc,
        BySlot,
        ByNewest
    }

    /// <summary>
    /// Inventory operations over the save model (Loop 15): equip/unequip per slot,
    /// sorted/filtered views, auto-merge (fuse every eligible triple), and the
    /// aggregated equipped-stat bundle the run bootstrap applies to the player's
    /// StatSheet. Views return copies — callers can never corrupt save state by
    /// mutating a returned list.
    /// Serialization/cloud-readiness: state lives entirely in SaveModel (instance ids +
    /// definition ids), already versioned/migratable via SaveService (Loop 4/20).
    /// </summary>
    public sealed class InventoryService
    {
        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly EquipmentService _equipment;
        private readonly EconomyConfig _economy;

        public InventoryService(ISaveService save, IEventBus events,
                                EquipmentService equipment, EconomyConfig economy)
        {
            _save = save;
            _events = events;
            _equipment = equipment;
            _economy = economy;
        }

        // --- Views ---

        /// <summary>Filtered + sorted snapshot for the UI. slotFilter/rarityFilter null = all.</summary>
        public List<SaveModel.OwnedEquipment> GetItems(EquipmentSlot? slotFilter = null,
                                                       EquipmentRarity? rarityFilter = null,
                                                       InventorySort sort = InventorySort.ByRarityDesc)
        {
            var result = new List<SaveModel.OwnedEquipment>();
            foreach (var item in _save.Data.Inventory)
            {
                var def = _equipment.GetDefinition(item.DefinitionId);
                if (def == null) continue; // definition removed in an update: hide rather than crash (DoD rule 7)
                if (slotFilter.HasValue && def.Slot != slotFilter.Value) continue;
                if (rarityFilter.HasValue && item.Rarity != (int)rarityFilter.Value) continue;
                result.Add(item);
            }

            Sort(result, sort);
            return result;
        }

        public SaveModel.OwnedEquipment GetEquipped(EquipmentSlot slot)
        {
            if (!_save.Data.EquippedBySlot.TryGetValue(slot.ToString(), out var id)) return null;
            foreach (var item in _save.Data.Inventory)
                if (item.InstanceId == id) return item;
            return null;
        }

        // --- Operations ---

        public bool Equip(string instanceId)
        {
            SaveModel.OwnedEquipment target = null;
            foreach (var item in _save.Data.Inventory)
                if (item.InstanceId == instanceId) { target = item; break; }
            if (target == null) return false;

            var def = _equipment.GetDefinition(target.DefinitionId);
            if (def == null) return false;

            // One item per slot: equipping replaces implicitly (no separate unequip step needed).
            _save.Data.EquippedBySlot[def.Slot.ToString()] = instanceId;
            _save.Save();
            _events.Publish(new EquipmentChangedEvent());
            return true;
        }

        public void Unequip(EquipmentSlot slot)
        {
            if (_save.Data.EquippedBySlot.Remove(slot.ToString()))
            {
                _save.Save();
                _events.Publish(new EquipmentChangedEvent());
            }
        }

        /// <summary>
        /// Merge-all convenience (GDD §7 QoL): repeatedly fuses any eligible triple
        /// (same slot + rarity, unequipped) from lowest rarity up until nothing fuses.
        /// Returns fusions performed.
        /// </summary>
        public int MergeAll()
        {
            int fused = 0;
            bool progress = true;
            while (progress)
            {
                progress = false;
                // Group by (slot, rarity); re-scan after each fusion since the list changed.
                var groups = new Dictionary<(EquipmentSlot, int), List<string>>();
                foreach (var item in _save.Data.Inventory)
                {
                    var def = _equipment.GetDefinition(item.DefinitionId);
                    if (def == null || IsEquipped(item.InstanceId)) continue;
                    if (!EquipmentMath.CanFuseRarity((EquipmentRarity)item.Rarity)) continue;

                    var key = (def.Slot, item.Rarity);
                    if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<string>();
                    list.Add(item.InstanceId);
                }

                foreach (var pair in groups)
                {
                    if (pair.Value.Count >= _economy.FusionInputCount &&
                        _equipment.TryFuse(pair.Value.GetRange(0, _economy.FusionInputCount)))
                    {
                        fused++;
                        progress = true;
                        break; // list mutated; rebuild groups
                    }
                }
            }
            return fused;
        }

        /// <summary>
        /// The equipped loadout as (stat, value, isPercent) triples. The run bootstrap
        /// applies these to the player's StatSheet — Meta never references Gameplay (Loop 3 §3).
        /// </summary>
        public List<(StatType Stat, float Value, bool IsPercent)> GetEquippedStatBonuses()
        {
            var result = new List<(StatType, float, bool)>(6);
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                var item = GetEquipped(slot);
                if (item == null) continue;
                var def = _equipment.GetDefinition(item.DefinitionId);
                if (def == null) continue;
                result.Add((def.MainStat, _equipment.GetItemMainStat(item), def.MainStatIsPercent));
            }
            return result;
        }

        public int GetTotalGearScore()
        {
            int score = 0;
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                var item = GetEquipped(slot);
                if (item != null)
                    score += EquipmentMath.ComputeGearScore(_economy, (EquipmentRarity)item.Rarity, item.EnhancementLevel);
            }
            return score;
        }

        private bool IsEquipped(string instanceId)
        {
            foreach (var pair in _save.Data.EquippedBySlot)
                if (pair.Value == instanceId) return true;
            return false;
        }

        private void Sort(List<SaveModel.OwnedEquipment> items, InventorySort sort)
        {
            switch (sort)
            {
                case InventorySort.ByRarityDesc:
                    items.Sort((a, b) => b.Rarity != a.Rarity
                        ? b.Rarity.CompareTo(a.Rarity)
                        : b.EnhancementLevel.CompareTo(a.EnhancementLevel));
                    break;
                case InventorySort.ByGearScoreDesc:
                    items.Sort((a, b) =>
                        EquipmentMath.ComputeGearScore(_economy, (EquipmentRarity)b.Rarity, b.EnhancementLevel)
                        .CompareTo(EquipmentMath.ComputeGearScore(_economy, (EquipmentRarity)a.Rarity, a.EnhancementLevel)));
                    break;
                case InventorySort.BySlot:
                    items.Sort((a, b) =>
                    {
                        var slotA = _equipment.GetDefinition(a.DefinitionId)?.Slot ?? 0;
                        var slotB = _equipment.GetDefinition(b.DefinitionId)?.Slot ?? 0;
                        return slotA != slotB ? slotA.CompareTo(slotB) : b.Rarity.CompareTo(a.Rarity);
                    });
                    break;
                case InventorySort.ByNewest:
                    items.Reverse(); // append-ordered storage: newest last → reversed
                    break;
            }
        }
    }
}
