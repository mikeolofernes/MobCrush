using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using MobCrush.Gameplay.Enemies;
using MobCrush.Gameplay.Player;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons
{
    /// <summary>
    /// The auto-combat orchestrator on the player (GDD: combat is fully automatic).
    /// Owns up to MaxWeapons simultaneous WeaponInstances, ticks their cooldowns with
    /// the player's attack-speed stat, and exposes add/level/evolve for the upgrade
    /// system (Loop 11). Target selection lives in behaviors via EnemySystem queries —
    /// each weapon type wants a different policy (nearest, cluster, corridor).
    /// </summary>
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private EnemySystem _enemies;
        [SerializeField] private WeaponDefinition _startingWeapon; // GDD: every run begins armed

        public const int MaxWeapons = 6; // GDD §4 slot cap

        private readonly List<WeaponInstance> _weapons = new(MaxWeapons);
        private WeaponContext _context;

        public IReadOnlyList<WeaponInstance> Weapons => _weapons;

        private void Start()
        {
            // Composition point for combat: assemble context once, hand to every behavior.
            _context = new WeaponContext
            {
                Player = _player,
                Enemies = _enemies,
                Pool = ServiceLocator.Get<IPoolService>(),
                Events = ServiceLocator.Get<IEventBus>(),
                DamageResolver = new DamageCalculator(_player.Stats)
            };

            if (_startingWeapon != null)
                TryAddWeapon(_startingWeapon);
        }

        private void Update()
        {
            if (_player.State == PlayerState.Dead) return;

            float attackSpeed = _player.Stats.Get(StatType.AttackSpeedPercent);
            float dt = Time.deltaTime; // scaled: weapons freeze during pause automatically

            for (int i = 0; i < _weapons.Count; i++)
                _weapons[i].TickCooldown(dt, attackSpeed);
        }

        /// <summary>Adds a weapon if a slot is free. Returns the instance (or null when full/duplicate).</summary>
        public WeaponInstance TryAddWeapon(WeaponDefinition definition)
        {
            if (_weapons.Count >= MaxWeapons) return null;
            if (Find(definition.Id) != null) return null; // duplicates level up instead (upgrade system's job)

            var instance = new WeaponInstance(definition, WeaponBehaviourFactory.Create(definition.Type), _context);
            _weapons.Add(instance);
            return instance;
        }

        public WeaponInstance Find(string weaponId)
        {
            for (int i = 0; i < _weapons.Count; i++)
                if (_weapons[i].Definition.Id == weaponId) return _weapons[i];
            return null;
        }

        public bool HasFreeSlot => _weapons.Count < MaxWeapons;

        /// <summary>Run teardown: release every behavior's pooled bodies before the pool clears.</summary>
        public void UnequipAll()
        {
            for (int i = 0; i < _weapons.Count; i++)
                _weapons[i].Behaviour.Unequip();
            _weapons.Clear();
        }
    }
}
