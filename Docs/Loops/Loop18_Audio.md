# Loop 18 — Audio

**Status:** Complete (engine work was front-loaded in Loop 4's AudioService; this loop adds the gameplay-facing half).

## Script explanations
- **`GameplayAudioBinder`** — the single event→SFX mapping point: kills (spatialized, elite variant), player hurt, level-up, upgrade chosen, boss spawn/defeat. Gameplay systems stay silent-by-design; the whole soundscape is retunable in one inspector.
- **`DynamicMusicController`** — calm ⇄ intense stage-track crossfade driven by live enemy pressure with hysteresis (enter 60 / exit 35, evaluated 1×/s), hard switch to boss track on `BossSpawnedEvent`. Reuses AudioService's dual-source crossfade — no new plumbing.

## Recap of the Loop 4 audio engine (satisfies this loop's perf items)
16 pooled AudioSources (zero runtime AddComponent), round-robin voice stealing, per-frame duplicate-SFX throttle (max 2 same-id/frame — 100 dying enemies ≠ white noise), pitch variance per clip, dB-correct mixer volume mapping, id-based `AudioLibrary` SO.

## Mixer & import spec (editor-side)
Mixer: Master → Music / SFX / UI, exposed `MusicVolume`/`SfxVolume`; ~-3 dB headroom on Master; light limiter on SFX bus. Imports: SFX = Vorbis q0.35, Decompress On Load, mono; Music = Vorbis q0.5, Streaming; 22.05 kHz for UI blips.

## Deliverables
✅ Music, SFX, mixer wiring, volume persistence (Loop 17 SettingsView), dynamic music, performance measures.

## Folder Structure (added)
```
Scripts/Gameplay/Audio/{GameplayAudioBinder,DynamicMusicController}.cs
```

## Scripts
2 new.

## Assets Needed
- 3 music tracks (calm/intense/boss), ~12 SFX; populate `SO_AudioLibrary` with the ids referenced by the binder.

## Risks
- Calm/intense as separate tracks (not vertical stems) means crossfades lose musical phase — acceptable at this scope; stem-based layering is a drop-in upgrade behind the same controller.

## Testing Checklist
- [x] Hysteresis prevents rapid track flapping around the threshold.
- [x] Boss music overrides pressure logic permanently for the run.
- [x] SFX throttle bounds worst-case massacre frames.
- [x] All ids resolve via AudioLibrary (missing id = silent no-op, never a crash).

## What Comes Next
Loop 19 — visual polish (camera shake, hit flash, damage numbers, juice).
