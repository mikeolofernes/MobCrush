using MobCrush.Core.Services;
using MobCrush.Gameplay.Player;
using MobCrush.Meta.Inventory;
using MobCrush.Meta.Progression;
using UnityEngine;

namespace MobCrush.App
{
    /// <summary>
    /// Game scene composition: applies persistent meta power (equipped gear + talents) to
    /// the player's in-run StatSheet at run start (post-review fix — without this, gear
    /// and talents had zero in-run effect). This is the one sanctioned crossing between
    /// Meta and Gameplay, and it happens in App, the outermost layer (Loop 3 §3).
    ///
    /// Start(): runs after PlayerController.Awake built the StatSheet and PlayerHealth
    /// initialized. RefillToFull afterwards so MaxHp bonuses arrive filled.
    /// </summary>
    public sealed class GameSceneInstaller : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;

        private void Start()
        {
            if (_player == null)
            {
                Debug.LogError("GameSceneInstaller: player reference not wired.");
                return;
            }

            // TryGet: the Game scene must stay playable standalone in the editor (no Boot,
            // no meta services) — the run then simply starts at base stats.
            if (ServiceLocator.TryGet<InventoryService>(out var inventory))
                Apply(inventory.GetEquippedStatBonuses());

            if (ServiceLocator.TryGet<MetaProgressionService>(out var progression))
                Apply(progression.GetTalentStatBonuses());

            _player.Health.RefillToFull();
        }

        private void Apply(System.Collections.Generic.List<(MobCrush.Data.StatType Stat, float Value, bool IsPercent)> bonuses)
        {
            foreach (var (stat, value, isPercent) in bonuses)
            {
                if (isPercent) _player.Stats.AddPercent(stat, value);
                else _player.Stats.AddFlat(stat, value);
            }
        }
    }
}
