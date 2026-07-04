using MobCrush.Core.Events;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using MobCrush.Gameplay.Enemies;
using UnityEngine;

namespace MobCrush.Gameplay.Bosses
{
    /// <summary>
    /// Runtime boss (Loop 13): runs BossDefinition phases as a small interpreter —
    /// pick phase by HP fraction, loop its pattern steps (telegraph → execute), apply
    /// soft enrage per phase time. All patterns are built from the same primitives
    /// enemies use (EnemyProjectile, EnemySystem summons), so bosses are data remixes,
    /// not bespoke code. IDamageable: player weapons need no boss special-casing.
    /// </summary>
    public sealed class BossController : MonoBehaviour, ITargetable
    {
        [SerializeField] private SpriteRenderer _renderer;

        private BossDefinition _def;
        private EnemySystem _enemies;
        private Transform _player;
        private IDamageable _playerDamageable;
        private IEventBus _events;
        private Core.Pooling.IPoolService _pool;

        private float _hp;
        private int _phaseIndex = -1;
        private int _stepIndex;
        private float _stepTimer;
        private float _phaseTime;      // for soft enrage
        private bool _telegraphing;

        // Charge state
        private Vector2 _chargeDirection;
        private bool _chargeHitDone;

        private float _contactTimer;
        private const float ContactCooldown = 0.8f;
        private const float ContactRange = 1.2f;
        private const float ChargeSpeedMultiplier = 6f;

        public bool IsAlive => _hp > 0f;
        public Vector2 Position => transform.position; // ITargetable: weapons treat the boss like any target
        public float CurrentHp => _hp;
        public float MaxHp => _def != null ? _def.MaxHp : 1f;

        public void Initialize(BossDefinition definition, EnemySystem enemies, Transform player,
                               IDamageable playerDamageable, IEventBus events, Core.Pooling.IPoolService pool)
        {
            _pool = pool;
            _def = definition;
            _enemies = enemies;
            _player = player;
            _playerDamageable = playerDamageable;
            _events = events;
            _hp = definition.MaxHp;
            EnterPhaseForHp();
        }

        private void Update()
        {
            if (!IsAlive || _def == null) return;

            float dt = Time.deltaTime;
            _phaseTime += dt;
            _contactTimer -= dt;

            var step = CurrentStep;
            if (step == null) { ChasePlayer(dt); return; }

            _stepTimer -= dt;

            if (_telegraphing)
            {
                if (_stepTimer <= 0f)
                {
                    _telegraphing = false;
                    if (_renderer != null) _renderer.color = Color.white;
                    ExecuteStep(step);
                    _stepTimer = step.Duration;
                }
                return; // frozen while telegraphing = the player's read window
            }

            // Steps with continuous motion:
            if (step.Pattern == BossPattern.Charge) TickCharge(step, dt);
            else ChasePlayer(dt);

            TickContactDamage();

            if (_stepTimer <= 0f) AdvanceStep();
        }

        // --- Phase / step machinery ---

        private BossDefinition.PatternStep CurrentStep =>
            _phaseIndex >= 0 && _phaseIndex < _def.Phases.Count && _def.Phases[_phaseIndex].Steps.Count > 0
                ? _def.Phases[_phaseIndex].Steps[_stepIndex]
                : null;

        private void EnterPhaseForHp()
        {
            float fraction = _hp / _def.MaxHp;
            int target = _phaseIndex;
            for (int i = 0; i < _def.Phases.Count; i++)
                if (fraction <= _def.Phases[i].HpThreshold) target = i;

            if (target != _phaseIndex)
            {
                _phaseIndex = target;
                _stepIndex = 0;
                _phaseTime = 0f;
                BeginStep();
            }
        }

        private void AdvanceStep()
        {
            var steps = _def.Phases[_phaseIndex].Steps;
            _stepIndex = (_stepIndex + 1) % steps.Count; // pattern list loops within a phase
            BeginStep();
        }

        private void BeginStep()
        {
            var step = CurrentStep;
            if (step == null) return;

            if (step.Telegraph > 0f && step.Pattern != BossPattern.Idle)
            {
                _telegraphing = true;
                _stepTimer = step.Telegraph;
                if (_renderer != null) _renderer.color = new Color(1f, 0.4f, 0.4f); // Loop 19 replaces with real VFX
                if (step.Pattern == BossPattern.Charge)
                {
                    // Lock direction at telegraph start: dodgeable by moving after the read.
                    _chargeDirection = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                    _chargeHitDone = false;
                }
            }
            else
            {
                _telegraphing = false;
                ExecuteStep(step);
                _stepTimer = step.Duration;
            }
        }

