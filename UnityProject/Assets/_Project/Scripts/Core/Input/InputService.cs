using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MobCrush.Core.Input
{
    /// <summary>
    /// New Input System wrapper. A single "Move" action bound to: on-screen joystick
    /// (via OnScreenStick feeding a virtual Gamepad), WASD/arrows, and gamepad left stick —
    /// one code path for all devices. MonoBehaviour because InputAction callbacks want a
    /// stable owner with enable/disable lifecycle.
    /// </summary>
    public sealed class InputService : MonoBehaviour, IInputService
    {
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _gameplayMapName = "Gameplay";
        [SerializeField] private string _uiMapName = "UI";

        public Vector2 Move { get; private set; }
        public event Action PauseRequested;

        private InputActionMap _gameplayMap;
        private InputAction _moveAction;
        private InputAction _pauseAction;

        private void Awake()
        {
            _gameplayMap = _actions.FindActionMap(_gameplayMapName, throwIfNotFound: true);
            _moveAction = _gameplayMap.FindAction("Move", throwIfNotFound: true);
            _pauseAction = _gameplayMap.FindAction("Pause", throwIfNotFound: true);

            _pauseAction.performed += OnPausePerformed;
            _actions.FindActionMap(_uiMapName)?.Enable();
            _gameplayMap.Enable();
        }

        private void OnDestroy()
        {
            if (_pauseAction != null) _pauseAction.performed -= OnPausePerformed;
        }

        private void Update()
        {
            // Poll rather than callback for movement: one read per frame beats
            // per-event delegate churn for a continuously-held stick.
            Vector2 raw = _moveAction.enabled ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            Move = raw.sqrMagnitude > 1f ? raw.normalized : raw;
        }

        public void SetUiMode(bool uiMode)
        {
            if (uiMode) _gameplayMap.Disable();
            else _gameplayMap.Enable();
            if (uiMode) Move = Vector2.zero; // never leak a stale move vector into a paused game
        }

        private void OnPausePerformed(InputAction.CallbackContext _) => PauseRequested?.Invoke();
    }
}
