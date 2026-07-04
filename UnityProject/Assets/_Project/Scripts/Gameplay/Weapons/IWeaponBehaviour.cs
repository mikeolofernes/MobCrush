namespace MobCrush.Gameplay.Weapons
{
    /// <summary>
    /// Strategy for one weapon delivery type. One instance PER equipped weapon
    /// (stateful — orbitals own their orbiting bodies, boomerangs track flight).
    /// The controller (Loop 8) owns cooldown/attack-speed and calls Fire when ready;
    /// Tick runs every frame for continuous types (orbitals, zones).
    /// </summary>
    public interface IWeaponBehaviour
    {
        /// <summary>Bind context + owning instance. Called once when the weapon is acquired or evolves.</summary>
        void Equip(WeaponContext context, WeaponInstance instance);

        /// <summary>Discrete activation (cooldown elapsed). Continuous types may no-op.</summary>
        void Fire();

        /// <summary>Per-frame update for continuous effects. Discrete types may no-op.</summary>
        void Tick(float deltaTime);

        /// <summary>Teardown: release pooled bodies. Called on evolution swap / run end.</summary>
        void Unequip();
    }
}
