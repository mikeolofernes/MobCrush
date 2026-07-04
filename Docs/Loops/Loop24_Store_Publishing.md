# Loop 24 — Store Publishing

**Status:** Complete (release-preparation package; execution items are launch-window tasks with owners/gates listed).

## 1. Google Play
- **Build:** AAB (Play requires it), IL2CPP, ARM64 + ARMv7, target latest required API level, `minSdkVersion 26` (Loop 0 spec). R8 enabled; symbols uploaded for native crash symbolication.
- **Play Console setup:** internal → closed (soft-launch countries PH/ID) → production tracks; staged rollout 10 → 50 → 100%; pre-launch report on every closed build.
- **Data safety form:** with Null services only, "no data collected". The moment analytics/ads SDKs land, regenerate from each SDK's published data-safety mapping — mismatch is the #1 avoidable rejection.
- **Policy gates:** Families policy NOT targeted (13+ audience declaration), ads declaration (rewarded only, user-initiated), in-app purchases declared.

## 2. App Store
- **Build:** IL2CPP arm64, min iOS 15; App Privacy nutrition labels mirroring the Play data-safety answers; ATT prompt ONLY if an ads SDK uses IDFA (prefer SDKs that work without ATT at launch — higher opt-out rates cost more than contextual ads lose).
- **Review notes:** demo save with mid-game progression + "all content reachable without purchase" statement (true by design — Loop 1 monetization).
- **TestFlight:** external beta = the iOS soft launch.

## 3. Store assets checklist
| Asset | Spec | Note |
|---|---|---|
| Icon | 512 (Play) / 1024 (iOS) | Hero silhouette vs. swarm; readable at 48 px; no text |
| Screenshots | 6–8 per platform, phone + tablet | First 3 carry install decision: horde chaos, evolution moment, draft screen — captioned |
| Feature graphic (Play) | 1024×500 | Swarm closing on glowing player |
| Preview video | 15–30 s, portrait-safe | Gameplay only, first 5 s = peak chaos; no UI walkthrough |

## 4. Privacy Policy & Terms
- Hosted page (required by both stores even with zero collection); covers: what's stored locally, what SDKs collect post-integration, contact, deletion (local: delete-save button — `ISaveService.DeleteAll` already exists; GDPR/CCPA erasure path documented).
- Terms: virtual goods non-refundable where lawful, service availability, age requirement.
- Owner: needs legal review before production — flagged as an external dependency.

## 5. Age rating
IARC questionnaire (Play) + Apple questionnaire: fantasy violence vs. machines, no gore/language/gambling; contains IAP + ads → expected **ESRB E10+ / PEGI 7 / USK 6**. No loot boxes with real-money odds at v1.0 (chests bought with earned gems only) — avoids Belgium/Netherlands odds-disclosure complexity; revisit with legal if paid chests ship.

## 6. Monetization compliance
- IAP catalog: remove-ads, gem packs ×4, starter bundle (Play Console + App Store Connect, matching ids from one source-of-truth sheet).
- Rewarded placements marked user-initiated; restore purchases (iOS requirement) in settings; price localization on store defaults.

## 7. ASO
- **Title:** "MobCrush: Swarm Survivor" (genre keyword in title, both stores).
- **Keywords:** survivor, roguelike, bullet heaven, offline, arcade + localized sets (soft-launch locales first).
- **A/B:** Play Store listing experiments on icon (silhouette vs. action) and first screenshot — decision data before global launch.
- **Localization ladder:** EN → store-listing-only localization for top 10 locales → full in-game localization post-launch (TMP + id-based strings make this mechanical).

## Deliverables
✅ Both-store release plan, asset specs, privacy/terms scaffold requirements, age-rating position, monetization compliance list, ASO plan.

## Folder Structure
Doc only.

## Scripts
None.

## Assets Needed
Icon/screenshot/video source files (art team), hosted privacy policy page (legal), store console accounts.

## Risks
- Data-safety mismatch post-SDK integration (top rejection cause) — regeneration step is mandatory in the SDK-integration checklist.
- ATT opt-in economics on iOS — mitigated by choosing non-IDFA-dependent mediation at launch.

## Testing Checklist
- [ ] Pre-launch report clean on min-spec device (Play).
- [ ] IAP sandbox purchases + restore verified on both platforms.
- [ ] Store listing experiments configured before global rollout.

## What Comes Next
Loop 25 — Year 2 expansion plan.