        private void ExecuteStep(BossDefinition.PatternStep step)
        {
            float enrage = 1f + _def.Phases[_phaseIndex].EnrageDamagePer10s * (_phaseTime / 10f);
            float damage = step.Damage * enrage;

            switch (step.Pattern)
            {
                case BossPattern.AimedVolley:
                {
                    Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                    // Narrow fan at the player.
                    for (int i = 0; i < step.ProjectileCount; i++)
                    {
                        float spread = (i - (step.ProjectileCount - 1) * 0.5f) * 8f * Mathf.Deg2Rad;
                        FireShot(Rotate(dir, spread), step.ProjectileSpeed, damage);
                    }
                    break;
                }
                case BossPattern.RadialBurst:
                {
                    float stepAngle = Mathf.PI * 2f / step.ProjectileCount;
                    for (int i = 0; i < step.ProjectileCount; i++)
                        FireShot(new Vector2(Mathf.Cos(stepAngle * i), Mathf.Sin(stepAngle * i)),
                                 step.ProjectileSpeed, damage);
                    break;
                }
                case BossPattern.SummonMinions:
                {
                    if (step.SummonEnemy == null) break;
                    for (int i = 0; i < step.SummonCount; i++)
                    {
                        float angle = Mathf.PI * 2f * i / step.SummonCount;
                        Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 2f;
                        _enemies.Spawn(step.SummonEnemy, pos);
                    }
                    break;
                }
                // Charge damage happens during motion (TickCharge); Idle is the recovery window.
            }
        }

        private void TickCharge(BossDefinition.PatternStep step, float dt)
        {
            transform.position += (Vector3)(_chargeDirection * (_def.MoveSpeed * ChargeSpeedMultiplier * dt));

            if (!_chargeHitDone &&
                ((Vector2)_player.position - (Vector2)transform.position).sqrMagnitude < ContactRange * ContactRange)
            {
                _chargeHitDone = true; // one hit per charge — a clipped charge must not shred the whole HP bar
                _playerDamageable.TakeDamage(new DamageInfo(step.Damage, false, transform.position));
            }
        }

        private void ChasePlayer(float dt)
        {
            Vector2 pos = transform.position;
            Vector2 dir = (Vector2)_player.position - pos;
            if (dir.sqrMagnitude < 0.01f) return;
            transform.position = pos + dir.normalized * (_def.MoveSpeed * dt);
            if (_renderer != null) _renderer.flipX = dir.x < 0f;
        }

        private void TickContactDamage()
        {
            if (_contactTimer > 0f) return;
            if (((Vector2)_player.position - (Vector2)transform.position).sqrMagnitude < ContactRange * ContactRange)
            {
                _playerDamageable.TakeDamage(new DamageInfo(_def.ContactDamage, false, transform.position));
                _contactTimer = ContactCooldown;
            }
        }

        private void FireShot(Vector2 dir, float speed, float damage)
        {
            // Pool injected at Initialize — gameplay code must not touch the locator (Loop 3 §4).
            if (_def.ProjectilePrefab == null) return;
            var go = _pool.Get(_def.ProjectilePrefab, transform.position, Quaternion.identity);
            go.GetComponent<EnemyProjectile>().Launch(dir, speed, damage, _playerDamageable, _pool);
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float c = Mathf.Cos(radians), s = Mathf.Sin(radians);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        // --- Damage intake ---
        public void TakeDamage(in DamageInfo damage)
        {
            if (!IsAlive) return;
            _hp -= damage.Amount;
            _events.Publish(new DamageDealtEvent(damage.Amount, damage.Critical, transform.position));

            if (_hp <= 0f)
            {
                _hp = 0f;
                _events.Publish(new BossDefeatedEvent(_def.Id));
                gameObject.SetActive(false); // run flow owns victory sequence + rewards (RunEndedEvent)
            }
            else
            {
                EnterPhaseForHp(); // phase transitions ride the damage path — no per-frame threshold polling
            }
        }
    }
}
