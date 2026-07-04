using MobCrush.Core.Events;
using MobCrush.Core.Input;
using MobCrush.Core.Services;
using MobCrush.Data;
using MobCrush.Gameplay.Combat;
using UnityEngine;

namespace MobCrush.Gameplay.Player
{
    /// <summary>
    /// Player root: builds the StatSheet from the definition SO, wires PlayerHealth,
    /// moves via Rigidbody2D, drives facing + animator params, and runs the small
    /// player state machine. This is the Game scene's player composition point, so it
    /// is the one class allowed to touch the ServiceLocator on the player.
    /// Mobile notes: physics movement in FixedUpdate, no allocations per frame,
    /// animator hashes cached.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerStatsDefinition _definition;
        [SerializeField] private Animator _animator;              // optional until art lands
        [SerializeField] private SpriteRenderer _spriteRenderer;  // flipped for facing

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int DeadParam = Animator.StringToHash("Dead");

        private Rigidbody2D _body;
        private PlayerHealth _health;
        private IInputService _input;
        private IEventBus _events;

        public StatSheet Stats { get; private set; }
        public PlayerState State { get; private set; } = PlayerState.Idle;
        public PlayerHealth Health => _health;
        /// <summary>Last non-zero move direction; weapons aim here when no target exists.</summary>
        public Vector2 Facing { get; private set; } = Vector2.right;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<PlayerHealth>();

            _input = ServiceLocator.Get<IInputService>();
            _events = ServiceLocator.Get<IEventBus>();

            Stats = BuildStatSheet(_definition);
            _health.Initialize(Stats, _events, _definition.InvincibilitySeconds);

            _events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDestroy() => _events.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);

        private void FixedUpdate()
        {
            if (State == PlayerState.Dead)
            {
                _body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 move = _input.Move;
            _body.linearVelocity = move * Stats.Get(StatType.MoveSpeed);

            State = move.sqrMagnitude > 0.0001f ? PlayerState.Moving : PlayerState.Idle;
            if (State == PlayerState.Moving)
            {
                Facing = move.normalized;
                if (_spriteRenderer != null)
                    _spriteRenderer.flipX = Facing.x < 0f; // sprite flip beats scale flip: no physics/child side effects
            }

            if (_animator != null)
                _animator.SetFloat(SpeedParam, move.magnitude);
        }

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            State = PlayerState.Dead;
            if (_animator != null) _animator.SetBool(DeadParam, true);
        }

        /// <summary>Revive entry point used by the run flow (ad/gem revive).</summary>
        public void Revive(float hpFraction)
        {
            _health.ReviveWithHpFraction(hpFraction);
            State = PlayerState.Idle;
            if (_animator != null) _animator.SetBool(DeadParam, false);
        }

        private static StatSheet BuildStatSheet(PlayerStatsDefinition def)
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.MaxHp, def.MaxHp);
            sheet.SetBase(StatType.HpRegenPerSecond, def.HpRegenPerSecond);
            sheet.SetBase(StatType.MoveSpeed, def.MoveSpeed);
            sheet.SetBase(StatType.Armor, def.Armor);
            sheet.SetBase(StatType.CritChance, def.CritChance);
            sheet.SetBase(StatType.CritDamage, def.CritDamage);
            sheet.SetBase(StatType.PickupRadius, def.PickupRadius);
            // Percent-type stats default to base 0; DamagePercent etc. arrive from gear/upgrades.
            return sheet;
        }
    }
}
