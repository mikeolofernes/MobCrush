# Production Readiness Assessment

**Verdict (2026-07-05): NOT ready to ship.** The codebase is feature-complete at the systems level and the code-side gaps found in review are now fixed, but a mobile game ships as *built, tuned, tested content* — and that half lives outside this repository's code. This document is the honest gap list and the order to close it.

## ✅ Closed in the readiness fix pass (code)
- **Pause flow** — `PauseMenuView` now consumes `IInputService.PauseRequested` (toggle pause/resume, quit-to-menu forfeit). Previously the pause signal had no listener at all.
- **Revive flow** — `ReviveView` (rewarded ad → `RunController.TryRevive`), with a real revive window on `RunController` (6 s, skippable via `DeclineRevive`, one revive per run, no prompt when ads can't serve).
- **Daily missions UI** — `MissionsView` (rows, progress, claim), injected by `MenuInstaller`; mission set now re-checks the date on app focus (midnight-in-background bug).
- **Achievements** — `AchievementService` (tiered lifetime counters → gem claims, persisted in `SaveModel.LifetimeCounters`, no schema change) + `WeaponEvolvedEvent` feeding the evolutions counter.
- **Data validator** — `MobCrush → Validate Game Data` editor menu: unique ids, complete evolution pairs, level tables, stage wave sanity, talent prerequisite reachability. Run before every content commit; CI-callable.

## 🔴 Blockers that require the Unity editor / humans / accounts

### 1. The project has never been compiled or run
No Unity installation exists in this environment. First action: open in Unity 6000.0.32f1, fix whatever the compiler flags, then run the EditMode test suite. Everything below assumes this passes.

### 2. Editor authoring (per-loop "Assets Needed" checklists, consolidated)
- Scenes: `Boot` (GameBootstrap + AppInstaller), `Menu` (MenuInstaller + views), `Game` (player, EnemySystem, SpawnSystem, BossSpawner, RunController, GameSceneInstaller, HUD/draft/revive/pause canvases).
- Prefabs: player, 6 enemy archetypes, projectiles, gems, boss, VFX, damage numbers, UI widgets.
- Assets: `SO_*` instances for every definition (player stats, enemies, weapons+evolutions, passives, pools, rarity, stages, bosses, economy, talents, audio library), InputActions (Gameplay/UI maps), AudioMixer, Addressables groups.
- **All art and audio is placeholder** — real sprite/SFX/music production is unstarted.

### 3. Content & balance
- Data exists conceptually for ~1 chapter; launch spec (GDD) is 5 chapters, 12 weapons, 3 bosses, Daily Challenge + Endless modes, stage-select UI (currently the Play button loads one hardcoded scene).
- Zero playtesting; balance simulator (Loop 22 plan) not yet built; XP/economy curves are untested first guesses.

### 4. Quality gates (Loop 21/22, unexecuted)
- PlayMode integration tests (need scene fixtures).
- On-device profiling matrix: 150+ enemies at 60 FPS mid-tier / 30 FPS min-spec, 0 B/frame steady-state GC, memory < 400 MB, cold start < 4 s, soak + interrupt tests.
- Crash-free ≥ 99.5% over an internal test cycle.

### 5. Launch infrastructure (Loop 23/24, planned not executed)
- Real ads mediation + analytics SDK adapters (interfaces ready; Null implementations never grant in prod).
- IAP catalog + restore purchases; store accounts; data-safety/privacy nutrition labels regenerated after SDK integration.
- Hosted privacy policy + terms (legal review); age rating questionnaires; store assets (icon, screenshots, video); soft launch in test markets before global.

### Recommended order to ship
1. Compile + tests green → 2. Author the three scenes/prefabs with placeholder art → **playable vertical slice** → 3. On-device perf gate → 4. Content production + balance sim → 5. Real art/audio → 6. SDKs + store prep → 7. Soft launch → 8. Global.

Realistically this is the M1→Launch stretch of the Loop 0 milestone plan (weeks, not days) — the code you have is the foundation for it, not a substitute.
