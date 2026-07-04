# Loop 0 — Game Production Blueprint

**Project codename:** MobCrush
**Status:** Complete — awaiting approval to proceed to Loop 1
**Roles active in this loop:** Product Manager, Senior Game Architect, Technical Artist, QA Lead

This document defines the project's scope before any design or code exists. Every later loop is bound by the decisions made here; changing them later is a formal scope change, not a drive-by edit.

---

## 1. Core Inspiration

### What we keep from Survivor.io

| Element | Why we keep it |
|---|---|
| One-finger movement, fully automatic combat | The genre's core accessibility hook; it's what makes 5-minute sessions on a phone viable. |
| Horde survival with escalating waves + timed bosses | The proven tension curve: quiet start → screen-filling chaos → boss spike. |
| In-run upgrade drafts (pick 1 of 3 on level-up) | The moment-to-moment decision layer that makes runs feel authored by the player. |
| Weapon evolution (base weapon + passive → evolved form) | The mid-run "jackpot" moment that drives build planning. |
| Meta progression: equipment, rarity, enhancement, talent tree | The long-term retention layer; runs feed permanent power. |
| Stage/chapter structure with difficulty tiers | Clear goals and a reason to replay with stronger builds. |

### What we make original

- **Theme & tone:** Not zombies. MobCrush uses a stylized "rogue signal" sci-fi theme — the player is a lone drone-commander carving through corrupted machine swarms. This keeps art scope controllable (mechanical enemies tolerate lower animation fidelity than organic ones) and differentiates on the store page.
- **Synergy-first upgrade design:** Instead of Survivor.io's mostly-independent weapons, our upgrade pool is built around explicit cross-weapon synergies (defined in data, Loop 11), so drafting is about combos, not just rarity.
- **Honest difficulty scaling:** Difficulty curves are data-driven and visible in design docs, tuned for skill expression rather than pure stat-check paywalls. Monetization accelerates progress; it never gates completion.
- **No energy system at launch.** Session count is not throttled. Retention comes from content, not gates. (Revisit only with data, Loop 23.)

### Non-goals for version 1.0 (explicitly out of scope)

- Multiplayer of any kind (co-op/PvP is Year 2, Loop 25).
- 3D gameplay. The game is 2D top-down with sprite rendering.
- User-generated content, chat, or social systems beyond leaderboard-style comparisons.
- Simultaneous PC/Steam launch (see platforms below).

---

## 2. Target Platforms

| Platform | Priority | Notes |
|---|---|---|
| Android | **Primary** | Largest genre audience; first market for soft launch. Min spec: Android 8.0 (API 26), 3 GB RAM, Mali-G52 / Adreno 610 class GPU. Target 60 FPS on mid-tier, locked 30 FPS floor on min spec. |
| iOS | **Primary** | Ships with Android at global launch. Min spec: iPhone 8 / iOS 15. |
| Steam (PC) | **Deferred (post-launch)** | Architecture must not preclude it: input layer (Loop 5) abstracts touch/keyboard/gamepad from Day 1, and no plugin choice may be mobile-only without a desktop fallback. But no Steam work is scheduled before Year 2 planning. |

**Reference devices** (buy/borrow before Alpha): a min-spec Android (e.g., Galaxy A14), a mid-tier Android (e.g., Pixel 6a), an iPhone 8 or SE 2, and a current flagship of each.

---

## 3. Engine and Package Decisions

**Engine: Unity 6 LTS (6000.x)**, locked for the project's life. Why Unity: mature 2D tooling, best-in-class mobile deployment, Addressables for content delivery, and the team's C# requirement. Why 6 LTS specifically: long support window through launch + Year 1 LiveOps, and GPU Resident Drawer/URP improvements help mobile batching. The exact patch version is pinned in `ProjectVersion.txt` and only bumped deliberately at milestone boundaries.

**Render pipeline: URP (2D Renderer).** Mobile-first, supports 2D lights if we want them for polish (Loop 19), and is the only pipeline Unity actively optimizes for mobile.

### Package baseline (locked at Loop 4, pinned in `manifest.json`)

