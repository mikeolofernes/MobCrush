# Loop 15 — Inventory

**Status:** Complete

## Script explanation
- **`InventoryService`** — all inventory operations over the save model:
  - **Views:** filtered (slot/rarity) + sorted (rarity / gear score / slot / newest) snapshots; returned lists are copies so UI can never corrupt save state. Items whose definition disappeared in an update are hidden, not crashed on (DoD resilience rule).
  - **Equip/Unequip:** one item per slot, equip-replaces semantics; every mutation saves + publishes `EquipmentChangedEvent`.
  - **MergeAll:** QoL auto-fusion — repeatedly fuses eligible triples via `EquipmentService.TryFuse` until stable (all validation reused, no duplicated rules).
  - **`GetEquippedStatBonuses`:** the loadout as `(StatType, value, isPercent)` triples; the Game-scene bootstrap applies them to the player's StatSheet at run start. This keeps the Loop 3 rule intact: **Meta never references Gameplay** — the data contract is the shared `StatType` vocabulary in Data.
  - **Gear score total** for stage recommendations.

## Serialization / cloud-readiness
Inventory state is entirely inside `SaveModel` (GUID instance ids + definition ids + two ints per item): versioned, migratable, encrypted in Loop 20, and small enough to sync as a single cloud blob with the `LastSavedUtcTicks` conflict stamp — no separate work needed later.

## Deliverables
✅ Sorting, filtering, merge, equip/unequip, serialization, cloud-ready — as one testable pure-C# service.

## Folder Structure (added)
```
Scripts/Meta/Inventory/InventoryService.cs
```

## Scripts
1 new (deliberately — Loop 14 already carved the right seams).

## Assets Needed
None (UI prefabs come with Loop 17).

## Risks
- MergeAll regroups after every fusion (O(n²) worst case) — inventories are ≤ a few hundred items; simplicity wins.

## Testing Checklist
- [x] Equip replaces same-slot item; unequip clears; events fire on every mutation.
- [x] MergeAll terminates (rarity strictly increases per fusion; Mythic excluded).
- [x] Filters + sorts never mutate stored order (copy semantics).
- [x] Stat bundle matches equipped items' math exactly (shared EquipmentMath).

## What Comes Next
Loop 16 — Meta progression (talent tree, wallet, research/unlocks, offline progress).
