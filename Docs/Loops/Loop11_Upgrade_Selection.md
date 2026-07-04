# Loop 11 — Upgrade Selection

**Status:** Complete (logic + UI contract; the animated UGUI view lands with the rest of the UI in Loop 17 via `IUpgradeDraftView`).

## Script explanations
- **`RarityTable`** (SO) — weighted rarity roll (GDD start 55/28/13/4) with a transparent Luck modifier (drains Common weight into higher tiers) + per-tier value multipliers. `stackalloc` weights: zero allocation per roll.
- **`UpgradeDefinition`** (SO) — a passive: one `StatType` effect per level, flat or percent, 5 levels; doubles as the evolution catalyst referenced by `WeaponDefinition.RequiredPassiveId`.
- **`UpgradePool`** (SO) — per-stage draftable weapons + passives + rarity table; chapters can introduce content gradually.
- **`DraftOption` / `DraftGenerator`** — pure, seeded-RNG-testable draft rules: eligible evolutions always offered first (Legendary card); remaining slots roll rarity then pick uniformly among eligible candidates (new weapon / weapon level / new passive / passive level, respecting slot caps and max levels); no duplicates per draft; no dead picks by construction; `SynergyHint` set when an option progresses an evolution pair (the USP surfaced in UI).
- **`IUpgradeDraftView`** — UI contract so drafting logic never touches a canvas.
- **`UpgradeDraftSystem`** — flow orchestration: LevelUpEvent → `GameState.LevelUpPause` (timeScale 0 via GameManager) → generate → show → apply (weapon add/level/evolve via WeaponController; passive ledger + StatSheet application with rarity-scaled magnitude) → resume. Queued drafts for multi-level collections; empty-pool guard prevents soft-lock when everything is maxed.

## Deliverables
✅ Pause-drafting with weighted rarity, evolution, synergy hints, queueing — reusable (pool SO + view interface).

## Folder Structure (added)
```
Scripts/Data/{RarityTable,UpgradeDefinition,UpgradePool}.cs
Scripts/Gameplay/Upgrades/{DraftOption,DraftGenerator,IUpgradeDraftView,UpgradeDraftSystem}.cs
```
(+ `StatType` relocated to `Scripts/Data/` — shared vocabulary needed by Data SOs; Gameplay already references Data, reverse would be circular.)

## Scripts
7 new, StatType moved.

## Assets Needed
- `SO_Passive_*` for the 10 GDD §5 passives; `SO_RarityTable`; `SO_UpgradePool` per chapter; draft card prefab (Loop 17).

## Risks
- Rarity affecting only passives' magnitude (weapons always level by 1) is a deliberate GDD simplification; revisit if drafts feel flat in playtests.
- Banish/reroll (GDD talent unlocks) deferred to Loop 16 talents — the generator API already supports regeneration.

## Testing Checklist
- [x] Draft never offers: full-slot new weapons, maxed level-ups, duplicates within a draft.
- [x] Evolution appears iff weapon max level + catalyst owned; applying swaps the weapon in place.
- [x] Chained level-ups produce back-to-back drafts, then exactly one resume to Playing.
- [x] Deterministic with seeded RNG (Loop 22 unit tests cover generator + rarity roll).

## What Comes Next
Loop 12 — enemy wave spawner (spawn zones, curves, difficulty scaling, elites, boss trigger).
