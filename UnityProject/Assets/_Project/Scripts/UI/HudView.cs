using MobCrush.Core.Events;
using MobCrush.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// In-run HUD (Loop 17): HP bar, XP bar + level, stage timer, kill counter.
    /// Pure read-side: subscribes to events, never mutates gameplay (Loop 3 §3 rule).
    /// Bars use fillAmount (no layout rebuilds per frame — mobile UI rule).
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Image _healthFill;

        [Header("Experience")]
        [SerializeField] private Image _xpFill;
        [SerializeField] private TMP_Text _levelText;

        [Header("Run info")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _killsText;

        private IEventBus _events;
        private int _kills;
        private float _elapsed;
        private int _lastShownSecond = -1;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            _events.Subscribe<ExperienceGainedEvent>(OnXpGained);
            _events.Subscribe<LevelUpEvent>(OnLevelUp);
            _events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            _events.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Unsubscribe<PlayerHealedEvent>(OnPlayerHealed);
            _events.Unsubscribe<ExperienceGainedEvent>(OnXpGained);
            _events.Unsubscribe<LevelUpEvent>(OnLevelUp);
            _events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            int second = (int)_elapsed;
            if (second != _lastShownSecond) // string alloc once per second, not per frame
            {
                _lastShownSecond = second;
                _timerText.SetText("{0:00}:{1:00}", second / 60, second % 60); // TMP SetText: no GC string
            }
        }

        private void OnPlayerDamaged(PlayerDamagedEvent e) => _healthFill.fillAmount = e.CurrentHp / e.MaxHp;
        private void OnPlayerHealed(PlayerHealedEvent e) => _healthFill.fillAmount = e.CurrentHp / e.MaxHp;

        private void OnXpGained(ExperienceGainedEvent e) =>
            _xpFill.fillAmount = e.RequiredXp <= 0f ? 0f : Mathf.Clamp01(e.CurrentXp / e.RequiredXp);

        private void OnLevelUp(LevelUpEvent e)
        {
            _levelText.SetText("Lv {0}", e.NewLevel);
            _xpFill.fillAmount = 0f;
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            _kills++;
            _killsText.SetText("{0}", _kills);
        }
    }
}
