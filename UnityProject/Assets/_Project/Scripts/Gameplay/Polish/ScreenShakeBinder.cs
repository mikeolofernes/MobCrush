using MobCrush.Core.Events;
using MobCrush.Core.Services;
using UnityEngine;

namespace MobCrush.Gameplay.Polish
{
    /// <summary>
    /// Maps game events to camera shake (Loop 19), mirroring the audio binder pattern:
    /// one tunable place, zero shake knowledge inside gameplay systems.
    /// Amplitudes are inspector-tunable feel values.
    /// </summary>
    public sealed class ScreenShakeBinder : MonoBehaviour
    {
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private float _playerHitAmplitude = 0.25f;
        [SerializeField] private float _playerHitDuration = 0.2f;
        [SerializeField] private float _bossSpawnAmplitude = 0.5f;
        [SerializeField] private float _bossSpawnDuration = 0.6f;

        private IEventBus _events;

        private void Awake()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Subscribe<BossSpawnedEvent>(OnBossSpawned);
        }

        private void OnDestroy()
        {
            _events.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _events.Unsubscribe<BossSpawnedEvent>(OnBossSpawned);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent _) => _camera.Shake(_playerHitAmplitude, _playerHitDuration);
        private void OnBossSpawned(BossSpawnedEvent _) => _camera.Shake(_bossSpawnAmplitude, _bossSpawnDuration);
    }
}
