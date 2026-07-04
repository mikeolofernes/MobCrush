using MobCrush.Core.Events;
using MobCrush.Gameplay.Combat;
using UnityEngine;
using MobCrush.Data;

namespace MobCrush.Gameplay.Player
{
    /// <summary>
    /// Player HP, regen, i-frames and death. Separated from movement (SRP): health rules
    /// change independently of locomotion. Publishes events; owns no UI or flow decisions.
    /// </summary>
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        private StatSheet _stats;
        private IEventBus _events;
        private float _currentHp;
        private float _invincibleUntil; // global i-frame window (per-source refinement is a Loop 21 candidate if needed)
        private float _invincibilitySeconds;

        public bool IsAlive => _currentHp > 0f;
        public float CurrentHp => _currentHp;
        public float MaxHp => _stats.Get(StatType.MaxHp);

        /// <summary>Called by PlayerController at run start (constructor-style init — no locator here).</summary>
        public void Initialize(StatSheet stats, IEventBus events, float invincibilitySeconds)
        {
            _stats = stats;
            _events = events;
            _invincibilitySeconds = invincibilitySeconds;
            _currentHp = MaxHp;
        }

        private void Update()
        {
            if (!IsAlive || _stats == null) return;

            float regen = _stats.Get(StatType.HpRegenPerSecond);
            if (regen > 0f && _currentHp < MaxHp)
            {
                // Silent regen: no heal event spam every frame; UI reads CurrentHp each frame anyway.
                _currentHp = Mathf.Min(_currentHp + regen * Time.deltaTime, MaxHp);
            }
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (!IsAlive) return;
            if (Time.time < _invincibleUntil) return;

            // Armor is flat reduction with a floor: every hit does SOMETHING (GDD §1).
            float reduced = Mathf.Max(1f, damage.Amount - _stats.Get(StatType.Armor));
            _currentHp -= reduced;
            _invincibleUntil = Time.time + _invincibilitySeconds;

            _events.Publish(new PlayerDamagedEvent(reduced, _currentHp, MaxHp));

            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                _events.Publish(new PlayerDiedEvent());
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            _currentHp = Mathf.Min(_currentHp + amount, MaxHp);
            _events.Publish(new PlayerHealedEvent(amount, _currentHp, MaxHp));
        }

        /// <summary>
        /// Top up to the CURRENT MaxHp. Called by the run installer after meta bonuses
        /// (gear/talents) raise MaxHp — Initialize ran in Awake, before bonuses applied,
        /// so without this the player would start a run missing the bonus HP.
        /// </summary>
        public void RefillToFull() => _currentHp = MaxHp;

        /// <summary>Revive support (rewarded ad / gem revive, GDD §1).</summary>
        public void ReviveWithHpFraction(float fraction)
        {
            _currentHp = Mathf.Clamp01(fraction) * MaxHp;
            _invincibleUntil = Time.time + 2f * _invincibilitySeconds; // grace window on revive
        }
    }
}
