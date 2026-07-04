using UnityEngine;

namespace MobCrush.Gameplay.Combat
{
    /// <summary>Immutable description of one hit. Struct: created constantly on the hot path.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly bool Critical;
        public readonly Vector2 SourcePosition; // for knockback direction / damage-number placement

        public DamageInfo(float amount, bool critical, Vector2 sourcePosition)
        {
            Amount = amount;
            Critical = critical;
            SourcePosition = sourcePosition;
        }
    }
}
