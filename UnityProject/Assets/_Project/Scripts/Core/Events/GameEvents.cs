using UnityEngine;

namespace MobCrush.Core.Events
{
    // Canonical cross-system event payloads (Loop 3 §7). They live in Core because they are
    // plain data contracts shared by Gameplay, Meta and UI — Core still references no gameplay code.
    // All structs: publishing allocates nothing.

    public readonly struct PlayerDamagedEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly float CurrentHp;
        public readonly float MaxHp;
        public PlayerDamagedEvent(float amount, float currentHp, float maxHp)
        { Amount = amount; CurrentHp = currentHp; MaxHp = maxHp; }
    }

    public readonly struct PlayerDiedEvent : IGameEvent { }

    public readonly struct PlayerHealedEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly float CurrentHp;
        public readonly float MaxHp;
        public PlayerHealedEvent(float amount, float currentHp, float maxHp)
        { Amount = amount; CurrentHp = currentHp; MaxHp = maxHp; }
    }

    public readonly struct EnemyKilledEvent : IGameEvent
    {
        public readonly string EnemyId;
        public readonly Vector3 Position;
        public readonly bool WasElite;
        public readonly float XpValue; // carried in the event so the XP system needs no enemy-definition lookup
        public EnemyKilledEvent(string enemyId, Vector3 position, bool wasElite, float xpValue)
        { EnemyId = enemyId; Position = position; WasElite = wasElite; XpValue = xpValue; }
    }

    public readonly struct ExperienceGainedEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly float CurrentXp;
        public readonly float RequiredXp;
        public ExperienceGainedEvent(float amount, float currentXp, float requiredXp)
        { Amount = amount; CurrentXp = currentXp; RequiredXp = requiredXp; }
    }

    public readonly struct LevelUpEvent : IGameEvent
    {
        public readonly int NewLevel;
        public LevelUpEvent(int newLevel) { NewLevel = newLevel; }
    }

    public readonly struct UpgradeChosenEvent : IGameEvent
    {
        public readonly string UpgradeId;
        public UpgradeChosenEvent(string upgradeId) { UpgradeId = upgradeId; }
    }

    public readonly struct WeaponEvolvedEvent : IGameEvent
    {
        public readonly string WeaponId; // the evolved (new) form's id
        public WeaponEvolvedEvent(string weaponId) { WeaponId = weaponId; }
    }

    public readonly struct WaveStartedEvent : IGameEvent
    {
        public readonly int WaveIndex;
        public WaveStartedEvent(int waveIndex) { WaveIndex = waveIndex; }
    }

    public readonly struct BossSpawnedEvent : IGameEvent
    {
        public readonly string BossId;
        public BossSpawnedEvent(string bossId) { BossId = bossId; }
    }

    public readonly struct BossDefeatedEvent : IGameEvent
    {
        public readonly string BossId;
        public BossDefeatedEvent(string bossId) { BossId = bossId; }
    }

    public readonly struct RunEndedEvent : IGameEvent
    {
        public readonly bool Victory;
        public readonly float DurationSeconds;
        public readonly int Kills;
        public readonly int LevelReached;
        public RunEndedEvent(bool victory, float durationSeconds, int kills, int levelReached)
        { Victory = victory; DurationSeconds = durationSeconds; Kills = kills; LevelReached = levelReached; }
    }

    public readonly struct CurrencyChangedEvent : IGameEvent
    {
        public readonly string CurrencyId; // "coins" | "gems" | "cores"
        public readonly long NewAmount;
        public CurrencyChangedEvent(string currencyId, long newAmount)
        { CurrencyId = currencyId; NewAmount = newAmount; }
    }

    public readonly struct EquipmentChangedEvent : IGameEvent { }

    public readonly struct DamageDealtEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly bool Critical;
        public readonly Vector3 Position;
        public DamageDealtEvent(float amount, bool critical, Vector3 position)
        { Amount = amount; Critical = critical; Position = position; }
    }
}
