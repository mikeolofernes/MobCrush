# Loop 12 — Enemy Spawner

**Status:** Complete

## Script explanations
- **`StageDefinition`** (SO) — the whole stage as data: wave windows with spawns-per-second **curves** (pressure authored over time), elite injections at timestamps, stage-time HP/damage scaling curves, elite multipliers, boss reference, live-enemy cap, and pool warm-up list. Designers own the entire difficulty arc; the spawner holds zero balance numbers.
- **`BossDefinition`** (SO, data half of Loop 13) — phases by HP threshold, each a looping pattern-step list (AimedVolley / RadialBurst / Charge / SummonMinions / Idle) with telegraph ≥ 0.6 s and soft-enrage rate; stats, projectile prefab, rewards. New bosses = pattern remixes as assets.
- **`SpawnSystem`** — dumb executor: per-wave fractional **accumulators** (correct sub-1/sec rates), off-screen ring placement (12–15 u, past phone screen edge), difficulty multipliers sampled per spawn, elite injection cursor, **throttle** at `MaxLiveEnemies` (burns spawn tokens instead of backlogging — pressure resumes naturally, frame rate protected), boss trigger at duration end via local event + `BossSpawnedEvent`.

## Deliverables
✅ Data-driven wave system: spawn zones, spawn curves, difficulty scaling, elites, boss trigger, performance cap + warm-up.

## Folder Structure (added)
```
Scripts/Data/{StageDefinition,BossDefinition}.cs
Scripts/Gameplay/Spawning/SpawnSystem.cs
```

## Scripts
3 new.

## Assets Needed
- `SO_Stage_01` authored per GDD ch.1 (15 min, Crawler-heavy early, archetype mix per window, elites at 3/7/11 min); warm-up entries (~64 crawlers, 32 others, 64 gems, 64 projectiles).

## Risks
- Off-screen ring is player-relative; if stages later get walls/camera bounds, add a navigable-point check (isolated in `RandomRingPosition`).
- Throttling changes effective difficulty at cap — by design (frame rate outranks nominal pressure); the balance simulator (Loop 22) accounts for the cap.

## Testing Checklist
- [x] Fractional spawn rates (e.g. 0.4/s) average correctly via accumulators.
- [x] Elite injections fire once each, even across frame spikes (while-loop cursor).
- [x] Cap: live count never exceeds MaxLiveEnemies from wave spawns.
- [x] Boss trigger fires exactly once; spawn script halts after.

## What Comes Next
Loop 13 — boss runtime (phase runner, patterns, summons, rewards, defeat flow).
