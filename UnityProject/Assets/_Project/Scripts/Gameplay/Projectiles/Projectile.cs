using System.Collections.Generic;
using MobCrush.Gameplay.Enemies;
using MobCrush.Gameplay.Weapons;
using MobCrush.Core.Pooling;
using UnityEngine;

namespace MobCrush.Gameplay.Projectiles
{
    /// <summary>
    /// The reusable player projectile (Loop 9). One component supports straight/boomerang/
    /// homing motion plus pierce, bounce, split and explosion — all from ProjectileConfig,
    /// so every future upgrade ("+1 bounce", "shots explode") is a config tweak, not code.
    /// Hit detection is distance-based against EnemySystem (no physics): deterministic,
    /// collider-free enemies stay cheap, and hits can't tunnel at our speeds because
    /// hit radius exceeds per-frame travel at target frame rates.
    /// </summary>
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        // Shared scratch: projectile Updates run sequentially on the main thread.
        private static readonly List<EnemyController> Query = new(32);

        private WeaponContext _ctx;
        private ProjectileConfig _config;
        private Vector2 _velocity;
        private Vector2 _launchOrigin;
        private float _age;
        private bool _returning;      // boomerang phase
        private int _pierceLeft;
        private int _bounceLeft;
        private bool _hasSplit;
        private bool _live;

        // Enemies already damaged (pierce/boomerang must not re-hit); small list, cleared per spawn.
        private readonly List<EnemyController> _alreadyHit = new(8);

        public void Launch(WeaponContext ctx, in ProjectileConfig config, Vector2 position, Vector2 direction)
        {
            _ctx = ctx;
            _config = config;
            transform.position = position;
            _launchOrigin = position;
            _velocity = direction.normalized * config.Speed;
            _age = 0f;
            _returning = false;
            _pierceLeft = config.Pierce;
            _bounceLeft = config.Bounce;
            _hasSplit = false;
            _alreadyHit.Clear();
            _live = true;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (!_live) return;

            float dt = Time.deltaTime;
            _age += dt;
            if (_age >= _config.Lifetime) { Despawn(exploded: false); return; }

            Steer(dt);
            transform.position += (Vector3)(_velocity * dt);
            CheckHits();
        }

        private void Steer(float dt)
        {
            switch (_config.Motion)
            {
                case ProjectileMotion.Boomerang:
                {
                    Vector2 pos = transform.position;
                    if (!_returning && (pos - _launchOrigin).sqrMagnitude >= _config.Range * _config.Range)
                    {
                        _returning = true;
                        _alreadyHit.Clear(); // the return pass may hit the same enemies again — that's the weapon's identity
                    }
                    if (_returning)
                    {
                        Vector2 toPlayer = _ctx.PlayerPosition - pos;
                        if (toPlayer.sqrMagnitude < 0.36f) { Despawn(exploded: false); return; } // caught
                        _velocity = toPlayer.normalized * _config.Speed;
                    }
                    break;
                }
                case ProjectileMotion.Homing:
                {
                    var target = _ctx.Enemies.FindNearest(transform.position, maxRange: 10f);
                    if (target != null)
                    {
                        Vector2 desired = (target.Position - (Vector2)transform.position).normalized * _config.Speed;
                        float maxRadians = _config.HomingTurnSpeed * Mathf.Deg2Rad * dt;
                        _velocity = Vector3.RotateTowards(_velocity, desired, maxRadians, float.MaxValue);
                    }
                    break;
                }
            }
        }

        private void CheckHits()
        {
            Vector2 pos = transform.position;
            int count = _ctx.Enemies.QueryRadius(pos, _config.HitRadius, Query);
            for (int i = 0; i < count; i++)
            {
                var enemy = Query[i];
                if (_alreadyHit.Contains(enemy)) continue;

                _alreadyHit.Add(enemy);
                _ctx.Hit(enemy, _config.Damage, pos);

                if (!_hasSplit && _config.SplitCount > 0) Split(pos);

                if (_bounceLeft > 0)
                {
                    _bounceLeft--;
                    RedirectToNextTarget(enemy, pos);
                    return; // a bounce consumes this frame's hit processing
                }

                if (_pierceLeft > 0) { _pierceLeft--; continue; }

                Despawn(exploded: true);
                return;
            }
        }

        private void RedirectToNextTarget(EnemyController exclude, Vector2 from)
        {
            // Nearest enemy we haven't already damaged; fallback: keep flying straight.
            EnemyController best = null;
            float bestSqr = 12f * 12f;
            int count = _ctx.Enemies.QueryRadius(from, 12f, Query);
            for (int i = 0; i < count; i++)
            {
                var e = Query[i];
                if (e == exclude || _alreadyHit.Contains(e)) continue;
                float sqr = (e.Position - from).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = e; }
            }
            if (best != null)
                _velocity = (best.Position - from).normalized * _config.Speed;
        }

        private void Split(Vector2 at)
        {
            _hasSplit = true;
            var childConfig = _config;
            childConfig.SplitCount = 0;               // children never re-split (no exponential storms)
            childConfig.Damage = _config.Damage * 0.5f; // halved child damage: split is width, not free DPS
            childConfig.Lifetime = 1.5f;

            float step = 360f / _config.SplitCount;
            for (int i = 0; i < _config.SplitCount; i++)
            {
                float rad = step * i * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                var go = _ctx.Pool.Get(gameObject.GetPrefabSource() ?? gameObject, at, Quaternion.identity);
                go.GetComponent<Projectile>().Launch(_ctx, childConfig, at, dir);
            }
        }

        private void Despawn(bool exploded)
        {
            if (!_live) return;
            _live = false;

            if (exploded && _config.ExplosionRadius > 0f)
            {
                Vector2 pos = transform.position;
                int count = _ctx.Enemies.QueryRadius(pos, _config.ExplosionRadius, Query);
                for (int i = 0; i < count; i++)
                    _ctx.Hit(Query[i], _config.ExplosionDamage, pos);
            }

            _ctx.Pool.Release(gameObject);
        }

        public void OnSpawned() { }
        public void OnDespawned() { _live = false; _alreadyHit.Clear(); }
    }

    /// <summary>
    /// Split needs "which prefab am I" to pool-spawn children. The pool keys by prefab, so we
    /// record it on first launch via this helper component-free registry.
    /// </summary>
    internal static class PrefabSourceExtension
    {
        private static readonly Dictionary<GameObject, GameObject> Sources = new();

        public static void RegisterPrefabSource(this GameObject instance, GameObject prefab) => Sources[instance] = prefab;
        public static GameObject GetPrefabSource(this GameObject instance) =>
            Sources.TryGetValue(instance, out var p) ? p : null;
    }
}
