# Loop 17 — UI

**Status:** Complete (all view code; canvas prefabs are editor-authored per the setup notes below).

## Script explanations
- **`HudView`** — HP/XP fill bars (fillAmount, no layout rebuilds), level, timer (string alloc once per second via `TMP SetText`), kill counter. Pure read-side event subscriber — UI never mutates gameplay (Loop 3 rule).
- **`UpgradeDraftView`** — implements Loop 11's `IUpgradeDraftView`: up to 4 cards, rarity coloring, synergy badge (the USP surfaced), staggered scale-in on **unscaled time** (game is frozen), tap-guard until animation ends, first-tap-wins disarm.
- **`ResultsView`** — victory/defeat header + run stats from `RunEndedEvent`; navigation intent only (reward granting is run-flow logic).
- **`MainMenuView`** — play flow (LoadingRun → scene → Playing), event-driven currency labels, panel toggles.
- **`SettingsView`** — music/SFX sliders → `IAudioService`, persisted in PlayerPrefs (device preferences intentionally bypass the encrypted progress save).
- **`InventoryView`** — grid of pre-instantiated pooled widgets (no Instantiate on refresh), slot filter + sort dropdowns, tap-to-equip, merge-all, gear score; refreshes purely from `EquipmentChangedEvent`. Service injected by the scene installer.

## Responsive strategy (editor-side, documented for prefab authoring)
Canvas Scaler: Scale With Screen Size, reference 1920×1080, match 0.5; safe-area wrapper component on the HUD root for notches; all interactables ≥ 88 px touch targets; portrait-safe anchoring (HUD corners, draft cards center-stacked).

## Deliverables
✅ HUD, menus, inventory screen, settings, upgrade screen, result screen code; shop screen deferred to Loop 23 (it is meaningless before IAP/ads exist — scope honesty over empty chrome).

## Folder Structure (added)
```
Scripts/UI/{MobCrush.UI.asmdef, HudView, UpgradeDraftView, ResultsView, MainMenuView, SettingsView, InventoryView}.cs
```

## Scripts
6 views + asmdef; `InventoryService.GetDefinitionFor` helper added.

## Assets Needed
- Canvas prefabs per view (layout per responsive strategy above), TMP font asset, icon placeholder set, on-screen joystick prefab wired to the Input System.

## Risks
- Views resolve services via locator in Awake — acceptable: views are leaf composition points; InventoryView takes constructor-style injection where a service isn't locator-registered.

## Testing Checklist
- [x] Zero per-frame string/GC allocations (SetText numeric overloads, once-per-second timer).
- [x] Draft animation runs at timeScale 0; double-tap cannot double-apply.
- [x] Currency/equipment labels refresh on events from any system.
- [ ] Device-matrix layout pass (editor step, batch review).

## What Comes Next
Loop 18 — audio content integration & dynamic music layer.
