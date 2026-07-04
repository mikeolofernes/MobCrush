# Loop 1 — Product Vision

**Status:** Complete
**Roles:** Product Manager, Game Designer, Senior Game Architect

## Game Genre
Top-down **survivor-style action roguelite** (bullet-heaven / horde survival) with RPG meta progression. 2D, portrait-friendly landscape, single-hand playable.

## Core Gameplay
You are a **drone-commander carving through corrupted machine swarms** ("the Mob"). You move; your weapons fire automatically. Survive timed stages (~15 min) against escalating waves, collect energy cores (XP), draft upgrades on level-up, evolve weapons through synergies, and defeat the stage boss.

## Target Audience
- **Primary:** Casual-midcore mobile players 18–40 who play Survivor.io, Vampire Survivors, Archero. Short sessions, progression-driven, tolerant of ads-for-rewards.
- **Secondary:** Roguelite enthusiasts attracted by deeper synergy drafting.
- Markets: NA/EU/SEA soft launch order: PH/ID → Nordics → global.

## Core Gameplay Loop
1. **Pick stage** → 2. **Survive & draft** (move, auto-fight, level up, choose 1 of 3 upgrades) → 3. **Beat boss / die** → 4. **Collect rewards** (coins, equipment, materials) → 5. **Spend in meta** (enhance gear, talents) → 6. **Return stronger**. Loop time: 5–18 minutes.

## Retention Strategy
- **D1:** Fast first-run power fantasy; first evolution achievable in run 1–2; starter gear gift.
- **D7:** Daily missions, chapter unlock cadence (a new stage/mode every 1–2 days of normal play), first talent-tree milestone.
- **D30:** Equipment fusion chase (rarity ladder), achievements, weekly challenge mode, battle pass (post-launch, Loop 23).
- No energy gate: play is never blocked; retention comes from goals, not throttles.

## Meta Progression
- **Equipment:** 6 slots (weapon, armor, gloves, boots, necklace, ring), 5 rarities, enhancement levels, fusion (3× same → next rarity).
- **Talent tree:** permanent stat nodes bought with coins; branch choices for build identity.
- **Research/unlocks:** new base weapons, characters (post-launch), stages, difficulty tiers.

## Monetization (accelerate, never gate)
- **Rewarded ads:** revive (1/run), post-run reward multiplier, daily free chest.
- **IAP:** remove-ads pack, gem currency for chests/energy-free skips, starter/value bundles, battle pass (Loop 23).
- **No pay-to-win walls:** every stage clearable free; payers save time.

## Difficulty Progression
- In-run: wave HP/count/speed scale on data-driven curves; elite injections at fixed timestamps; boss at stage end.
- Meta: chapters 1→N with rising gear-score expectation; difficulty tiers (Normal/Hard/Nightmare) replay layer.
- Skill expression: dodging elites/boss patterns matters; stats alone never trivialize patterns until over-tiered.

## Session Length
Target median **6–10 min**; full stage clear 15 min; meta-only "check-in" sessions 1–2 min. All UI flows resumable and interruption-safe (calls, backgrounding).

## USP
1. **Synergy-first drafting** — upgrades are designed as combo pieces, with visible synergy hints in the draft UI.
2. **No energy system** — unlimited play at launch.
3. **Readable machine-swarm aesthetic** — high-contrast silhouettes tuned for small screens and huge crowd counts.

## Roadmap
- **v0.1 Vertical Slice** (Loops 4–13): 1 stage, 6 weapons, 1 boss, core loop complete.
- **v0.5 Alpha** (14–20): meta systems, UI, audio, save.
- **v0.9 Beta** (21–22): 5 chapters, 12 weapons, 3 bosses, optimized & tested.
- **v1.0 Launch** (23–24): LiveOps, stores.
- **Year 2** (25): co-op, PvP, guilds, pets, seasonal events.

## Tech Stack
Unity 6 LTS · URP 2D · C# · New Input System · Addressables · TextMeshPro · Newtonsoft JSON · Unity Test Framework. (Locked in Loop 0 §3.)

---

## Deliverables
✅ This vision document.

## Folder Structure
`Docs/Loops/Loop01_Product_Vision.md` added; no other changes.

## Scripts
None (by rule: no code in this loop).

## Assets Needed
None yet. Forward note: style-guide moodboard needed before Loop 17.

## Risks
- Genre saturation → mitigated by USP 1/3 and store-page differentiation.
- "No energy" reduces monetization pressure points → offset with rewarded-ad placements and battle pass; revisit with soft-launch data only.

## Testing Checklist
- [x] All 12 requested topics covered.
- [x] Vision consistent with Loop 0 non-goals (no multiplayer/3D at v1.0).
- [ ] Product owner approval (deferred to batch review).

## What Comes Next
Loop 2 — full Game Design Document.
