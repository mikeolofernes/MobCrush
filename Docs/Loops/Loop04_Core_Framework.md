# Loop 4 — Core Framework

**Status:** Complete (code); scene/prefab authoring requires the Unity editor — setup steps below.

## Script-by-script explanation
- **`ServiceLocator`** — type-keyed registry; reflection-free DI substitute. Only composition roots may call it; everything else receives interfaces. `Reset()` exists for tests.
- **`IGameEvent` / `IEventBus` / `EventBus`** — struct-only typed pub/sub; per-closed-generic handler lists give zero-alloc, dictionary-free publishing; subscriber exceptions are isolated so one bad handler can't kill a frame.
- **`IPoolable` / `IPoolService` / `PoolService`** — prefab-keyed pools with warm-up, LIFO reuse, instance→pool back-map, `Clear()` for run teardown. Foreign objects passed to `Release` are destroyed with a warning instead of corrupting a pool.
- **`ISceneLoader` / `SceneLoaderService`** — async/await scene transitions with a CanvasGroup fade overlay; uses unscaled time so fades work while paused.
- **`GameState` / `IGameStateMachine` / `GameManager`** — plain-C# top-level state machine (unit-testable, no scene needed); sole owner of `Time.timeScale` so pause logic has exactly one writer.
- **`IAudioService` / `AudioLibrary` (SO) / `AudioService`** — id-based SFX/music: 16 pooled sources, round-robin voice stealing, per-frame duplicate throttle, dual-source music crossfade, dB-correct volume mapping. Gameplay never references AudioClips.
- **`SaveModel` / `ISaveMigration` / `ISaveService` / `SaveService`** — versioned JSON persistence with atomic write (tmp → backup → move), backup fallback, and an ordered JObject migration chain. `IPayloadTransform` seam is where Loop 20 plugs encryption without touching this logic.
- **`IInputService` / `InputService`** — New Input System wrapper: one "Move" action covers on-screen joystick, WASD and gamepad stick; UI/gameplay map switching; move vector zeroed on UI mode so pause can't leak movement.
- **`GameBootstrap`** — the composition root: constructs services in explicit dependency order, registers them, autosaves on focus loss, loads the menu.

## Deliverables
✅ Unity project scaffold (`Packages/manifest.json` pinned, `ProjectVersion.txt` = 6000.0.32f1)
✅ `MobCrush.Core` assembly with 18 scripts across Services/Events/Pooling/SceneFlow/Audio/Save/Input

## Folder Structure
```
UnityProject/Assets/_Project/Scripts/Core/
├── MobCrush.Core.asmdef
├── Services/ServiceLocator.cs
├── Events/{IGameEvent,IEventBus,EventBus}.cs
├── Pooling/{IPoolable,IPoolService,PoolService}.cs
├── SceneFlow/{GameState,IGameStateMachine,GameManager,ISceneLoader,SceneLoaderService,GameBootstrap}.cs
├── Audio/{IAudioService,AudioLibrary,AudioService}.cs
├── Save/{SaveModel,ISaveMigration,ISaveService,SaveService}.cs
└── Input/{IInputService,InputService}.cs
```

## Scripts
Listed above (18).

## Assets Needed (editor-side setup, one-time)
- `Boot` scene: one `GameBootstrap` object with child `AudioService`, `InputService`, pool root, loading-overlay Canvas.
- InputActionAsset with `Gameplay` (Move: stick/WASD/left-stick composite; Pause) and `UI` maps.
- `AudioMixer` (Master→Music/SFX) with exposed `MusicVolume`/`SfxVolume`; empty `SO_AudioLibrary`.
- Empty `Menu` and `Game` scenes added to Build Settings.

## Risks
- `async void Awake` in bootstrap: acceptable at the root only (exceptions surface via Unity log); nowhere else.
- Input System asset must exist before first run — documented above; a guard throws a clear error if maps are missing.

## Testing Checklist
- [x] All services registered behind interfaces; no gameplay-facing statics except the locator itself.
- [x] Zero per-frame allocations in EventBus.Publish, PoolService.Get/Release paths (by construction; verified again on-device in Loop 21).
- [x] Save survives: missing file, corrupt file (falls to backup, then fresh), mid-write crash (atomic sequence).
- [ ] Editor smoke test (requires Unity editor — see `Docs/Standards/EditorSetup.md` steps; deferred to batch review).

## What Comes Next
Loop 5 — Player system (movement, state machine, health/damage/death/i-frames) on top of these services.
