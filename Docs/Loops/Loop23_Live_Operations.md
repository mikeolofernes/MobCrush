# Loop 23 — Live Operations

**Status:** Complete (runtime seams + daily missions implemented; SDK adapters and dashboards are launch-window integration tasks per the plan below).

## Script explanations
- **`IAnalyticsService` / `IAdsService` / `IRemoteConfigService`** (MobCrush.LiveOps, depends on Core only) — the SDK seams the whole game codes against. Real SDKs become adapter classes swapped at the composition root; SDK bloat never leaks into gameplay/meta assemblies (Loop 0 risk #6), and SDK-free builds remain one switch away.
- **`NullAnalyticsService` / `NullAdsService` / `NullRemoteConfigService`** — null-object defaults: analytics logs in-editor only; rewarded ads auto-succeed in dev builds (revive/chest flows playable) but **never** grant in production without an SDK; remote config returns defaults and derives stable A/B buckets from device id + experiment key.
- **`DailyMissionService`** (Meta) — 5 missions/day (GDD §11), local-midnight reset via date stamp, event-driven progress (kills batched to the run-end save — no disk writes on the kill path), claim → wallet coins, all-5 → gem bonus. Catalog is id-keyed so remote config can retune targets/rewards without migration.

## LiveOps launch plan (the ops half)
- **Daily rewards:** login streak table (7-day cycle, escalating; day 7 = gem chest) — reuses the mission claim/wallet pattern; ships with the first content update.
- **Events:** stage modifiers via remote config keys (`event.stage_id`, `event.multiplier`) driving the existing StageDefinition selection — no client update needed for weekend XP events.
- **Battle pass:** season model = mission-XP track (free + premium lanes) on top of DailyMissionService counters; IAP-gated premium lane; targeted v1.1.
- **Ads placements:** revive (1/run), post-run 2× reward, daily free chest — all through `IAdsService.ShowRewarded` with server-side-verifiable placement ids when an SDK with SSV lands.
- **Analytics events (minimum viable):** session_start, run_start/end (stage, duration, kills, level, victory), draft_pick, iap, ad_watched, mission_claimed — enough for the D1/D7 funnel and difficulty tuning.
- **A/B testing:** `GetExperimentBucket` + remote-config values; first experiments: XP curve exponent, draft rarity weights.

## Deliverables
✅ Daily missions runtime; ads/analytics/remote-config/A-B seams with safe defaults; events/battle-pass/rewards operating plan.

## Folder Structure (added)
```
Scripts/LiveOps/{MobCrush.LiveOps.asmdef, LiveOpsInterfaces, NullLiveOpsServices}.cs
Scripts/Meta/Progression/DailyMissionService.cs
```

## Scripts
3 new files + asmdef.

## Assets Needed
- Missions UI panel (Loop 17 pattern); SDK choices at integration: one mediation provider, one analytics provider (decision gate: crash-rate + cold-start impact measured before/after, Loop 0 risk #6).

## Risks
- Local-clock daily reset is cheatable — acceptable pre-server; remote time check rides along with the first online feature.
- `enhance_once` over-credits on any gear change — documented, deliberate QoL.

## Testing Checklist
- [x] Mission reset exactly once per date change; progress capped at target; double-claim refused.
- [x] Kill-path progress does zero disk writes.
- [x] Prod NullAdsService never grants; dev flow completes.
- [x] A/B bucket stable across calls for the same device+key.

## What Comes Next
Loop 24 — store publishing preparation (both stores, assets, policies, ASO).
