# Post-Review Fixes

Code review (correctness + security) findings applied after Loop 25, as four commits:

## 1. Security hardening
- **Encrypt-then-MAC saves:** `AesPayloadTransform` is now AES-256-CBC + HMAC-SHA256 with a magic/version header, domain-separated enc/mac keys, fresh IV per write, constant-time MAC comparison. Tampering fails deterministically into SaveService's corrupt→backup→fresh path (plain CBC was malleable and only failed on lucky padding breaks).
- **Plaintext fallback removed:** raw JSON in a save file is rejected — the old "legacy migration" passthrough was a permanent cheat backdoor. Nothing had shipped, so there are no legacy saves to honor.
- **Stable A/B buckets:** `NullRemoteConfigService` uses FNV-1a instead of `string.GetHashCode` (not stable across runtimes → silent bucket reshuffles).
- Tests updated: plaintext rejected, tampered ciphertext rejected, cross-device key rejected.

## 2. Combat fixes
- **Boomerang:** forces effectively-infinite pierce; with data pierce 0 it previously died on its first hit and never returned.
- **Split safety:** children spawn only from the recorded prefab source; the `?? gameObject` fallback could have poisoned the pool with a live instance as key. Instances unregister from the prefab-source table on destroy (leak fix); split children re-register (so they can bounce/pierce properly).
- **Knockback implemented:** was declared in weapon data but never applied. `WeaponContext.Hit` now carries an impulse; `EnemyController` integrates and decays it; all behaviors + projectiles pass `stats.Knockback`. Bosses are deliberately immune.

## 3. Core/service fixes
- `EventBus`: instance-level handler storage (the static per-generic cache leaked every bus for process lifetime).
- `SaveService`: migration loop advances its tracked version with the JSON (was relying on an implicit monotonicity invariant).
- `PoolService`: HashSet live-set (O(1) release); `EnemySystem`+`EnemyController.LiveIndex` swap-remove (O(1) despawn) with a double-despawn guard.
- `OrbitalBehaviour`: sweeps only expired lockouts (the wholesale `Clear()` at 128 entries caused brief double-hits).
- `BossController`: `IPoolService` injected at `Initialize` — no locator access in gameplay code.
- `UpgradeDraftSystem`: fails fast at startup if the draft view isn't wired; only resumes `Playing` if it still owns the `LevelUpPause` (can't resurrect an ended run).
- `DailyMissionService`: `IDisposable` (prevents double subscriptions on re-composition).

## 4. Run flow + composition layer (the critical gap)
- **`RunController`** (Gameplay/Run): the missing run-lifecycle owner — kills/duration/level bookkeeping, death → revive window (`TryRevive` API for the ads/gems UI flow) → defeat, boss defeat → victory, combat teardown, and the **single publisher of `RunEndedEvent`** (previously nothing published it; results/autosave/missions all dead-ended).
- **`RunRewardGranter`** (Meta): converts `RunEndedEvent` into wallet/loot grants (coins per kill, victory coin/core bonus, one victory equipment drop) — Gameplay publishes facts, Meta owns rewards. Tunables added to `EconomyConfig`.
- **`MobCrush.App` assembly**: the outermost composition layer (only place allowed to see Core+Data+Gameplay+Meta+UI+LiveOps at once):
  - `AppInstaller` (Boot scene, persists): builds validated id→asset registries, constructs and registers wallet/equipment/inventory/missions/reward-granter + LiveOps null services, claims offline progress. Uses `Start()` so `GameBootstrap.Awake`'s registrations always precede it.
  - `MenuInstaller`: injects `InventoryService` into `InventoryView` (previously uncalled).
  - `GameSceneInstaller`: applies equipped-gear + talent stat bonuses to the player's StatSheet at run start (previously meta power had zero in-run effect), then refills HP so MaxHp bonuses arrive full.
- `PlayerHealth.RefillToFull()` added for the installer.

## Editor wiring added to the checklist
Boot scene: `AppInstaller` (economy + equipment + talent assets). Menu scene: `MenuInstaller`. Game scene: `GameSceneInstaller` + `RunController` (player/weapons/enemies/experience refs). Revive UI panel (calls `IAdsService.ShowRewarded` → `RunController.TryRevive`) is the one remaining unbuilt UI piece — API is in place.

## Known-accepted (documented, not fixed)
Local-clock trust for missions/offline (needs a server), key extractability (deterrence-only posture), boss `CoinReward` field currently superseded by EconomyConfig run rewards (candidate for per-boss override later).
