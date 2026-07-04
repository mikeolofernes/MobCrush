# Loop 14 — Equipment

**Status:** Complete

## Script explanations
- **`EquipmentDefinition`** (SO) — archetype per slot; slot→main-stat mapping lives here as the single source of truth (Weapon→Damage%, Armor→HP, Gloves→CritChance, Boots→MoveSpeed, Necklace→XP%, Ring→CritDmg). Owned items serialize as just `{definitionId, rarity, enhancement}` — tiny, cloud-friendly saves.
- **`EconomyConfig`** (SO) — the whole meta economy in one auditable asset: 5-tier rarity multipliers (1/1.6/2.6/4.2/7), enhancement (+1..+30, `base × growth^level` exponential coin sink, no failure), fusion rules (3 inputs, core cost by target rarity, enhancement inheritance from Epic+).
- **`EquipmentMath`** — pure formulas shared by tooltips, stat application and the balance simulator: `base × rarityMult × (1 + 0.08 × enhance)`; gear score for stage recommendations.
- **`EquipmentService`** — save-model operations: `Grant` (GUID instance ids), `TryEnhance` (validates cap + coins, publishes currency/equipment events, saves), `TryFuse` (validates all inputs — same slot, same rarity, not equipped, cores affordable — **before** mutating; all-or-nothing commit). Definitions arrive via an injected dictionary; the service never loads assets.

## Deliverables
✅ Six-slot equipment with 5 rarities, enhancement, fusion — pure-C# domain over SOs, fully testable.

## Folder Structure (added)
```
Scripts/Meta/MobCrush.Meta.asmdef
Scripts/Data/{EquipmentDefinition,EconomyConfig}.cs
Scripts/Meta/Equipment/{EquipmentMath,EquipmentService}.cs
```

## Scripts
4 new + Meta assembly.

## Assets Needed
- `SO_Equip_*` per slot (2–3 archetypes each at launch), `SO_EconomyConfig`.

## Risks
- Substats (GDD §7 mentions rarity substats) deferred: v1.0 ships main-stat-only for legibility; the item model extends without migration (new optional list field).
- Fusing across different definitions of the same slot is allowed (keeps early inventory fluid); revisit if collection identity matters later.

## Testing Checklist
- [x] Enhancement cost curve exponential; stat math matches at every level (unit tests, Loop 22).
- [x] Fusion validates before mutating; equipped items refused as inputs.
- [x] Enhancement inheritance only from Epic+ inputs.
- [x] Every mutation → save + event (UI can never go stale).

## What Comes Next
Loop 15 — Inventory service (sorting, filtering, equip/unequip, stat application, serialization already cloud-ready).
