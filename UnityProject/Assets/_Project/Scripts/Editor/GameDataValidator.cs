using System.Collections.Generic;
using MobCrush.Data;
using UnityEditor;
using UnityEngine;

namespace MobCrush.Editor
{
    /// <summary>
    /// Authoring-time integrity check for all game data (menu: MobCrush → Validate Game Data).
    /// Catches the whole class of "asset wiring" bugs code cannot: empty/duplicate ids,
    /// incomplete evolution pairs, malformed level tables, stages with holes.
    /// Run it before every content commit; it is also safe to call from CI batch mode.
    /// </summary>
    public static class GameDataValidator
    {
        private static int _errors;

        [MenuItem("MobCrush/Validate Game Data")]
        public static void Validate()
        {
            _errors = 0;

            var weapons = LoadAll<WeaponDefinition>();
            var enemies = LoadAll<EnemyDefinition>();
            var passives = LoadAll<UpgradeDefinition>();
            var equipment = LoadAll<EquipmentDefinition>();
            var talents = LoadAll<TalentDefinition>();
            var stages = LoadAll<StageDefinition>();
            var bosses = LoadAll<BossDefinition>();

            CheckUniqueIds("WeaponDefinition", weapons, w => w.Id);
            CheckUniqueIds("EnemyDefinition", enemies, e => e.Id);
            CheckUniqueIds("UpgradeDefinition", passives, p => p.Id);
            CheckUniqueIds("EquipmentDefinition", equipment, e => e.Id);
            CheckUniqueIds("TalentDefinition", talents, t => t.Id);
            CheckUniqueIds("StageDefinition", stages, s => s.Id);
            CheckUniqueIds("BossDefinition", bosses, b => b.Id);

            ValidateWeapons(weapons, passives);
            ValidateEnemies(enemies);
            ValidateStages(stages);
            ValidateTalents(talents);

            if (_errors == 0)
                Debug.Log($"GameDataValidator: OK — {weapons.Count} weapons, {enemies.Count} enemies, " +
                          $"{passives.Count} passives, {equipment.Count} equipment, {talents.Count} talents, " +
                          $"{stages.Count} stages, {bosses.Count} bosses. No issues.");
            else
                Debug.LogError($"GameDataValidator: {_errors} issue(s) found — see errors above.");
        }

        private static void ValidateWeapons(List<WeaponDefinition> weapons, List<UpgradeDefinition> passives)
        {
            var passiveIds = new HashSet<string>();
            foreach (var p in passives) passiveIds.Add(p.Id);

            foreach (var w in weapons)
            {
                if (w.Levels == null || w.Levels.Length == 0)
                    Error(w, $"Weapon '{w.Id}': no level rows.");

                if (w.EffectPrefab == null)
                    Error(w, $"Weapon '{w.Id}': EffectPrefab not set (nothing will spawn/render).");

                // Evolution pair completeness: both halves or neither (GDD §4).
                bool hasForm = w.EvolvedForm != null;
                bool hasCatalyst = !string.IsNullOrEmpty(w.RequiredPassiveId);
                if (hasForm != hasCatalyst)
                    Error(w, $"Weapon '{w.Id}': evolution needs BOTH EvolvedForm and RequiredPassiveId (has {(hasForm ? "form" : "catalyst")} only).");
                if (hasCatalyst && !passiveIds.Contains(w.RequiredPassiveId))
                    Error(w, $"Weapon '{w.Id}': RequiredPassiveId '{w.RequiredPassiveId}' matches no UpgradeDefinition.");
                if (hasForm && w.EvolvedForm == w)
                    Error(w, $"Weapon '{w.Id}': evolves into itself.");
            }
        }

        private static void ValidateEnemies(List<EnemyDefinition> enemies)
        {
            foreach (var e in enemies)
            {
                if (e.Prefab == null)
                    Error(e, $"Enemy '{e.Id}': Prefab not set.");
                if (e.Behavior == EnemyBehavior.Ranged && e.ProjectilePrefab == null)
                    Error(e, $"Enemy '{e.Id}': Ranged behavior without ProjectilePrefab (will never attack).");
            }
        }

        private static void ValidateStages(List<StageDefinition> stages)
        {
            foreach (var s in stages)
            {
                if (s.Waves.Count == 0)
                    Error(s, $"Stage '{s.Id}': no waves.");
                foreach (var wave in s.Waves)
                {
                    if (wave.Enemy == null)
                        Error(s, $"Stage '{s.Id}': wave with no enemy assigned.");
                    if (wave.EndTime <= wave.StartTime)
                        Error(s, $"Stage '{s.Id}': wave window [{wave.StartTime}, {wave.EndTime}] is empty or inverted.");
                }
                foreach (var elite in s.Elites)
                    if (elite.Enemy == null)
                        Error(s, $"Stage '{s.Id}': elite injection at {elite.Time}s with no enemy.");
                if (s.Boss == null)
                    Debug.LogWarning($"GameDataValidator: Stage '{s.Id}' has no boss (fine for Endless, wrong for campaign).", s);
            }
        }

        private static void ValidateTalents(List<TalentDefinition> talents)
        {
            var ids = new HashSet<string>();
            foreach (var t in talents) ids.Add(t.Id);
            foreach (var t in talents)
                if (!string.IsNullOrEmpty(t.PrerequisiteId) && !ids.Contains(t.PrerequisiteId))
                    Error(t, $"Talent '{t.Id}': prerequisite '{t.PrerequisiteId}' does not exist (node unreachable).");
        }

        private static void CheckUniqueIds<T>(string label, List<T> assets, System.Func<T, string> idOf)
            where T : Object
        {
            var seen = new Dictionary<string, T>();
            foreach (var asset in assets)
            {
                string id = idOf(asset);
                if (string.IsNullOrEmpty(id)) { Error(asset, $"{label} '{asset.name}': empty Id."); continue; }
                if (!seen.TryAdd(id, asset))
                    Error(asset, $"{label}: duplicate id '{id}' in '{asset.name}' and '{seen[id].name}'.");
            }
        }

        private static List<T> LoadAll<T>() where T : ScriptableObject
        {
            var results = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) results.Add(asset);
            }
            return results;
        }

        private static void Error(Object context, string message)
        {
            _errors++;
            Debug.LogError($"GameDataValidator: {message}", context);
        }
    }
}