| Package | Purpose | Decision rationale |
|---|---|---|
| Input System (new) | All input | Single API for touch, keyboard, gamepad → satisfies the platform-abstraction rule. |
| Addressables | Asset loading & content updates | Required by architecture (Loop 3); enables remote content for LiveOps without app updates. |
| URP | Rendering | See above. |
| TextMeshPro | All text | Non-negotiable for localization-ready UI. |
| Unity Test Framework | Unit/playmode tests | Required by Definition of Done. |
| Newtonsoft JSON (com.unity.nuget.newtonsoft-json) | Save serialization | Versioned, migration-friendly saves (Loop 20). |
| DOTween (asset) *or* built-in tweening | UI/juice animation | Decide at Loop 17; if DOTween, use the free version and wrap it behind an interface so it's swappable. |

**Deliberately excluded:** ECS/DOTS (complexity not justified — pooled MonoBehaviours with struct-based data hit our enemy counts; revisit only if Loop 21 profiling proves otherwise), third-party "game kit" assets (they fight Clean Architecture), and any analytics/ads SDKs before Loop 23 (SDK bloat is a top mobile perf killer; they enter behind interfaces defined in Loop 3).

### Repository layout

```
MobCrush/
├── README.md
├── .gitignore
├── Docs/
│   ├── Loops/            # One markdown file per completed loop (this file is the first)
│   ├── GDD/              # Game design document (Loop 2)
│   └── Standards/        # Coding standards, art specs (grow from this doc)
└── UnityProject/         # The Unity project root (created in Loop 4)
    └── Assets/
        ├── _Project/     # ALL first-party content lives under this single folder
        │   ├── Scripts/
        │   ├── Art/
        │   ├── Audio/
        │   ├── Data/     # ScriptableObject assets
        │   ├── Prefabs/
        │   └── Scenes/
        └── Plugins/      # Third-party only
```

The `_Project` convention keeps first-party assets separated from imported packages, which keeps Addressables grouping and version control sane. Full folder/namespace/assembly design is Loop 3's job.

---

## 4. Asset Pipeline

- **Art style:** Stylized 2D sprites, flat-shaded with strong silhouettes. Enemies read at ~64–128 px on-screen height. Style guide with palette and silhouette rules is a Loop 2/17 deliverable.
- **Sprite delivery:** Source art in a separate art repository or cloud drive (never PSDs in the game repo). Game repo receives exported PNGs only, power-of-two atlases via Unity Sprite Atlas (ASTC compression on both platforms).
- **Placeholder policy:** Loops 4–16 use primitive/placeholder art (colored shapes + free CC0 packs such as Kenney). Placeholder assets live in `Art/_Placeholder/` and are tracked in an asset-replacement checklist so nothing placeholder ships. Real art integration is scheduled at Loops 17–19.
- **Audio delivery:** Source WAV (48 kHz) in the art store; Unity import settings force Vorbis compression, `Decompress on Load` for short SFX, `Streaming` for music. Audio spec finalized in Loop 18.
- **Naming convention:** `TYPE_Category_Name_Variant` (e.g., `SPR_Enemy_Crawler_01`, `SFX_Weapon_LaserFire`, `SO_Weapon_Boomerang`). Enforced by review; a validation editor script is added in Loop 4.
- **Addressables:** Every runtime-loaded asset goes through Addressables groups organized by feature and load-timing (defined in Loop 3). No `Resources/` folder, ever.

---

## 5. Coding Standards

Full standards doc grows in `Docs/Standards/`; these are the binding rules from Day 1:

1. **Language/style:** C# with `.editorconfig`-enforced formatting. PascalCase for types/methods/properties, camelCase for locals/parameters, `_camelCase` for private fields. One class per file; file name matches type name.
2. **Architecture:** SOLID + Clean Architecture. Gameplay code depends on interfaces and events, never on concrete managers. Domain logic (damage formulas, XP curves) lives in plain C# classes testable without Unity.
3. **Data-driven:** All tunable values (damage, cooldowns, curves, spawn rates, prices) live in ScriptableObjects. A numeric literal in gameplay code is a code-review rejection, except mathematical constants and obvious identities (0, 1).
4. **No singletons-by-`static`:** Access to services goes through the Service Locator / injection seam defined in Loop 4, so every consumer is testable with fakes.
5. **Pooling:** Anything instantiated more than once per run (enemies, projectiles, gems, damage numbers, VFX) must go through the Pool Manager. `Instantiate` in a gameplay `Update` path is a review rejection.
6. **Allocation discipline:** No LINQ, `foreach` over interfaces, string concatenation, or boxing in per-frame paths. Cache component lookups. Target: 0 B/frame steady-state GC allocation in gameplay.
7. **Events:** Cross-system communication via the Event Bus (Loop 4). Direct references only within a single feature module.
8. **Comments:** Every script has a header comment stating its responsibility; public APIs get XML doc comments; non-obvious decisions get a `// why:` comment. Comments explain *why*, not *what*.
9. **Async:** Coroutines only for simple, self-contained timing; `async/await` (Awaitable in Unity 6) for loading flows. Never both in one system.

