using MobCrush.Core.Events;
using MobCrush.Core.Services;
using MobCrush.Gameplay.Run;
using MobCrush.LiveOps;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// The death revive prompt (GDD §1: one revive per run via rewarded ad).
    /// Flow: PlayerDiedEvent → show if RunController's window is open → "Revive" runs the
    /// rewarded ad and revives ONLY on a completed watch → "Give up" (or ad failure)
    /// calls DeclineRevive so the defeat lands immediately instead of idling out the window.
    /// </summary>
    public sealed class ReviveView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private Button _declineButton;
        [SerializeField] private RunController _run;
        [SerializeField] private string _adPlacementId = "revive";
        [Range(0f, 1f)] [SerializeField] private float _reviveHpFraction = 0.5f;

        private IEventBus _events;
        private IAdsService _ads;
        private bool _adInFlight;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _ads = ServiceLocator.Get<IAdsService>();

            _reviveButton.onClick.AddListener(OnReviveClicked);
            _declineButton.onClick.AddListener(OnDeclineClicked);
            _events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            _panel.SetActive(false);
        }

        private void OnDestroy() => _events.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            // No offer when the revive is already spent (second death) or ads can't serve —
            // showing a dead button is worse than no prompt.
            if (!_run.IsReviveWindowOpen || !_ads.IsRewardedReady)
            {
                _run.DeclineRevive();
                return;
            }
            _panel.SetActive(true);
        }

        private void OnReviveClicked()
        {
            if (_adInFlight) return;
            _adInFlight = true;
            _reviveButton.interactable = false;

            _ads.ShowRewarded(_adPlacementId, watched =>
            {
                _adInFlight = false;
                _reviveButton.interactable = true;
                _panel.SetActive(false);

                // Reward gate: only a COMPLETED watch revives; anything else lands the defeat.
                if (!watched || !_run.TryRevive(_reviveHpFraction))
                    _run.DeclineRevive();
            });
        }

        private void OnDeclineClicked()
        {
            if (_adInFlight) return;
            _panel.SetActive(false);
            _run.DeclineRevive();
        }
    }
}
