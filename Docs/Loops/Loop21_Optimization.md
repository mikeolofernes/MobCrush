# Loop 21 — Optimization

**Status:** Complete (architecture-level work + audit; on-device profiling passes require hardware and are captured as the checklist below).

## Optimizations already structural (audit results, by area)

### CPU
- Central tick loops: enemies (`EnemySystem`), gems (`ExperienceSystem`), AoE zones (behavior-owned slabs) — no per-instance `Update()` for high-count objects.
- No physics for gameplay hits: distance/segment/cone math against a flat list; manual normalize (single sqrt); `sqrMagnitude` comparisons throughout.
- Stateless enemy brains (zero allocation per spawn); struct events; struct `DamageInfo`/`ProjectileConfig`.
- Per-frame string work bounded: TMP `SetText` numeric overloads; timer text once/second.

### Memory / GC
- Pools for everything repeated (enemies, projectiles, gems, VFX, damage numbers, audio sources); `TimedPoolReturn` replaces `Destroy(go, t)`.
- Caller-owned/shared query buffers; `stackalloc` rarity weights; no LINQ in gameplay assemblies (rule + review).
- Known bounded caches: orbital lockout dict (cleared at 128), prefab-source registry (bounded by pooled instances).

### GPU (spec for the editor pass)
- URP 2D, single sprite atlas per stage content group (ASTC), no per-sprite materials → one dynamic batch for the horde; damage numbers capped/frame; overdraw watch: zone glows additive but small.

### Addressables (spec)
- Groups per Loop 3 §11 (`Boot`/`CoreGameplay`/`Stage_n`/`UI`/`Audio_Music`); stage content loaded additively, released on run end; no `Resources/` anywhere (verified — zero hits in repo).

### Code cleanup executed this loop
- `EquipmentService` currency writes now route through the `MetaProgressionService` wallet (single currency authority + one `CurrencyChangedEvent` source; flagged in Loop 16, paid here — no debt carried past the milestone).

## On-device profiler checklist (run on mid-tier + min-spec before Beta exit)
1. 150+ enemies + 60 projectiles + boss: ≥60 FPS mid-tier, ≥30 min-spec.
2. Profiler GC Alloc column: 0 B steady-state in Playing state (spikes allowed only on draft open / scene load).
3. Frame Debugger: horde renders in ≤ a handful of batches; no unexpected material breaks.
4. Memory snapshot: total < 400 MB on 3 GB device; pool sizes vs. peak usage (shrink oversized warm-ups).
5. Cold start < 4 s to menu on mid-tier.
6. Battery/thermal: 20-min session, no sustained thermal throttle (frame time drift watch).
7. `Application.targetFrameRate` adaptive: 60 default, drop to 30 on sustained throttle (bootstrap hook exists).

## Deliverables
✅ CPU/GPU/memory audit, pooling/batching/Addressables verification, profiler checklist, wallet-cleanup refactor.

## Folder Structure
No new runtime files (EquipmentService touched).

## Scripts
1 modified.

## Assets Needed
None.

## Risks
- The two prepared fallbacks if device profiling fails targets: spatial bucketing for target queries (isolated in EnemySystem), centralized projectile ticking (isolated in Projectile). Neither is speculatively built — YAGNI until data says otherwise.

## Testing Checklist
- [x] Repo-wide audit: no `Resources.Load`, no gameplay `Instantiate` outside pool/composition paths, no LINQ in hot assemblies.
- [x] Wallet refactor: enhancement/fusion spend paths still all-or-nothing.
- [ ] Device matrix profiling (hardware required — checklist above is the exit gate).

## What Comes Next
Loop 22 — testing: unit test suite for domain logic + integration/stress/edge-case plan and benchmarks.
