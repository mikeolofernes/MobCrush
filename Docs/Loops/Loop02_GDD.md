# Loop 2 — Game Design Document (loop record)

**Status:** Complete. The GDD itself lives at `Docs/GDD/GameDesignDocument.md` (living document).

## Deliverables
✅ Full GDD covering player, enemies, bosses, weapons, skills, items, equipment, currencies, upgrade system, stage system, daily missions, achievements, game modes, unlockables, and balancing philosophy.

## Folder Structure
```
Docs/
├── GDD/GameDesignDocument.md   (new)
└── Loops/Loop02_GDD.md         (new)
```

## Scripts
None (design loop).

## Assets Needed
None yet. The GDD's weapon/enemy tables define the v1.0 art list that Loop 17–19 will consume.

## Risks
- 7 weapons × evolutions is the vertical-slice ceiling; weapons 8–12 are Beta content — do not pull them forward.
- Fusion economy (3→1) can starve early players; mitigate with mission-driven equipment income; simulator verifies in Loop 22.

## Testing Checklist
- [x] Every mechanic listed in the loop prompt has a numbered GDD section.
- [x] All numbers marked as ScriptableObject-owned initial values.
- [x] Consistent with Loop 1 vision (no energy gate, synergy-first drafting).

## What Comes Next
Loop 3 — software architecture (folders, namespaces, assemblies, dependency flow, systems design). Still no implementation.
