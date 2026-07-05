using MobCrush.Core.Input;
using MobCrush.Core.SceneFlow;
using MobCrush.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// Pause menu (production-readiness fix: IInputService.PauseRequested previously had
    /// NO listener — Escape/Start did nothing and there was no way to pause or quit a run).
    /// Pause is only honored during Playing; GameManager freezes timeScale for us.
    /// The gameplay input map stays enabled so the pause key also unpauses.
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _quitToMenuButton;
        [SerializeField] private string _menuSceneName = "Menu";

        private IInputService _input;
        private IGameStateMachine _state;

        private void Awake()
        {
            _input = ServiceLocator.Get<IInputService>();
            _state = ServiceLocator.Get<IGameStateMachine>();

            _input.PauseRequested += OnPauseRequested;
            _resumeButton.onClick.AddListener(Resume);
            _quitToMenuButton.onClick.AddListener(QuitToMenu);
            _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_input != null) _input.PauseRequested -= OnPauseRequested;
        }

        private void OnPauseRequested()
        {
            // Toggle semantics; ignore during drafts/death/results — those own the flow.
            if (_state.Current == GameState.Playing) Pause();
            else if (_state.Current == GameState.Paused) Resume();
        }

        /// <summary>Also wired to the HUD pause button.</summary>
        public void Pause()
        {
            if (_state.Current != GameState.Playing) return;
            _state.Set(GameState.Paused);
            _panel.SetActive(true);
        }

        public void Resume()
        {
            if (_state.Current != GameState.Paused) return;
            _panel.SetActive(false);
            _state.Set(GameState.Playing);
        }

        private async void QuitToMenu()
        {
            _panel.SetActive(false);
            // Leaving mid-run forfeits it — no RunEndedEvent, no rewards; by design.
            _state.Set(GameState.MainMenu); // also restores timeScale before the load
            await ServiceLocator.Get<ISceneLoader>().LoadSceneAsync(_menuSceneName);
        }
    }
}
