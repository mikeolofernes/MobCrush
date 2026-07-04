# MobCrush — Game Design Document (v1.0 scope)

Companion doc: `Docs/Loops/Loop02_GDD.md` (loop record). This is the living GDD; all numbers here are **initial values** that live in ScriptableObjects, never in code.

---

## 1. Player
- **Movement:** 8-directional analog, speed base 5 u/s (stat-modified). No dash at v1.0 (candidate upgrade).
- **Stats:** MaxHP, HPRegen/s, MoveSpeed, BaseDamage%, AttackSpeed%, CritChance, CritDamage (base 150%), Armor (flat reduction), PickupRadius, XPGain%, CoinGain%, ReviveCount.
- **Damage intake:** contact damage per enemy tick (0.5 s contact cooldown per enemy); on-hit **invincibility frames 0.3 s** vs. same source only.
- **Death:** HP ≤ 0 → revive prompt (rewarded ad or gem, once/run) → else Result screen.

## 2. Enemies (v1.0 archetypes; all data-driven)
| Archetype | Behavior | Notes |
|---|---|---|
| Crawler | Walks at player, contact damage | Fodder; 60% of spawns |
| Sprinter | Fast, low HP, lunges | Punishes standing still |
| Bulwark | Slow, high HP, high damage | Body-blocks corridors |
| Spitter | Keeps range, fires slow projectile | Introduces dodging |
| Swarmling | Spawns in rings around player | Magnet-check moment |
| Exploder | Runs in, detonates AoE after telegraph | Kite or burst it |
**Elites:** any archetype ×(HP 8–15, dmg 2, size 1.4) + 1 affix (Shielded, Hasted, Regenerating, Splitting). Guaranteed chest drop.

## 3. Bosses
- One per stage, spawns at stage timer end (or at fixed sub-boss timestamps in later chapters).
- **Framework:** phases by HP% (100/60/25), each phase = pattern set (aimed volley, radial burst, charge, minion summon, arena hazard). Telegraphs ≥ 0.6 s. Enrage timer 90 s per phase (soft: +10% dmg/10 s).
- **v1.0 bosses:** Chapter 1 "Forge Warden" (charge + radial), Ch.2 "Broodmother Node" (summons + spit arcs), Ch.3+ variants with remixed patterns.
- Rewards: guaranteed equipment drop (rarity table by chapter/difficulty), coins, first-clear gems.

## 4. Weapons (in-run; base + evolution)
| Base weapon | Type | Evolution (needs passive) |
|---|---|---|
| Pulse Blade | Melee arc | + Power Cell → **Nova Edge** (360° wave) |
| Rail Pistol | Projectile | + Targeting Chip → **Twin Railgun** (pierce beam volley) |
| Arc Laser | Laser sweep | + Heat Sink → **Prism Array** (multi-beam) |
| Shock Mine | AoE ground | + Blast Amp → **Quake Field** (pulsing zone) |
| Orbit Drones | Orbitals | + Gyro Core → **Drone Storm** (dual ring + contact stun) |
| Boomerang Disc | Boomerang | + Mag Glove → **Twin Cyclone** (2 discs, pierce) |
| Seeker Pod | Missiles | + Payload Rack → **Hydra Battery** (salvo + splash) |
- Max 6 weapon slots + 6 passive slots per run. Each weapon: 5 levels → evolution at Lv5 + matching passive Lv1+.

## 5. Skills / Passives (in-run)
Power Cell (+damage%), Targeting Chip (+crit), Heat Sink (+attack speed), Blast Amp (+area), Gyro Core (+duration), Mag Glove (+pickup radius), Payload Rack (+projectile count), Plating (+armor), Servo Legs (+move speed), Repair Nanites (+regen). 5 levels each.

## 6. Items (in-run pickups)
Energy Core (XP; small/medium/large), Coin, Magnet (vacuum all cores), Med Kit (+30% HP), Bomb (clear-screen damage), Chest (elite/boss drop → 1/3/5 random upgrades).

## 7. Equipment (meta; 6 slots)
Slots: Weapon, Armor, Gloves, Boots, Necklace, Ring. Each rolls a main stat by slot (Weapon=ATK, Armor=HP, Gloves=CritChance, Boots=MoveSpeed, Necklace=XPGain, Ring=CritDamage) + rarity substats.
**Rarities:** Common → Rare → Epic → Legendary → Mythic (stat multiplier ×1 / 1.6 / 2.6 / 4.2 / 7).
**Enhancement:** +1…+30 with coins; cost curve exponential; no destruction/failure at v1.0.
**Fusion:** 3 identical-rarity same-slot items → 1 next-rarity item (keeps highest enhancement of inputs at Epic+).

## 8. Currencies
- **Coins** (soft): run rewards, missions → enhancement, talents.
- **Gems** (hard): first-clears, achievements, IAP → chests, revives, cosmetics.
- **Cores** (material): boss/elite drops → fusion fees, research.
No currency caps; anti-inflation via exponential sinks.

## 9. Upgrade System (in-run drafting)
- Level-up → pause → 3 options (4 with Reroll talent). Weighted rarity: Common 55 / Rare 28 / Epic 13 / Legendary 4 (luck-stat modified).
- Pools: new weapon, weapon level, new passive, passive level, evolution (when eligible — always shown, top slot, distinct VFX).
- **Synergy hints:** options that progress an evolution pair show a link icon.
- Banish/skip: 1 banish per run (talent-unlockable more).

## 10. Stage System
- **Chapters** 1–5 at launch; each = 15 min survival + boss, distinct spawn table & hazard.
- Difficulty tiers per chapter: Normal / Hard (×2.5 enemy stats, better drops) / Nightmare (×6).
- Stage select shows recommended gear score.

## 11. Daily Missions
5/day (kill X, clear stage, enhance once, open chest, watch ad-chest). Each → coins; all 5 → daily gem bonus. Resets 00:00 local.

## 12. Achievements
Lifetime counters (kills, clears, evolutions, fusions, talent milestones) in tiers (bronze/silver/gold) → gems. ~40 at launch.

## 13. Game Modes
- **Campaign** (core), **Daily Challenge** (fixed seed + modifier, 1 attempt, leaderboard-lite), **Endless** (unlocks after Ch.3 clear; scaling until death, milestone rewards).

## 14. Unlockables
Weapons 8–12 via research; chapters via progression; difficulty tiers via clears; Endless mode; cosmetic trails (gems).

## 15. Balancing Philosophy
1. **Curves, not constants:** every scaler (XP, HP, cost) is an AnimationCurve/formula asset a designer can retune.
2. **Time-to-kill targets:** fodder dies in ≤2 hits early, ≤4 mid-run at on-curve power; if players fall behind, spawner pressure (not damage sponges) creates failure.
3. **Draft fairness:** no dead picks — every option must be net-positive; rarity changes magnitude, not validity.
4. **Meta ceiling:** gear/talents contribute ≤ 60% of total power at chapter-appropriate content; drafting skill covers the rest.
5. **Simulate before shipping:** headless run simulator (Loop 22) batch-tests builds vs. spawn tables; tune to 35–55% clear rate at recommended gear score.
6. **One knob per hotfix:** balance patches change the fewest values that fix the metric.
