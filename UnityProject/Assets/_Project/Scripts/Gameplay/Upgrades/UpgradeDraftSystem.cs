using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.SceneFlow;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Player;
using MobCrush.Gameplay.Weapons;
using UnityEngine;

namespace MobCrush.Gameplay.Upgrades
{
    /// <summary>
    /// Orchestrates the Survivor-style draft (GDD §9): on LevelUpEvent → freeze the game
    /// (GameState.LevelUpPause) → generate options → show view → apply choice → resume.
    /// Owns the passive ledger (id → level) and applies passive stat effects to the
    /// player's StatSheet; weapon effects route to WeaponController.
    /// Level-ups arriving while a draft is open queue up (one big gem = several drafts back-to-back).
    /// </summary>
    public sealed class UpgradeDraftSystem : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private WeaponController _weapons;
        [SerializeField] private UpgradePool _pool;
        [SerializeField] private MonoBehaviour _viewBehaviour; // must implement IUpgradeDraftView (wired in Loop 17)

        public const int MaxPassives = 6; // GDD §4 slot cap

        private readonly Dictionary<string, int> _passiveLevels = new();
        private IUpgradeDraftView _view;
        private IEventBus _events;
        private IGameStateMachine _state;
        private DraftGenerator _generator;
        private int _pendingDrafts;
        private bool _draftOpen;

        public IReadOnlyDictionary<string, int> PassiveLevels => _passiveLevels;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _state = ServiceLocator.Get<IGameStateMachine>();
            _generator = new DraftGenerator(_pool);

            _view = _viewBehaviour as IUpgradeDraftView;
            if (_view == null)
                throw new System.InvalidOperationException(
                    $"UpgradeDraftSystem on '{name}': _viewBehaviour must implement IUpgradeDraftView " +
                    "(wire UpgradeDraftView here). Failing at startup beats an NRE on the first level-up.");

            _events.Subscribe<LevelUpEvent>(OnLevelUp);
        }

        private void OnDestroy() => _events.Unsubscribe<LevelUpEvent>(OnLevelUp);

        private void OnLevelUp(LevelUpEvent _)
        {
            _pendingDrafts++;
            if (!_draftOpen) OpenDraft();
        }

        private void OpenDraft()
        {
            if (_pendingDrafts <= 0) return;
            _pendingDrafts--;
            _draftOpen = true;
            _state.Set(GameState.LevelUpPause);

            var options = _generator.Generate(
                _weapons.Weapons,
                _passiveLevels,
                weaponSlotsFree: _weapons.HasFreeSlot,
                passiveSlotsFree: _passiveLevels.Count < MaxPassives,
                luck: _player.Stats.Get(StatType.Luck));

            if (options.Count == 0) { CloseDraft(); return; } // everything maxed — rare but must not soft-lock

            _view.Show(options, index => Apply(options[index]));
        }

        private void Apply(DraftOption option)
        {
            switch (option.Kind)
            {
                case DraftOptionKind.NewWeapon:
                    _weapons.TryAddWeapon(option.Weapon);
                    break;

                case DraftOptionKind.WeaponLevel:
                    _weapons.Find(option.Weapon.Id)?.LevelUp();
                    break;

                case DraftOptionKind.Evolution:
                    _weapons.Find(option.Weapon.Id)?.Evolve();
                    break;

                case DraftOptionKind.NewPassive:
                case DraftOptionKind.PassiveLevel:
                    ApplyPassive(option.Passive, option.Rarity);
                    break;
            }

            _events.Publish(new UpgradeChosenEvent(
                option.Kind is DraftOptionKind.NewPassive or DraftOptionKind.PassiveLevel
                    ? option.Passive.Id : option.Weapon.Id));

            CloseDraft();
        }

        private void ApplyPassive(UpgradeDefinition passive, Rarity rarity)
        {
            _passiveLevels.TryGetValue(passive.Id, out int level);
            _passiveLevels[passive.Id] = level + 1;

            // Rarity scales the magnitude of THIS pick (GDD §9: rarity = magnitude, not validity).
            float value = passive.ValuePerLevel * _pool.RarityTable.GetValueMultiplier(rarity);
            if (passive.IsPercent) _player.Stats.AddPercent(passive.Stat, value);
            else _player.Stats.AddFlat(passive.Stat, value);
        }

        private void CloseDraft()
        {
            _view.Hide();
            _draftOpen = false;

            if (_pendingDrafts > 0) { OpenDraft(); return; } // chained level-ups draft immediately

            // Only resume if WE still own the pause — if the run ended while a draft was
            // queued, stomping RunEnding/Results with Playing would resurrect a dead run.
            if (_state.Current == GameState.LevelUpPause)
                _state.Set(GameState.Playing);
        }
    }
}
