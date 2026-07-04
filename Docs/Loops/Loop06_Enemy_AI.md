# Loop 6 — Enemy AI

**Status:** Complete

## Script explanations
- **`EnemyDefinition`** (SO) — every enemy stat/behavior knob; the six GDD archetypes are pure data assets over four behavior brains. Adding an enemy = new asset, zero code.
- **`IEnemyBrain` + `EnemyBrains`** — stateless strategy singletons (Chaser, Sprinter, Ranged, Exploder). Per-enemy mutable state (timers/flags) lives on the controller, so spawning allocates nothing. Sprinter lunges inside range on cooldown; Ranged holds a distance band and fires; Exploder telegraphs (frozen = dodge window) then detonates against the player only.
- **`EnemyController`** — one live enemy: HP (elite-multiplied at init), transform-based movement helpers (manual normalize, no physics forces — colliders exist only for weapon hits), shared contact-damage tick with per-enemy cooldown, `IDamageable` intake, `IPoolable` hard reset. **No `Update()`** — see next.
- **`EnemySystem`** — the single tick loop for all enemies (hundreds of MonoBehaviour Updates is the genre's classic mobile CPU sink), spawn/despawn through the pool, `EnemyKilledEvent`/`DamageDealtEvent` publishing, enemy projectile firing, and allocation-free targeting queries (`FindNearest`, `QueryRadius` into caller buffers) that Loop 8 consumes.
- **`EnemyProjectile`** — minimal pooled Spitter shot; intentionally separate from the Loop 9 player projectile framework (enemy shots need no modifier stack).

## Deliverables
✅ Data-driven multi-archetype enemy framework: movement, targeting, detection, attack (contact/ranged/explosion), death, pooling, animation hooks.

## Folder Structure (added)
```
Scripts/Data/EnemyDefinition.cs
Scripts/Gameplay/Enemies/{IEnemyBrain,EnemyBrains,EnemyController,EnemySystem,EnemyProjectile}.cs
```

## Scripts
5 new + 1 SO definition.

## Assets Needed
- Per-archetype enemy prefabs: SpriteRenderer + CircleCollider2D (trigger, for player-weapon hits) + EnemyController; matching `SO_Enemy_*` assets with GDD §2 starting values.
- Spitter projectile prefab (sprite + EnemyProjectile).

## Risks
- Linear-scan targeting is O(n) per query — fine at ≤300 live enemies with few queries/frame; spatial bucketing is the prepared Loop 21 fallback (interface already isolates callers).
- Transform movement means no enemy-vs-enemy separation; acceptable for horde feel (Survivor.io enemies overlap too). Soft separation is a polish candidate.

## Testing Checklist
- [x] All four brains driven by one shared Tick path; no per-enemy Update.
- [x] Pool reuse resets timers/flags/tint (OnSpawned hard reset).
- [x] Exploder self-destruct grants no kill rewards; normal deaths publish exactly one EnemyKilledEvent.
- [x] Zero steady-state allocations in Tick/queries (struct math, caller-owned buffers).

## What Comes Next
Loop 7 — data-driven modular weapon system (melee/projectile/laser/AoE/orbital/boomerang/missile behaviors over one WeaponDefinition model).