---

## 6. Git Branching Strategy

Trunk-based with short-lived feature branches — right-sized for a small team with loop-scoped work:

- **`main`** — always buildable and shippable. Protected; changes arrive only by reviewed PR. Every merged PR must pass CI (build + tests).
- **`loop/<nn>-<topic>`** — one branch per loop (e.g., `loop/04-core-framework`). A loop's branch merges to `main` only when the loop's Definition of Done is met and the loop document is approved. *(Current session note: work in this environment lands on the designated remote branch `claude/survivor-io-loop-engineering-m1ob2o`, which plays the role of the active loop branch.)*
- **`fix/<issue>`** — hotfixes branched from `main`, merged back immediately.
- **Releases:** tags on `main` (`v0.1.0-slice`, `v0.5.0-alpha`, `v0.9.0-beta`, `v1.0.0`). A `release/x.y` branch is cut only if we need to stabilize while `main` moves on.
- **Commits:** imperative, scoped messages: `Loop 4: Add object pool manager with warm-up support`. No "wip" commits reach `main`.
- **Binary assets:** Git LFS for PNG/WAV/FBX from the moment real assets enter (Loop 17). `.gitignore` excludes all Unity-generated folders (already committed).
- **Serialization settings:** Force Text serialization + Visible Meta Files (Unity default) so prefabs/scenes diff and merge. One-scene-per-person discipline; scene merge conflicts are resolved by rebuild, never by hand-editing YAML.

---

## 7. Milestone Schedule

Loops map onto four production milestones. Durations assume a small team working steadily; the loop sequence, not the calendar, is the contract.

| Milestone | Loops | Exit criteria | Target |
|---|---|---|---|
| **M0 — Pre-production** | 0–3 | Blueprint, vision, GDD, and architecture approved. Zero code debt because there's zero code. | Weeks 1–3 |
| **M1 — Vertical Slice** | 4–13 | One stage playable end-to-end on a real mid-tier phone: move, auto-fight, level up, draft upgrades, survive waves, kill a boss. 60 FPS mid-tier / 30 FPS min-spec with 150+ active enemies. Placeholder art acceptable. | Weeks 4–14 |
| **M2 — Alpha (feature-complete)** | 14–20 | All v1.0 systems in: equipment, inventory, meta progression, full UI, audio, polish pass, save system. Content may be thin; nothing new starts after this gate. | Weeks 15–24 |
| **M3 — Beta (content-complete + hardened)** | 21–22 | All launch content in; optimization and test loops done; crash-free rate ≥ 99.5% in internal testing; soft-launch build submitted. | Weeks 25–30 |
| **Launch** | 23–24 | LiveOps stack live, store assets approved, global release. | Weeks 31–34 |
| **Year 2 planning** | 25 | Expansion roadmap approved. | Post-launch |

Schedule risk rule: if a milestone slips more than 20%, we cut content (stages, weapons), never quality gates.

---

## 8. Definition of Done (applies to every feature, every loop)

A feature is **Done** only when all of the following hold:

1. **Designed:** Covered by the loop document / GDD section; deviations documented.
2. **Clean:** Follows the coding standards above; passed code review; no new warnings.
3. **Data-driven:** All tunables in ScriptableObjects; a designer can rebalance it without touching code.
4. **Tested:** Domain logic has unit tests; systems have playmode tests where feasible; the loop's testing checklist executed and recorded.
5. **Performant:** Profiled on the mid-tier reference device; no frame spikes attributable to the feature; 0 B steady-state GC allocation in its per-frame paths; pooled where the pooling rule applies.
6. **Integrated:** Works in the real game flow (bootstrap → menu → run), not only in a test scene.
7. **Resilient:** Handles its failure cases (missing data asset, interrupted load, save corruption where relevant) without crashing.
8. **Documented:** Script header comments present; the loop document's Deliverables section updated.

