using System.Collections.Generic;
using MobCrush.Core.Audio;
using MobCrush.Core.Events;
using MobCrush.Core.Input;
using MobCrush.Core.Pooling;
using MobCrush.Core.Save;
using MobCrush.Core.Services;
using UnityEngine;

namespace MobCrush.Core.SceneFlow
{
    /// <summary>
    /// THE composition root. Lives alone in the Boot scene on a DontDestroyOnLoad object,
    /// constructs every core service in dependency order, registers them in the
    /// ServiceLocator, then hands control to the main menu.
    /// why a single root: service construction order is explicit and reviewable here,
    /// instead of scattered across Awake() race conditions.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Scene-owned dependencies")]
        [SerializeField] private AudioService _audioService;   // MonoBehaviour services live as children of this object
        [SerializeField] private InputService _inputService;
        [SerializeField] private CanvasGroup _loadingOverlay;
        [SerializeField] private Transform _poolRoot;

        [Header("Flow")]
        [SerializeField] private string _mainMenuScene = "Menu";

        [Header("Save (Loop 20)")]
        [Tooltip("Active slot 0-2; slot files are independent SaveService instances.")]
        [SerializeField, Range(0, 2)] private int _saveSlot = 0;
        [Tooltip("App-specific salt mixed into the device-bound AES key. Changing it orphans existing saves.")]
        [SerializeField] private string _saveSalt = "mobcrush.v1";

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60; // mobile default; Loop 21 makes this device-adaptive

            // --- Construction order: leaf services first, flow last. ---
            var eventBus = new EventBus();
            ServiceLocator.Register<IEventBus>(eventBus);

            var poolService = new PoolService(_poolRoot);
            ServiceLocator.Register<IPoolService>(poolService);

            // Encrypted, slotted persistence: slot_<n>.sav, AES key bound to this device.
            var transform = new AesPayloadTransform(_saveSalt, SystemInfo.deviceUniqueIdentifier);
            var saveService = new SaveService($"slot_{_saveSlot}.sav", GetMigrations(), transform);
            saveService.Load();
            ServiceLocator.Register<ISaveService>(saveService);

            var autosave = gameObject.AddComponent<AutosaveController>();
            autosave.Initialize(saveService, eventBus);

            ServiceLocator.Register<IAudioService>(_audioService);
            ServiceLocator.Register<IInputService>(_inputService);

            var sceneLoader = new SceneLoaderService(_loadingOverlay);
            ServiceLocator.Register<ISceneLoader>(sceneLoader);

            var gameManager = new GameManager();
            ServiceLocator.Register<IGameStateMachine>(gameManager);

            // Autosave on backgrounding — mobile processes die without warning.
            Application.focusChanged += focused => { if (!focused) saveService.Save(); };

            await sceneLoader.LoadSceneAsync(_mainMenuScene);
            gameManager.Set(GameState.MainMenu);
        }

        /// <summary>Save-schema migration chain. Append here whenever SaveModel.Version bumps.</summary>
        private static IEnumerable<ISaveMigration> GetMigrations()
        {
            yield break; // Version 1 — no migrations yet.
        }
    }
}
