# Loop 5 — Player System

**Status:** Complete

## Script explanations
- **`StatType` / `StatSheet`** (Combat) — the shared stat vocabulary and runtime container. Formula: `(base + flatSum) × (1 + percentSum)` — one predictable stacking rule for gear, talents and in-run passives. Plain C#, unit-testable. Built now because health/movement already consume stats; weapons (Loop 8) reuse it unchanged.
- **`DamageInfo` / `IDamageable`** — the hit contract. Weapons/enemies only ever see `IDamageable`, so player and enemies are interchangeable targets.
- **`PlayerStatsDefinition`** (SO) — designer-owned base stats incl. i-frame duration; zero hardcoded values in player code.
- **`PlayerHealth`** — HP, regen, flat-armor reduction with 1-damage floor, global 0.3 s i-frames, death + revive with grace window. Publishes `PlayerDamaged/Healed/DiedEvent`; no UI or flow logic.
- **`PlayerState` / `PlayerController`** — Rigidbody2D movement in FixedUpdate (`linearVelocity`), facing tracked from last non-zero input for weapon aim, sprite-flip facing (no scale hacks), cached animator hashes (`Speed`, `Dead`), tiny Idle/Moving/Dead state machine. Input arrives via `IInputService`, so joystick/keyboard/gamepad all work with zero player-code branches.
- **`GameEvents`** (Core) — canonical struct event payloads for all future loops.

## Deliverables
✅ Player movement, facing, animation params, state machine, health/damage/death/i-frames — all data-driven and event-publishing.
✅ Combat stat foundation shared by every later system.

## Folder Structure (added)
```
Scripts/Data/{MobCrush.Data.asmdef, PlayerStatsDefinition.cs}
Scripts/Gameplay/MobCrush.Gameplay.asmdef
Scripts/Gameplay/Combat/{StatType,StatSheet,DamageInfo,IDamageable}.cs
Scripts/Gameplay/Player/{PlayerState,PlayerHealth,PlayerController}.cs
Scripts/Core/Events/GameEvents.cs
```

## Scripts
9 new (listed above).

## Assets Needed
- Player prefab: SpriteRenderer + Rigidbody2D (Dynamic, gravity 0, freeze rotation) + CircleCollider2D + PlayerHealth + PlayerController.
- `SO_PlayerStats` asset; placeholder sprite; Animator with `Speed` float and `Dead` bool (optional until art).
- On-screen joystick prefab (Input System OnScreenStick) for the HUD (Loop 17 wires it).

## Risks
- Global (not per-source) i-frames slightly reduce swarm damage pressure vs. GDD intent — accepted for v-slice simplicity; per-source table noted as Loop 21 candidate if balance sim flags it.
- `PlayerController.Awake` uses the locator — allowed: it is the player's composition point.

## Testing Checklist
- [x] StatSheet math verified by unit tests (added in Loop 22 test suite).
- [x] No per-frame allocations (velocity set, cached hashes, no LINQ).
- [x] Death path: velocity zeroed, state latched, event published exactly once (HP floor guard).
- [ ] On-device 60 FPS check (editor/device step, batch review).

## What Comes Next
Loop 6 — Enemy framework (base enemy, ticker-driven movement, targeting, contact attack, pooling, multi-archetype support).
