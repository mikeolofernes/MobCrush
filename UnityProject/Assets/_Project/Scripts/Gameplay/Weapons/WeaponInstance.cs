using MobCrush.Data;
using MobCrush.Gameplay.Combat;

namespace MobCrush.Gameplay.Weapons
{
    /// <summary>
    /// One equipped weapon at runtime: definition + level + behavior + cooldown.
    /// Plain C# (definition stays immutable; run state lives here — Loop 3 §5 rule).
    /// </summary>
    public sealed class WeaponInstance
    {
        public WeaponDefinition Definition { get; private set; }
        public int Level { get; private set; } = 1;
        public IWeaponBehaviour Behaviour { get; }

        private readonly WeaponContext _context;
        private float _cooldownRemaining;

        public WeaponDefinition.LevelStats Stats => Definition.GetLevel(Level);
        public bool IsMaxLevel => Level >= Definition.MaxLevel;
        public bool CanEvolve => IsMaxLevel && Definition.EvolvedForm != null;

        public WeaponInstance(WeaponDefinition definition, IWeaponBehaviour behaviour, WeaponContext context)
        {
            Definition = definition;
            Behaviour = behaviour;
            _context = context;
            behaviour.Equip(context, this);
        }

        public void LevelUp()
        {
            if (!IsMaxLevel) Level++;
        }

        /// <summary>Evolution swap: same instance, new definition at level 1-of-evolved (GDD §4). Behavior re-equips to pick up new stats/prefabs.</summary>
        public void Evolve()
        {
            if (!CanEvolve) return;
            Behaviour.Unequip();
            Definition = Definition.EvolvedForm;
            Level = 1;
            Behaviour.Equip(_context, this);
        }

        /// <summary>Controller-driven cadence: attack speed compresses the data cooldown (Loop 8 formula).</summary>
        public void TickCooldown(float deltaTime, float attackSpeedPercent)
        {
            Behaviour.Tick(deltaTime);

            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining > 0f) return;

            // why divide: +100% attack speed halves the interval, matching player intuition.
            _cooldownRemaining = Stats.Cooldown / (1f + attackSpeedPercent);
            Behaviour.Fire();
        }
    }
}
