# Loop 3 — Software Architecture

**Status:** Complete
**Roles:** Senior Game Architect, C# Engineer, Performance Engineer

This is the binding technical design for all implementation loops (4+). No implementation in this loop.

---

## 1. Folder Structure (under `UnityProject/Assets/_Project/`)

```
Scripts/
├── Core/                  # Engine-agnostic-ish foundation (asm: MobCrush.Core)
│   ├── Services/          # ServiceLocator, service interfaces
│   ├── Events/            # EventBus + event structs
│   ├── Pooling/           # PoolService
│   ├── Save/              # SaveService, serializers, migrations
│   ├── Audio/             # AudioService
│   ├── Input/             # InputService (wraps Input System)
│   ├── SceneFlow/         # SceneLoaderService, Bootstrap, GameManager
│   └── Utils/             # Timers, math, extensions
├── Data/                  # ScriptableObject definitions only (asm: MobCrush.Data)
├── Gameplay/              # In-run systems (asm: MobCrush.Gameplay)
│   ├── Player/  ├── Enemies/  ├── Weapons/  ├── Projectiles/
│   ├── Combat/  ├── Experience/  ├── Upgrades/  ├── Spawning/  └── Bosses/
├── Meta/                  # Out-of-run systems (asm: MobCrush.Meta)
│   ├── Equipment/  ├── Inventory/  └── Progression/
├── UI/                    # Views & presenters (asm: MobCrush.UI)
└── LiveOps/               # Analytics/Ads/RemoteConfig interfaces + stubs (asm: MobCrush.LiveOps)
Data/                      # .asset instances (SO_*), mirrored by feature
Prefabs/  Art/  Audio/  Scenes/
Tests/
├── EditMode/              # asm: MobCrush.Tests.EditMode
└── PlayMode/              # asm: MobCrush.Tests.PlayMode
```

## 2. Namespaces
`MobCrush.Core.{Services,Events,Pooling,Save,Audio,Input,SceneFlow,Utils}`, `MobCrush.Data`, `MobCrush.Gameplay.{Player,Enemies,Weapons,Projectiles,Combat,Experience,Upgrades,Spawning,Bosses}`, `MobCrush.Meta.{Equipment,Inventory,Progression}`, `MobCrush.UI`, `MobCrush.LiveOps`. Namespace always equals folder path.

## 3. Assembly Definitions & Dependency Flow

```
MobCrush.Core          ← depends on: nothing first-party
MobCrush.Data          ← Core
MobCrush.Gameplay      ← Core, Data
MobCrush.Meta          ← Core, Data
MobCrush.UI            ← Core, Data, Gameplay*, Meta*   (*read-side only, via events/services)
MobCrush.LiveOps       ← Core
Tests                  ← everything
```
**Rules:** dependencies point inward/downward only; `Core` never references gameplay; `Gameplay` and `Meta` never reference each other directly (they communicate via events and save data); UI never mutates gameplay state directly — it raises intents through services. Why: keeps systems unit-testable, keeps compile times low, and makes each module reusable in future projects.

## 4. Managers & Services (all behind interfaces, resolved via ServiceLocator)

| Interface | Impl | Responsibility |
|---|---|---|
| `IEventBus` | `EventBus` | Typed pub/sub, struct events, zero-alloc dispatch |
| `IPoolService` | `PoolService` | Prefab pools, warm-up, auto-return |
| `ISceneLoader` | `SceneLoaderService` | Async additive scene flow + loading screen |
| `IAudioService` | `AudioService` | Music/SFX, mixer, pooled audio sources |
| `ISaveService` | `SaveService` | Versioned, encrypted, slotted persistence |
| `IInputService` | `InputService` | Move vector + UI actions from any device |
| `IGameStateMachine` | `GameManager` | Boot → Menu → Run → Paused → Results |
| `ITargetingService` | `TargetingService` | Spatially-bucketed enemy queries for auto-combat |
| `IRunSession` | `RunSession` | Per-run state (time, kills, level, drafted upgrades) |
| `IMetaService` | `MetaProgressionService` | Wallet, talents, unlocks |
| `IInventoryService` | `InventoryService` | Equipment storage & operations |

**Why Service Locator (with constructor-style init) over a DI framework:** zero reflection cost on mobile, trivially debuggable, and every consumer still codes against interfaces so tests inject fakes. Locator access is confined to composition roots (bootstrap, scene installers) — gameplay classes receive references, they don't fetch them.

