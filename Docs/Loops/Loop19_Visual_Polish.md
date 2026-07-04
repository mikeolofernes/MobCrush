# Loop 19 — Visual Polish

**Status:** Complete (code side; particle prefabs/shaders are editor/art tasks specced below).

## Script explanations
- **`CameraFollow`** — SmoothDamp follow in LateUpdate + shake as a post-offset (never pollutes follow position → no post-shake drift); overlapping shakes take the max, not the sum (no nausea stacking).
- **`ScreenShakeBinder`** — event→shake mapping in one tunable component (mirrors the audio binder pattern): player hit = small kick, boss spawn = big rumble.
- **`DamageNumberSystem` / `DamageNumber`** — pooled TMP labels from `DamageDealtEvent`: rise + fade, per-frame cap of 6 (beyond a few, numbers are cost, not information) with **crits always shown** (bigger, gold — they're the payoff), pool-returned via `TimedPoolReturn`.
- **Enemy hit flash** — brief white-out tint on damage, decayed inside the existing central `Tick` (no new per-enemy component), telegraph tint protected.

## Juice inventory (where each lives)
Screen shake ✅ · hit flash ✅ · damage numbers ✅ · camera smoothing ✅ · draft card pop (Loop 17) ✅ · gem magnet acceleration (Loop 10) ✅ · boss telegraph tint (Loop 13) ✅. Particles & shaders: specced for art — death puff (8–12 sprite particles, pooled), muzzle flash, evolution burst, zone rim glow (URP 2D shader graph, additive, mobile-safe single texture sample).

## Deliverables
✅ Particles hooks, screen shake, camera, hit effects, damage numbers, juice — all pooled and event-driven; shader/particle art brief written.

## Folder Structure (added)
```
Scripts/Gameplay/Polish/{CameraFollow,ScreenShakeBinder,DamageNumberSystem}.cs
```

## Scripts
3 new files (4 classes), EnemyController touched.

## Assets Needed
- Damage-number prefab (world-space TMP + DamageNumber + TimedPoolReturn), death-puff particle prefab, zone/beam glow materials.

## Risks
- World-space TMP numbers cost fill rate on weak GPUs — capped per frame; switch to a sprite-sheet font if Loop 21 profiling flags it.

## Testing Checklist
- [x] Shake decays fully; camera returns to exact follow position.
- [x] Number cap holds under nova hits; crits bypass cap.
- [x] Hit flash never overrides exploder telegraph; pooled reuse resets tint (OnSpawned).

## What Comes Next
Loop 20 — save hardening (AES encryption, slots, autosave triggers, versioning verification).
