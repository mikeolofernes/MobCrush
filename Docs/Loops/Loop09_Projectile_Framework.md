# Loop 9 — Projectile Framework

**Status:** Complete (weapon set from Loop 7 is now fully covered: all seven GDD types fire).

## Script explanations
- **`ProjectileConfig`** — a struct "launch recipe": damage, speed, lifetime, hit radius, motion (Straight/Boomerang/Homing), pierce, bounce, split, explosion. Copied per shot, so future upgrades ("+1 bounce", "shots explode") mutate a copy — zero new code paths. This is the "must support future upgrades" requirement made concrete.
- **`Projectile`** — one pooled component implements everything: distance-based hit detection against `EnemySystem` (no physics, no tunneling at our speed/radius ratios), per-projectile already-hit list (pierce can't double-tap), boomerang out-and-return with re-hit allowed on the return pass (weapon identity), homing steer with turn-rate cap, bounce redirection to nearest un-hit enemy, split-on-first-hit with child damage halved and re-split forbidden (no exponential storms), explosion query on final despawn, hard lifetime safety net. `PrefabSourceExtension` records each instance's prefab so split children pool correctly.
- **`ProjectileVolleyBehaviour`** — one behavior, three modes (Straight/Boomerang/Homing) for Rail Pistol / Boomerang Disc / Seeker Pod: symmetric fan spread, nearest-enemy aim with facing fallback, projectile-count bonus stat applied, Seeker Pod gets impact splash per GDD.
- **`TimedPoolReturn`** (Core) — self-releasing pooled VFX lifetime; the sanctioned replacement for `Destroy(go, t)`.

## Deliverables
✅ Reusable projectile system: collision, piercing, bounce, split, homing, explosion, lifetime, pooling — all config-driven.

## Folder Structure (added)
```
Scripts/Gameplay/Projectiles/{ProjectileConfig,Projectile}.cs
Scripts/Gameplay/Weapons/Behaviours/ProjectileVolleyBehaviour.cs
Scripts/Core/Pooling/TimedPoolReturn.cs
```

## Scripts
4 new.

## Assets Needed
- Projectile prefabs (sprite + `Projectile`) for Rail Pistol / Boomerang Disc / Seeker Pod; `SO_Weapon_` assets for the three weapons + evolutions.

## Risks
- Per-projectile `Update()` (unlike centrally-ticked enemies): projectile counts (≤100) make this acceptable; centralizing is a mechanical Loop 21 refactor if profiling demands.
- Distance hit tests could skip contacts if `Speed × frameTime > HitRadius` at severe frame drops — mitigated by the 30 FPS floor + our speed ranges; swept test noted as hardening option.

## Testing Checklist
- [x] Pierce N hits exactly N+1 enemies, never the same one twice.
- [x] Split children never re-split; child damage halved.
- [x] Boomerang returns and despawns at the player; re-hit on return allowed by design.
- [x] Explosion triggers only on despawn-by-hit, not on lifetime expiry.
- [x] All spawns/despawns through IPoolService.

## What Comes Next
Loop 10 — experience system (gems, magnet pickup, level curve, level-up events feeding Loop 11's draft).
