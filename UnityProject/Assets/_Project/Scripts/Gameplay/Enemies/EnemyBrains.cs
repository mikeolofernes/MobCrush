using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Enemies
{
    // The four archetype brains (GDD §2). One file: they are small, closed strategies;
    // new archetypes get their own file when they outgrow a screenful.

    /// <summary>Walks straight at the player; contact damage handled by EnemyController.</summary>
    public sealed class ChaserBrain : IEnemyBrain
    {
        public static readonly ChaserBrain Instance = new();
        public void Tick(EnemyController e, float dt)
        {
            e.MoveTowards(e.PlayerPosition, e.Definition.MoveSpeed, dt);
        }
    }

    /// <summary>Approaches, then lunges at multiplied speed when inside LungeRange.</summary>
    public sealed class SprinterBrain : IEnemyBrain
    {
        public static readonly SprinterBrain Instance = new();
        public void Tick(EnemyController e, float dt)
        {
            bool lunging = e.TimerA > 0f;
            if (lunging)
            {
                e.TimerA -= dt;
                e.MoveTowards(e.PlayerPosition, e.Definition.MoveSpeed * e.Definition.LungeSpeedMultiplier, dt);
                return;
            }

            e.TimerB -= dt; // lunge cooldown
            e.MoveTowards(e.PlayerPosition, e.Definition.MoveSpeed, dt);

            if (e.TimerB <= 0f && e.DistanceToPlayerSqr < e.Definition.LungeRange * e.Definition.LungeRange)
            {
                e.TimerA = 0.4f;                      // lunge duration
                e.TimerB = e.Definition.LungeCooldown;
            }
        }
    }

    /// <summary>Keeps PreferredRange, fires projectiles on cooldown (GDD "Spitter").</summary>
    public sealed class RangedBrain : IEnemyBrain
    {
        public static readonly RangedBrain Instance = new();
        public void Tick(EnemyController e, float dt)
        {
            float preferredSqr = e.Definition.PreferredRange * e.Definition.PreferredRange;

            // Hold the band: advance if too far, retreat if crowded (0.6x inner ring).
            if (e.DistanceToPlayerSqr > preferredSqr)
                e.MoveTowards(e.PlayerPosition, e.Definition.MoveSpeed, dt);
            else if (e.DistanceToPlayerSqr < preferredSqr * 0.36f)
                e.MoveAway(e.PlayerPosition, e.Definition.MoveSpeed * 0.7f, dt);

            e.TimerA -= dt;
            if (e.TimerA <= 0f)
            {
                e.TimerA = e.Definition.FireCooldown;
                e.FireProjectileAtPlayer();
            }
        }
    }

    /// <summary>Runs in; inside DetonateRange starts a telegraph, then explodes (AoE), killing itself.</summary>
    public sealed class ExploderBrain : IEnemyBrain
    {
        public static readonly ExploderBrain Instance = new();
        public void Tick(EnemyController e, float dt)
        {
            bool telegraphing = e.FlagA;
            if (telegraphing)
            {
                e.TimerA -= dt;
                if (e.TimerA <= 0f)
                {
                    // Detonate: AoE against the player only (enemies don't friendly-fire, GDD §2).
                    if (e.DistanceToPlayerSqr <= e.Definition.ExplosionRadius * e.Definition.ExplosionRadius)
                        e.DamagePlayer(new DamageInfo(e.Definition.ExplosionDamage, false, e.Position));
                    e.Explode();
                }
                return; // frozen while telegraphing — the player's dodge window
            }

            e.MoveTowards(e.PlayerPosition, e.Definition.MoveSpeed, dt);
            if (e.DistanceToPlayerSqr < e.Definition.DetonateRange * e.Definition.DetonateRange)
            {
                e.FlagA = true;
                e.TimerA = e.Definition.TelegraphSeconds;
                e.OnTelegraphStarted();
            }
        }
    }
}