"Done except…" is not Done. Partial features stay on their branch.

---

## 9. Technical Risks and Mitigation

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **Enemy-count performance** — hundreds of enemies + projectiles tank mobile frame rate. | High | Critical | Pooling from Day 1; single-manager tick loops instead of per-enemy `Update()` (Loop 6); sprite atlasing + batching; hard perf gate in the M1 exit criteria (150+ enemies at 60 FPS mid-tier); profile on-device every loop from Loop 5 on. |
| 2 | **GC spikes** — C# allocation habits cause frame hitches. | High | High | Allocation rules in coding standards; per-loop profiler check; Incremental GC on; CI-adjacent allocation tests for hot paths. |
| 3 | **Architecture over-engineering** — Clean Architecture zeal slows the vertical slice. | Medium | Medium | Loop 3 defines *just enough* seams (events, pooling, save, services); YAGNI applies to everything else; the M1 date is the forcing function. |
| 4 | **Balancing complexity** — synergy-first upgrades explode the tuning space. | Medium | High | All balance in ScriptableObjects + curves; build a headless simulation harness in Loop 22 to batch-test builds; cut synergies rather than ship unbalanced ones. |
| 5 | **Save-data corruption/migration** — live players lose progress after an update. | Medium | Critical | Versioned save schema with migration path designed in Loop 20 *before* launch; checksummed writes; automatic backup slot; never change schema without a migration test. |
| 6 | **Third-party SDK bloat** (ads/analytics) degrades startup and stability. | Medium | Medium | SDKs enter only at Loop 23, behind interfaces defined in Loop 3; measure cold-start and crash rate before/after each SDK; ad mediation limited to one provider at launch. |
| 7 | **Store rejection** (privacy, data-safety, ad content ratings). | Low | High | Loop 24 checklist starts from current Play/App Store policy; privacy policy and data inventory maintained from the moment analytics enters. |
| 8 | **Scope creep** — "just one more weapon/system." | High | Medium | This blueprint's non-goals list; any scope addition requires a written trade (what gets cut); loop gates make creep visible. |
| 9 | **Single-engine-version risk** — a Unity regression blocks us. | Low | Medium | Pin the LTS patch; upgrade only at milestone boundaries on a branch with full regression pass. |

---

## Loop Closing Sections

### Deliverables
- ✅ This blueprint (`Docs/Loops/Loop00_Production_Blueprint.md`)
- ✅ Repository initialized: `README.md` with loop tracker, Unity-ready `.gitignore`, `Docs/` structure
- ✅ Locked decisions: engine (Unity 6 LTS + URP 2D), platforms (Android/iOS primary), repo layout, branching model, coding standards baseline, DoD, milestone map

### Folder Structure (as of this loop)
```
MobCrush/
├── README.md
├── .gitignore
└── Docs/
    └── Loops/
        └── Loop00_Production_Blueprint.md
```

### Scripts
None — by design. Loop 0 is pre-code; the first scripts appear in Loop 4.

### Assets Needed
None yet. Action item carried forward: acquire reference devices (min-spec Android, mid-tier Android, iPhone 8/SE 2) before Loop 5's on-device profiling begins.

### Risks
See §9. Top three to watch immediately: enemy-count performance (drives Loop 3/6 architecture), over-engineering (drives Loop 3 scoping), scope creep (drives every approval gate).

### Testing Checklist
- [x] Repo clones cleanly; `.gitignore` covers all Unity-generated paths under `UnityProject/`
- [x] README loop tracker matches the 26-loop plan
- [x] Blueprint covers all nine required topics (inspiration, platforms, engine/packages, asset pipeline, coding standards, branching, milestones, DoD, risks)
- [x] Every locked decision includes its rationale
- [ ] Human review: product owner confirms non-goals and platform priorities *(your approval of this loop)*

### What Comes Next
**Loop 1 — Product Vision:** genre, core gameplay and loop, target audience, retention strategy, meta progression, monetization, difficulty progression, session length, USP, roadmap, and tech stack confirmation. No code. Loop 1 starts only after this blueprint is approved.
