# Loop 16 — Meta Progression

**Status:** Complete

## Script explanations
- **`TalentDefinition`** (SO) — one tree node: stat effect per level, max level, exponential coin cost (`Base × Growth^level`, same sink shape as enhancement — GDD §15 anti-inflation), prerequisite link. The tree's shape emerges from prerequisite ids; no separate graph asset to desync.
- **`MetaProgressionService`** —
  - **Wallet:** the *only* code that mutates Coins/Gems/Cores; add/spend with balance checks, save + `CurrencyChangedEvent` on every change. (EquipmentService's direct writes predate this — flagged for the Loop 21 cleanup pass to route through the wallet.)
  - **Talents:** buy with prerequisite + cap + cost validation; bonuses exported as `(StatType, value, isPercent)` triples — identical contract to inventory, so the run bootstrap composes gear + talents into the StatSheet the same way.
  - **Research:** weapon unlocks for Cores (weapons 8–12 pipeline, GDD §14).
  - **Chapters:** monotonic highest-clear tracking.
  - **Offline progress:** capped welcome-back coin drip (2/min, 8 h cap) computed from `LastSavedUtcTicks`; clock rollback yields nothing. A retention nicety, not an idle economy.

## Balancing note
Meta contribution is bounded by design (GDD §15.4: ≤60% of power at on-tier content): talents and gear both feed the same additive StatSheet buckets, so the simulator (Loop 22) can compute the exact meta share for any loadout.

## Deliverables
✅ Talent tree, research, wallet, unlockables, offline progress — persistent and event-driven.

## Folder Structure (added)
```
Scripts/Data/TalentDefinition.cs
Scripts/Meta/Progression/MetaProgressionService.cs
```

## Scripts
2 new.

## Assets Needed
- `SO_Talent_*` (~15 nodes, 3 branches: Offense/Defense/Utility) with GDD costs.

## Risks
- Offline constants are code constants — deliberately: they're anti-abuse policy, not designer balance; move to EconomyConfig if LiveOps wants remote control (Loop 23 hook).

## Testing Checklist
- [x] Prerequisites gate purchases; costs follow the curve; caps enforced.
- [x] Wallet never goes negative; spend-then-mutate is atomic per call.
- [x] Offline: capped at 8 h, zero on first launch, zero on clock rollback.
- [x] Talent stat export matches purchased levels.

## What Comes Next
Loop 17 — complete UI layer (HUD, menus, draft view, inventory, settings, results) in MobCrush.UI.
