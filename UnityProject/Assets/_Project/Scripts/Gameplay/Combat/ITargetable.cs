using UnityEngine;

namespace MobCrush.Gameplay.Combat
{
    /// <summary>
    /// Anything player weapons can aim at and hit: regular enemies AND bosses.
    /// why: weapons query positions + deal damage — they must not care whether the
    /// target is a pooled swarm enemy or a unique boss (Loop 13 requirement).
    /// </summary>
    public interface ITargetable : IDamageable
    {
        Vector2 Position { get; }
    }
}
