namespace MobCrush.Gameplay.Combat
{
    /// <summary>
    /// Every numeric knob that gameplay math can read. One shared vocabulary used by
    /// player base stats, equipment, talents and in-run upgrades — so any source can
    /// modify any stat without new code paths (GDD §1).
    /// </summary>
    public enum StatType
    {
        MaxHp,
        HpRegenPerSecond,
        MoveSpeed,
        DamagePercent,       // additive % on all damage
        AttackSpeedPercent,  // additive % on weapon fire rate
        CritChance,          // 0..1
        CritDamage,          // multiplier, base 1.5
        Armor,               // flat reduction per hit
        PickupRadius,
        AreaPercent,         // AoE size bonus
        DurationPercent,     // effect duration bonus
        ProjectileCountBonus,// flat extra projectiles
        XpGainPercent,
        CoinGainPercent,
        Luck                 // shifts draft rarity weights
    }
}
