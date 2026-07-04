using MobCrush.Data;

namespace MobCrush.Gameplay.Upgrades
{
    public enum DraftOptionKind
    {
        NewWeapon,
        WeaponLevel,
        Evolution,
        NewPassive,
        PassiveLevel
    }

    /// <summary>
    /// One card in the draft UI. Plain data: the UI renders it, the drafter applies it.
    /// SynergyHint marks options that progress an evolution pair (USP: synergy-first drafting).
    /// </summary>
    public readonly struct DraftOption
    {
        public readonly DraftOptionKind Kind;
        public readonly WeaponDefinition Weapon;   // set for weapon kinds
        public readonly UpgradeDefinition Passive; // set for passive kinds
        public readonly Rarity Rarity;
        public readonly bool SynergyHint;

        public DraftOption(DraftOptionKind kind, WeaponDefinition weapon, UpgradeDefinition passive,
                           Rarity rarity, bool synergyHint)
        {
            Kind = kind;
            Weapon = weapon;
            Passive = passive;
            Rarity = rarity;
            SynergyHint = synergyHint;
        }

        public string Title => Kind switch
        {
            DraftOptionKind.Evolution => $"EVOLVE: {Weapon.EvolvedForm.DisplayName}",
            DraftOptionKind.NewWeapon => Weapon.DisplayName,
            DraftOptionKind.WeaponLevel => $"{Weapon.DisplayName} +",
            _ => Passive.DisplayName
        };
    }
}
