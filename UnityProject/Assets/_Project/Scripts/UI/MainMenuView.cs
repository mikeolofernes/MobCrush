using MobCrush.Core.Events;
using MobCrush.Core.SceneFlow;
using MobCrush.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// Main menu (Loop 17): play intent, currency readout, navigation to sub-screens.
    /// Currency labels are event-driven so any purchase anywhere refreshes them.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _gemsText;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private string _gameSceneName = "Game";

        private IEventBus _events;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _playButton.onClick.AddListener(OnPlay);

            var save = ServiceLocator.Get<Core.Save.ISaveService>();
            _coinsText.SetText("{0}", save.Data.Coins);
            _gemsText.SetText("{0}", save.Data.Gems);
        }

        private void OnDestroy() => _events.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);

        private void OnCurrencyChanged(CurrencyChangedEvent e)
        {
            if (e.CurrencyId == "coins") _coinsText.SetText("{0}", e.NewAmount);
            else if (e.CurrencyId == "gems") _gemsText.SetText("{0}", e.NewAmount);
        }

        private async void OnPlay()
        {
            var state = ServiceLocator.Get<IGameStateMachine>();
            state.Set(GameState.LoadingRun);
            await ServiceLocator.Get<ISceneLoader>().LoadSceneAsync(_gameSceneName);
            state.Set(GameState.Playing);
        }

        public void ToggleSettings() => _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        public void ToggleInventory() => _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
    }
}
