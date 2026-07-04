using System;
using System.Collections.Generic;
using MobCrush.Data;
using MobCrush.Gameplay.Weapons;

namespace MobCrush.Gameplay.Upgrades
{
    /// <summary>
    /// Pure draft logic (no Unity lifecycle — fully unit-testable with a seeded RNG).
    /// Rules (GDD §9):
    ///  1. An eligible EVOLUTION is always offered, top slot.
    ///  2. Remaining slots roll rarity, then pick uniformly among eligible candidates.
    ///  3. Candidates: new weapons (if slot free), weapon level-ups (below max),
    ///     new passives (if slot free), passive level-ups (below max).
    ///  4. No duplicate offers in one draft; no dead picks by construction.
    ///  5. SynergyHint set when an option progresses an evolution pair the player holds half of.
    /// </summary>
    public sealed class DraftGenerator
    {
        private readonly UpgradePool _pool;
        private readonly Random _rng;
        private readonly List<DraftOption> _candidates = new(32);

        public DraftGenerator(UpgradePool pool, Random rng = null)
        {
            _pool = pool;
            _rng = rng ?? new Random();
        }

        /// <param name="weapons">Currently equipped weapon instances.</param>
        /// <param name="passiveLevels">Passive id → current level (absent = not owned).</param>
        /// <param name="weaponSlotsFree">Player has an open weapon slot.</param>
        /// <param name="passiveSlotsFree">Player has an open passive slot.</param>
        /// <param name="luck">Player Luck stat (0 = neutral).</param>
        public List<DraftOption> Generate(IReadOnlyList<WeaponInstance> weapons,
                                          IReadOnlyDictionary<string, int> passiveLevels,
                                          bool weaponSlotsFree, bool passiveSlotsFree, float luck)
        {
            var result = new List<DraftOption>(_pool.OptionsPerDraft); // once per level-up; allocation acceptable

            // Rule 1: eligible evolutions first (weapon max level + catalyst passive owned).
            for (int i = 0; i < weapons.Count && result.Count < _pool.OptionsPerDraft; i++)
            {
                var w = weapons[i];
                if (w.CanEvolve && !string.IsNullOrEmpty(w.Definition.RequiredPassiveId)
                    && passiveLevels.ContainsKey(w.Definition.RequiredPassiveId))
                {
                    result.Add(new DraftOption(DraftOptionKind.Evolution, w.Definition, null, Rarity.Legendary, true));
                }
            }

            BuildCandidates(weapons, passiveLevels, weaponSlotsFree, passiveSlotsFree);

            while (result.Count < _pool.OptionsPerDraft && _candidates.Count > 0)
            {
                // Rarity colors the card and scales passive magnitude at apply time;
                // pick among candidates is uniform so every option stays reachable.
                var rarity = _pool.RarityTable.Roll(_rng, luck);
                int pick = _rng.Next(_candidates.Count);
                var chosen = _candidates[pick];
                _candidates.RemoveAt(pick); // no duplicates within one draft

                result.Add(new DraftOption(chosen.Kind, chosen.Weapon, chosen.Passive, rarity, chosen.SynergyHint));
            }

            return result;
        }

        private void BuildCandidates(IReadOnlyList<WeaponInstance> weapons,
                                     IReadOnlyDictionary<string, int> passiveLevels,
                                     bool weaponSlotsFree, bool passiveSlotsFree)
        {
            _candidates.Clear();

            // Which catalyst passives do owned weapons want? (for synergy hints both directions)
            // Small n (≤6 weapons, ≤10 passives): linear scans beat set allocations.
            foreach (var weaponDef in _pool.Weapons)
            {
                var owned = FindOwned(weapons, weaponDef.Id);
                if (owned == null)
                {
                    if (weaponSlotsFree)
                        _candidates.Add(new DraftOption(DraftOptionKind.NewWeapon, weaponDef, null,
                            Rarity.Common, PassiveOwnedIsCatalystFor(weaponDef, passiveLevels)));
                }
                else if (!owned.IsMaxLevel)
                {
                    _candidates.Add(new DraftOption(DraftOptionKind.WeaponLevel, weaponDef, null,
                        Rarity.Common, HasCatalyst(owned.Definition, passiveLevels)));
                }
            }

            foreach (var passive in _pool.Passives)
            {
                bool ownedPassive = passiveLevels.TryGetValue(passive.Id, out int level);
                bool isCatalystForOwnedWeapon = IsCatalystForAny(passive.Id, weapons);

                if (!ownedPassive)
                {
                    if (passiveSlotsFree)
                        _candidates.Add(new DraftOption(DraftOptionKind.NewPassive, null, passive,
                            Rarity.Common, isCatalystForOwnedWeapon));
                }
                else if (level < passive.MaxLevel)
                {
                    _candidates.Add(new DraftOption(DraftOptionKind.PassiveLevel, null, passive,
                        Rarity.Common, isCatalystForOwnedWeapon));
                }
            }
        }

        private static WeaponInstance FindOwned(IReadOnlyList<WeaponInstance> weapons, string id)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i].Definition.Id == id) return weapons[i];
            return null;
        }

        private static bool HasCatalyst(WeaponDefinition weapon, IReadOnlyDictionary<string, int> passiveLevels) =>
            !string.IsNullOrEmpty(weapon.RequiredPassiveId) && passiveLevels.ContainsKey(weapon.RequiredPassiveId);

        private static bool PassiveOwnedIsCatalystFor(WeaponDefinition weapon, IReadOnlyDictionary<string, int> passiveLevels) =>
            HasCatalyst(weapon, passiveLevels);

        private static bool IsCatalystForAny(string passiveId, IReadOnlyList<WeaponInstance> weapons)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i].Definition.RequiredPassiveId == passiveId) return true;
            return false;
        }
    }
}
