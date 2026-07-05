using MobCrush.Core.Events;
using MobCrush.Core.Services;
using MobCrush.Meta.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// Daily missions panel (production-readiness fix: the service had no UI).
    /// Five fixed rows (catalog size is fixed per day), claim buttons, refresh on
    /// currency events (claims change coins/gems → one refresh path covers all).
    /// Injected by MenuInstaller like InventoryView.
    /// </summary>
    public sealed class MissionsView : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Row
        {
            public GameObject Root;
            public TMP_Text Description;
            public TMP_Text Progress;
            public Button ClaimButton;
        }

        [SerializeField] private Row[] _rows = new Row[5];

        private DailyMissionService _missions;
        private IEventBus _events;

        public void Initialize(DailyMissionService missions)
        {
            _missions = missions;
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            Refresh();
        }

        private void OnDestroy() => _events?.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);

        private void OnCurrencyChanged(CurrencyChangedEvent _) => Refresh();

        private void Refresh()
        {
            if (_missions == null) return;

            var missions = _missions.Missions;
            for (int i = 0; i < _rows.Length; i++)
            {
                var row = _rows[i];
                bool used = i < missions.Count;
                row.Root.SetActive(used);
                if (!used) continue;

                var progress = missions[i];
                var spec = _missions.GetSpec(progress.MissionId);
                if (spec == null) { row.Root.SetActive(false); continue; }

                row.Description.text = spec.Description;
                row.Progress.SetText("{0}/{1}", progress.Progress, spec.Target);

                bool claimable = !progress.Claimed && progress.Progress >= spec.Target;
                row.ClaimButton.interactable = claimable;

                string missionId = progress.MissionId;
                row.ClaimButton.onClick.RemoveAllListeners();
                row.ClaimButton.onClick.AddListener(() =>
                {
                    _missions.TryClaim(missionId);
                    Refresh(); // claim → CurrencyChangedEvent already refreshes, but a failed claim should also re-sync
                });
            }
        }
    }
}
