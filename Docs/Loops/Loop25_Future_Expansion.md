# Loop 25 — Future Expansion (Year 2 Plan)

**Status:** Complete — final loop of the v1.0 program.

## Sequencing principle
Ship social-lite before social-heavy: Pets → Maps/Weapons/Bosses cadence → Seasonal Events → Co-op → Guilds → PvP. Each step reuses the previous step's infrastructure; nothing requires a rewrite because the v1.0 seams were built for it (noted per feature).

## Q1: Content cadence + Pets
- **New maps (chapters 6–9):** pure data — `StageDefinition` + spawn tables + one new hazard mechanic per chapter. Pipeline below makes this a content-team motion, not engineering.
- **New weapons (13–16) + evolutions:** `WeaponDefinition` assets over existing behaviors; at most one new `IWeaponBehaviour` (e.g. chain-lightning) per quarter.
- **Pets:** companion entity = orbital-style follower with its own small `StatSheet` + one auto-ability; collection/rarity/fusion reuses the equipment model wholesale (`EquipmentSlot.Pet` extension + `OwnedEquipment` — save schema handles it via one migration).
- **New bosses (per chapter):** `BossDefinition` pattern remixes + 1–2 new `BossPattern` entries (laser sweep, arena zones).

## Q2: Seasonal events + Battle pass maturity
- **Seasonal events:** remote-config-driven stage modifiers + event currency + event shop. Event currency = one more wallet id (wallet is string-keyed by design). Limited-time stages are Addressables remote groups — shipped without app updates (groups were labeled for this in Loop 3).
- **Season structure:** 8-week seasons aligning battle pass, event theme and a headline weapon.

## Q3: Co-op (the big lift)
- **Scope:** 2-player online co-op runs; drop-in not required v1.
- **Architecture:** deterministic-lockstep is wrong for this genre (variable enemy counts, physics-free is a plus though); choose client-hosted relay (Unity Multiplayer Services / NGO) with host-authoritative enemies and mirrored player inputs. The prep that pays off: all gameplay mutations already flow through services/events (interceptable), no gameplay statics, and `RunSession` state is serializable.
- **Explicit new work:** networked spawn ids for pooled objects, RTT-tolerant pickup arbitration, disconnect → solo continuation.
- **Gate:** technical spike (2 sprints) before commitment; co-op ships only if the spike holds 60 FPS with 250 mirrored enemies on mid-tier.

## Q4: Guilds + PvP-lite
- **Guilds:** server-backed (first real backend dependency): chat-less v1 (avoid moderation surface) — roster, guild missions (aggregate counters = DailyMissionService pattern), guild shop.
- **PvP:** asynchronous first — weekly ghost-race leaderboard (same seed, same draft offers, compare survival time; deterministic enough because RNG is already injectable). Real-time PvP explicitly NOT planned — genre fit is poor and the cost is co-op×3.

## Content pipeline (the enabler for all of the above)
1. **Authoring:** designer creates SO assets (stage/wave/boss/weapon) in-editor with validation editor scripts (id uniqueness, curve sanity, evolution pair completeness).
2. **Balance:** headless simulator batch (Loop 22 harness) gates every content PR — clear-rate matrix must land in band.
3. **Delivery:** Addressables remote groups per chapter/event; content updates decoupled from app releases.
4. **Cadence target:** 1 chapter + 1 weapon + 1 event theme per 8-week season with 1 content designer + 0.25 engineer.

## Deliverables
✅ Year 2 roadmap: co-op, PvP-lite, guilds, pets, maps, seasonal events, weapons, bosses, and the content pipeline that feeds them — each mapped to existing seams and honest about new infrastructure.

## Folder Structure
Doc only.

## Scripts
None.

## Assets Needed
Per-quarter content briefs (art/audio) at season kickoff.

## Risks
- Co-op is the existential scope risk — hence the spike gate and Q3 placement (after two quarters of revenue data justify it).
- Backend introduction (guilds) brings live-service ops cost; deferred to last deliberately.

## Testing Checklist
- [x] Every Year 2 feature traced to an existing seam or an explicitly-costed new one.
- [x] No feature requires modifying Loop 4–13 core systems' contracts.

## What Comes Next
Nothing in this program — the 26-loop plan is complete. Next actions belong to production: editor-side asset/scene authoring (each loop's "Assets Needed"), device profiling gates, and soft launch.
