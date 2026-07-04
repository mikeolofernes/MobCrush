using MobCrush.Core.Events;
using MobCrush.Core.SceneFlow;
using MobCrush.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// End-of-run screen (Loop 17): victory/defeat header, run stats, rewards, and the
    /// return-to-menu intent. Listens for RunEndedEvent; reward GRANTING is the run flow's
    /// job — this view only displays and navigates.
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private string _menuSceneName = "Menu";

        private IEventBus _events;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<RunEndedEvent>(OnRunEnded);
            _continueButton.onClick.AddListener(OnContinue);
            _panel.SetActive(false);
        }

        private void OnDestroy() => _events.Unsubscribe<RunEndedEvent>(OnRunEnded);

        private void OnRunEnded(RunEndedEvent e)
        {
            _panel.SetActive(true);
            _headerText.text = e.Victory ? "STAGE CLEARED" : "OVERRUN";
            int minutes = (int)(e.DurationSeconds / 60f);
            int seconds = (int)(e.DurationSeconds % 60f);
            _statsText.text = $"Time  {minutes:00}:{seconds:00}\nKills  {e.Kills}\nLevel  {e.LevelReached}";
        }

        private async void OnContinue()
        {
            ServiceLocator.Get<IGameStateMachine>().Set(GameState.MainMenu);
            await ServiceLocator.Get<ISceneLoader>().LoadSceneAsync(_menuSceneName);
        }
    }
}
