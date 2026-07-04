# MobCrush

A production-quality mobile survivor-style action roguelite inspired by Survivor.io, built with Unity.

## Development Process

This project is built using **loop engineering**: the work is divided into 26 loops (Loop 0–25), and each loop must produce a complete, reviewed deliverable before the next one begins. Every loop ends with Deliverables, Folder Structure, Scripts, Assets Needed, Risks, a Testing Checklist, and What Comes Next.

| Loop | Topic | Status |
|------|-------|--------|
| 0 | Game Production Blueprint | ✅ Complete |
| 1 | Product Vision | ✅ Complete |
| 2 | Game Design Document | ✅ Complete |
| 3 | Architecture | ✅ Complete |
| 4 | Core Framework | ✅ Complete |
| 5 | Player System | ✅ Complete |
| 6 | Enemy AI | ✅ Complete |
| 7 | Weapon System | ✅ Complete |
| 8 | Auto Combat | ✅ Complete |
| 9 | Projectile Framework | ✅ Complete |
| 10 | Experience System | ✅ Complete |
| 11 | Upgrade Selection | ✅ Complete |
| 12 | Enemy Spawner | ✅ Complete |
| 13 | Boss System | ✅ Complete |
| 14 | Equipment | ✅ Complete |
| 15 | Inventory | ✅ Complete |
| 16 | Meta Progression | ✅ Complete |
| 17 | UI | ✅ Complete |
| 18 | Audio | ✅ Complete |
| 19 | Visual Polish | ✅ Complete |
| 20 | Save System | ✅ Complete |
| 21 | Optimization | ✅ Complete |
| 22 | Testing | ✅ Complete |
| 23 | Live Operations | ✅ Complete |
| 24 | Store Publishing | ✅ Complete |
| 25 | Future Expansion | ✅ Complete |

Loop documents live in [`Docs/Loops/`](Docs/Loops/).

## Engineering Principles

- SOLID principles and Clean Architecture throughout.
- Everything modular; every system reusable.
- Data-driven via ScriptableObjects — no hardcoded values.
- Interfaces preferred over concrete coupling.
- Object pooling everywhere spawn rates matter.
- Mobile performance is a Day 1 requirement, not a post-launch fix.
