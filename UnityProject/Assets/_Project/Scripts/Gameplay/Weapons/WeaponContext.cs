using System.Collections.Generic;
using MobCrush.Core.Events;
using MobCrush.Core.Pooling;
using MobCrush.Gameplay.Combat;
using MobCrush.Gameplay.Enemies;
using MobCrush.Gameplay.Player;
using UnityEngine;

namespace MobCrush.Gameplay.Weapons
{
    /// <summary>
    /// Resolves a weapon's base damage into a final hit (crit roll + modifiers).
    /// Interface so behaviors never know the formula; implemented in Loop 8.
    /// </summary>
    public interface IDamageResolver
    {
        DamageInfo Resolve(float weaponBaseDamage, Vector2 sourcePosition);
    }

    /// <summary>
    /// Everything a weapon behavior may touch, injected once at equip time.
    /// why: behaviors get capabilities, not the world — no locator calls, no scene
    /// searches, fully fakeable in tests. One shared query buffer (behaviors run
    /// sequentially on the main thread) keeps radius queries allocation-free.
    /// </summary>
    public sealed class WeaponContext
    {
        public PlayerController Player;
        public EnemySystem Enemies;
        public IPoolService Pool;
        public IEventBus Events;
        public IDamageResolver DamageResolver;

        /// <summary>Shared scratch list for radius queries. Valid only within one behavior call.</summary>
        public readonly List<EnemyController> QueryBuffer = new(64);

        public Vector2 PlayerPosition => Player.transform.position;
        public Vector2 PlayerFacing => Player.Facing;

        /// <summary>Convenience: resolve + apply + knockback-free hit on one enemy.</summary>
        public void Hit(EnemyController enemy, float baseDamage, Vector2 from)
        {
            var info = DamageResolver.Resolve(baseDamage, from);
            enemy.TakeDamage(info);
        }
    }
}
