# Loop 10 — Experience System

**Status:** Complete

## Script explanations
- **`ExperienceCurve`** (SO) — `Base × level^Exponent × shapeCurve(level/Max)`: power term for macro shape, hand-editable AnimationCurve for local designer bumps. No XP number lives in code.
- **`ExperienceGem`** — pooled pickup, **no Update()** (hundreds may exist): inert → magnet-attracted when the player's `PickupRadius` stat reaches it → accelerating flight (snappy, no orbit) → collected on contact. Value tiers tint the sprite (small/medium/large read at a glance).
- **`ExperienceSystem`** — central gem ticker + XP ledger: spawns gems from `EnemyKilledEvent` (which now carries `XpValue`, elites ×10), applies `XpGainPercent`, publishes `ExperienceGainedEvent` and `LevelUpEvent` (multi-level from one big gem handled by a while-loop, each level drafts separately), `AttractAll()` implements the Magnet pickup item.

## Deliverables
✅ Gems, level-up, data-driven XP curve, pickup radius, magnet effect; smooth attraction motion.

## Folder Structure (added)
```
Scripts/Data/ExperienceCurve.cs
Scripts/Gameplay/Experience/{ExperienceGem,ExperienceSystem}.cs
```
(+ `EnemyKilledEvent` extended with XpValue; EnemySystem publishes it.)

## Scripts
3 new, 2 touched.

## Assets Needed
- Gem prefab (sprite + ExperienceGem), `SO_ExperienceCurve` with GDD-start values (base 5, exp 1.35, max 60).

## Risks
- Gem flood on massacre spikes: pooled + centrally ticked keeps cost linear; gem-merging (combine nearby small gems) is the prepared optimization if Loop 21 profiling flags it.

## Testing Checklist
- [x] Level-up math: multi-level single collection produces N LevelUpEvents with correct carryover.
- [x] Pickup radius reads the live stat (upgrades to PickupRadius work mid-run).
- [x] No allocations in the per-frame gem loop; removal is O(1)-amortized reverse iteration.
- [x] ClearAll returns every gem to the pool (no leaks across runs).

## What Comes Next
Loop 11 — Survivor-style upgrade drafting (pause on level-up, weighted rarity, evolutions, synergy hints, reusable UI contract).