## 5. ScriptableObjects (the data layer)
Every tunable is an SO: `PlayerStatsDefinition`, `EnemyDefinition`, `WeaponDefinition` (+ per-type behavior configs), `UpgradeDefinition`, `EvolutionRule`, `WaveTable`, `StageDefinition`, `BossDefinition`, `EquipmentDefinition`, `RarityTable`, `ExperienceCurve`, `TalentDefinition`, `EconomyConfig`, `AudioLibrary`, `GameConfig` (root). SOs are **read-only at runtime**; mutable state lives in plain C# session/save models. Why: SOs shared across scenes must never accumulate run state.

## 6. Interfaces (gameplay contracts)
`IDamageable`, `IDamageDealer`, `IPoolable`, `IWeaponBehaviour`, `IProjectileModifier`, `IEnemyBrain`, `IBossPhase`, `IPickup`, `ISaveMigration`, `IAnalyticsService`, `IAdsService`, `IRemoteConfigService`.

## 7. Event System
Single generic `EventBus`: `Subscribe<T>(Action<T>)`, `Publish<T>(in T)` where `T : struct, IGameEvent`. Struct events → no GC. Canonical events: `PlayerDamagedEvent`, `PlayerDiedEvent`, `EnemyKilledEvent`, `ExperienceGainedEvent`, `LevelUpEvent`, `UpgradeChosenEvent`, `WaveStartedEvent`, `BossSpawnedEvent`, `BossDefeatedEvent`, `RunEndedEvent`, `CurrencyChangedEvent`, `EquipmentChangedEvent`, `SaveCompletedEvent`. Why bus over UnityEvents: decouples assemblies, testable, no serialized-reference fragility.

## 8. Save System (design; impl Loops 4 & 20)
JSON (Newtonsoft) → AES encryption → atomic write (temp + rename) → 2 rotating backups. `SaveModel { int Version; ... }` with ordered `ISaveMigration` chain. Slots: `slot_0..2` + `settings` (unencrypted). Autosave on: run end, purchase, equip change, app pause. Cloud-ready: save blob is a single serializable payload with device timestamp for conflict resolution.

## 9. Audio (design; impl Loops 4 & 18)
One `AudioMixer` (Master → Music / SFX / UI). `AudioService` pools N (16) audio sources, throttles duplicate SFX per frame, crossfades music tracks, exposes intensity parameter for dynamic layering. All clips referenced through `AudioLibrary` SO ids — no direct clip references in gameplay code.

## 10. Pooling
`PoolService.Get(prefabId)` / `Release(instance)`; pools keyed by definition asset; warm-up counts declared per stage in `StageDefinition`. Pooled objects implement `IPoolable { OnSpawned(); OnDespawned(); }`. Enemies, projectiles, gems, VFX, damage numbers, audio sources — all pooled.

## 11. Addressables
Groups: `Boot` (local, always), `CoreGameplay` (local), `Stage_<n>` (local at launch, remote-capable), `UI`, `Audio_Music` (remote-capable). Labels per chapter enable future content updates without app store releases. All runtime loads via keys defined in SOs.

## 12. Scene Flow
`Boot` (bootstrap only, ~0 assets) → `Menu` ⇄ `Game` (+ additive `Stage_<n>` content scene). Loading screen scene additively overlays transitions. `GameManager` states: `Booting, MainMenu, LoadingRun, Playing, LevelUpPause, Paused, RunEnding, Results`. Pause = `Time.timeScale 0` + input-map switch (UI map stays live).

---

## Deliverables
✅ This architecture document (binding for Loops 4+).

## Folder Structure
Doc added; code folders will materialize exactly as §1 starting Loop 4.

## Scripts
None yet — signatures and contracts only.

## Assets Needed
None.

## Risks
- Locator misuse spreading through gameplay code → rule: locator only in composition roots; enforced in review.
- Assembly split too fine could slow iteration → six assemblies is deliberately coarse.

## Testing Checklist
- [x] Every system in the loop prompt mapped to an owner (service/SO/interface).
- [x] Dependency diagram acyclic; Core has zero first-party deps.
- [x] All Loop 0 coding-standard rules representable in this design (pooling, no hardcoding, interfaces).

## What Comes Next
Loop 4 — implement the core framework (bootstrap, locator, event bus, scene loader, audio, save, pooling, input, GameManager) plus the Unity project scaffold.
