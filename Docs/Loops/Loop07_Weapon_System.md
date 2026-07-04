# Loop 7 — Weapon System

**Status:** Complete (4 of 7 behaviors here; the 3 projectile-based ones land with the projectile framework in Loop 9, which they depend on — dependency-honest sequencing rather than stubs).

## Design
One `WeaponDefinition` SO describes ANY weapon: identity, per-level stat rows (damage/cooldown/area/count/speed/duration/pierce/knockback), pooled effect prefab, SFX id, and evolution pair (`RequiredPassiveId` + `EvolvedForm`). The `WeaponType` enum selects an `IWeaponBehaviour` strategy; everything else is data — a new weapon of an existing type is an asset, not code.

## Script explanations
- **`WeaponDefinition`** — the data model above; `GetLevel` clamps so max-level weapons awaiting evolution keep top stats.
- **`WeaponContext` / `IDamageResolver`** — capability bundle injected into behaviors at equip (player, enemy queries, pool, events, damage resolver, shared scratch buffer). Behaviors never touch the locator or the scene; `IDamageResolver` hides the damage/crit formula until Loop 8 provides it.
- **`IWeaponBehaviour`** — Equip / Fire (discrete) / Tick (continuous) / Unequip. One stateful instance per equipped weapon.
- **`WeaponInstance`** — runtime pairing of definition+level+behavior; owns cooldown (`cooldown / (1 + attackSpeed%)`); `Evolve()` swaps the definition in place and re-equips the behavior (GDD §4).
- **Behaviors:** `MeleeArcBehaviour` (radius query + cone dot-test, no physics casts), `AoePulseBehaviour` (fixed 8-slot zone slab, pulse ticks, cluster-seeking placement), `OrbitalBehaviour` (pooled orbiting bodies, per-enemy re-hit lockout, body count rebuilds on level-up), `LaserSweepBehaviour` (instant corridor via point-to-segment math with a circle broad-phase).

## Deliverables
✅ Fully data-driven weapon model supporting all seven GDD types; melee/AoE/orbital/laser behaviors implemented allocation-free.

## Folder Structure (added)
```
Scripts/Data/WeaponDefinition.cs
Scripts/Gameplay/Weapons/{WeaponContext,IWeaponBehaviour,WeaponInstance}.cs
Scripts/Gameplay/Weapons/Behaviours/{MeleeArc,AoePulse,Orbital,LaserSweep}Behaviour.cs
```

## Scripts
7 new.

## Assets Needed
- `SO_Weapon_*` assets for Pulse Blade, Shock Mine, Orbit Drones, Arc Laser (+ evolved forms) with GDD §4 pairs.
- Effect prefabs: swing arc sprite, zone circle, drone sprite, beam quad — all pooled, all with the Loop 9 `TimedPoolReturn` where they self-expire.

## Risks
- Behavior geometry constants (arc angle, beam length, pulse interval) are code constants by design — they define feel, not balance; documented in each file. If designers ever need them, they migrate to the SO.
- Orbital lockout dictionary uses enemy references as keys across pooled reuse — cleared at 128 entries and on unequip; a stale entry only delays one hit by ≤0.5 s (harmless).

## Testing Checklist
- [x] Adding a 5th melee weapon requires zero code (asset only).
- [x] All hit tests allocation-free (shared QueryBuffer, struct math).
- [x] Evolution swap releases old behavior resources (Unequip before re-Equip).
- [x] Zones/orbitals stop cleanly on Unequip (run end / evolution).

## What Comes Next
Loop 8 — auto-combat: WeaponController (multi-weapon ticking), TargetingService, damage formula with crit, attack-speed integration.
