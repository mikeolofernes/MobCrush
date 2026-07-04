# Loop 13 — Boss System

**Status:** Complete — this closes the Vertical Slice (M1) feature set.

## Script explanations
- **`BossDefinition`** (SO, committed with Loop 12) — phases by HP threshold, looping pattern steps (AimedVolley / RadialBurst / Charge / SummonMinions / Idle), telegraph ≥ 0.6 s, per-phase soft enrage, rewards. New bosses are asset remixes of the pattern vocabulary — zero code.
- **`BossController`** — a small data interpreter: phase selection rides the damage path (no per-frame threshold polling); telegraph freezes the boss (the player's read window) then executes; charge locks direction at telegraph start (dodgeable) and hits at most once per dash; summons use the normal `EnemySystem.Spawn`; enrage scales pattern damage with phase time; contact damage with cooldown; defeat publishes `BossDefeatedEvent` (run flow owns victory/rewards). "Cutscene" hook = the `BossSpawnedEvent`/telegraph windows — UI banner lands in Loop 17.
- **`BossSpawner`** — bridges `SpawnSystem.BossTimeReached`: clears trash (arena reset + frame budget), spawns the unique boss (direct Instantiate — pooling a one-off wastes memory), injects dependencies, registers the boss as a weapon target.
- **`ITargetable` refactor** — weapons/projectiles now target `ITargetable` (position + damage) instead of concrete `EnemyController`; `EnemySystem` queries include the registered boss. This is why bosses need no special-casing in any weapon code — the loop's "reusable architecture" requirement.

## Deliverables
✅ Phases, patterns, special skills, summons, rewards data, telegraph/cutscene hooks — all data-driven and reusable.

## Folder Structure (added)
```
Scripts/Gameplay/Combat/ITargetable.cs
Scripts/Gameplay/Bosses/{BossController,BossSpawner}.cs
```
(+ targeting refactor across EnemySystem, WeaponContext, Projectile, OrbitalBehaviour.)

## Scripts
3 new, 5 touched.

## Assets Needed
- `SO_Boss_ForgeWarden` (charge + radial, GDD §3) + boss prefab (sprite, BossController); boss projectile prefab.

## Risks
- Boss uses its own Update (unique object — central ticking buys nothing).
- Phase HP thresholds only trigger on damage events; a heal-based mechanic would need re-evaluation on heal (not in scope).

## Testing Checklist
- [x] Phase transitions at 100/60/25% thresholds; steps loop within a phase.
- [x] Charge deals at most one hit per dash; telegraph precedes every damaging pattern.
- [x] Weapons hit the boss through the same code path as enemies (ITargetable).
- [x] Defeat publishes exactly one BossDefeatedEvent; dead boss excluded from targeting.

## What Comes Next
Loop 14 — Equipment (meta): slots, rarity, main/substats, enhancement, fusion — pure-C# domain logic over SO definitions.
