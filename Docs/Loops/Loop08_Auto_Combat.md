# Loop 8 — Auto Combat

**Status:** Complete

## Script explanations
- **`DamageCalculator`** (implements `IDamageResolver`) — the single damage formula: `weaponBase × (1 + DamagePercent%)`, then crit roll (`CritChance`, `× CritDamage`). Injectable RNG for deterministic tests. Every hit in the game routes through one function, so balance/telemetry changes have one home.
- **`WeaponBehaviourFactory`** — the only class that knows concrete behavior types; maps `WeaponType` → fresh `IWeaponBehaviour`. Projectile/Boomerang/Missile map to Loop 9's `ProjectileVolleyBehaviour` in its three modes.
- **`WeaponController`** — auto-attack orchestrator on the player: up to 6 simultaneous `WeaponInstance`s, per-frame cooldown ticking compressed by the player's `AttackSpeedPercent` (`cooldown / (1 + AS)`), add/find/slot-cap API for the upgrade drafter (Loop 11), full unequip on run teardown. Uses scaled `deltaTime` so pausing freezes all weapons for free.

## Design notes
- **Target selection** is deliberately per-behavior (nearest for lasers, cluster-seeking for zones, facing-cone for melee, per-projectile homing next loop) using `EnemySystem`'s allocation-free queries — a single global "current target" service would fight the genre's multi-weapon feel.
- **Cooldown & attack speed** live in `WeaponInstance.TickCooldown` (Loop 7), consumed here — the controller stays a thin scheduler.
- **Critical hits** carry through `DamageInfo.Critical` → `DamageDealtEvent` → damage numbers (Loop 19) with no extra plumbing.
- **Projectile pooling** requirement is satisfied structurally: behaviors can only spawn through `IPoolService` in their context.

## Deliverables
✅ Automatic multi-weapon combat: scheduling, attack speed, crit, one-place damage formula.

## Folder Structure (added)
```
Scripts/Gameplay/Combat/DamageCalculator.cs
Scripts/Gameplay/Weapons/{WeaponBehaviourFactory,WeaponController}.cs
```

## Scripts
3 new.

## Assets Needed
- Player prefab gains `WeaponController` (wired to PlayerController + EnemySystem) and a starting `SO_Weapon_` reference.

## Risks
- `WeaponBehaviourFactory` references Loop 9's `ProjectileVolleyBehaviour` — the repo compiles from Loop 9's commit onward (noted; loops 8+9 form one compile unit by dependency).

## Testing Checklist
- [x] Six weapons tick simultaneously from one Update.
- [x] +100% attack speed exactly halves intervals (formula verified in Loop 22 tests).
- [x] Crit math: base 5%/150% produces expected expected-value (unit test with seeded RNG).
- [x] Dead player stops all firing; pause freezes cooldowns (scaled time).

## What Comes Next
Loop 9 — reusable projectile framework (pierce, bounce, split, homing, explosion, lifetime, pooling) + the three projectile-family weapon behaviors.
