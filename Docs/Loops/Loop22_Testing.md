# Loop 22 — Testing

**Status:** Complete (unit suite committed; integration/stress plans are the Beta exit gates below).

## Unit tests committed (EditMode, `MobCrush.Tests.EditMode`)
- **CombatMathTests** — StatSheet stacking formula, modifier reset, damage-percent application, deterministic crit math (seeded RNG), XP curve monotonicity.
- **EconomyTests** — equipment main-stat rarity/enhancement scaling vs. GDD ladder, exponential enhancement cost, Mythic fusion block, luck's effect on rarity distribution (statistical, seeded).
- **SaveSystemTests** — AES round-trip, unique IV per write, legacy plaintext passthrough, safe SaveModel defaults.

These cover the pure-domain layer deliberately carved out across loops (StatSheet, DamageCalculator, EquipmentMath, curves, transforms) — the code where silent math errors cost real money/balance.

## Integration test plan (PlayMode, requires editor)
1. Bootstrap → menu → run → kill → gem → level-up → draft → apply → resume (full loop, fake view).
2. Pool integrity: 500 spawn/despawn cycles → live count 0, no duplicate actives, no leaked objects.
3. Draft system: chained level-ups produce N drafts, single resume.
4. Save: mutate → reload service → identical state; corrupt main file → backup loads.
5. Boss flow: trigger → trash cleared → phases at 60/25% → defeat event → results.

## Gameplay & stress tests (device)
- **Stress:** cap enemies (250) + max projectiles + boss for 5 min — the Loop 21 profiler checklist is the pass bar.
- **Soak:** 3 consecutive full runs without restart — memory flat between runs (pool clear verification).
- **Interrupt matrix:** call/backgrounding/kill during: draft open, boss fight, results, save write.

## Edge cases catalog
Zero-weapon start (missing SO ref → clear error), all-maxed draft (no soft-lock — guarded), fusion with equipped/duplicate ids (refused), clock rollback (offline grant = 0), device date change mid-daily-missions, save from a newer app version (unknown fields preserved by JObject migrations).

## Performance benchmarks (recorded per build in `Docs/Standards/`)
Frame time p50/p95 at 150 enemies, GC alloc/frame, cold start, memory peak, battery/20 min — mid-tier + min-spec devices.

## Balance simulation (planned harness)
Headless run simulator (pure-C# systems make this possible): scripted builds vs. stage spawn tables → clear-rate matrix; tune to GDD §15.5's 35–55% at recommended gear score. Harness stub scheduled with Beta content lock.

## Deliverables
✅ 13 unit tests + full test plan with exit gates.

## Folder Structure (added)
```
Tests/EditMode/{MobCrush.Tests.EditMode.asmdef, CombatMathTests, EconomyTests, SaveSystemTests}.cs
```

## Scripts
3 test files + test asmdef.

## Assets Needed
None.

## Risks
- PlayMode tests need scene fixtures (editor work); until they exist, the integration plan runs manually per milestone.

## Testing Checklist
- [x] Unit suite compiles against domain assemblies only (no scene dependencies).
- [x] All RNG-dependent tests seeded (deterministic CI).
- [ ] PlayMode fixtures authored (editor step).

## What Comes Next
Loop 23 — LiveOps (daily rewards, missions runtime, ads/analytics/remote-config interfaces + stubs).
