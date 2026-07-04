using MobCrush.Core.Audio;
using MobCrush.Core.Events;
using MobCrush.Core.Services;
using MobCrush.Gameplay.Enemies;
using UnityEngine;

namespace MobCrush.Gameplay.Audio
{
    /// <summary>
    /// Dynamic music (Loop 18): crossfades between a calm and an intense stage track
    /// based on live enemy pressure, and hard-switches to the boss track on boss spawn.
    /// Uses the existing AudioService dual-source crossfade — no new audio plumbing.
    /// Hysteresis (enter 60 / exit 35 enemies) prevents flip-flopping at the threshold.
    /// </summary>
    public sealed class DynamicMusicController : MonoBehaviour
    {
        [SerializeField] private EnemySystem _enemies;

        [Header("Track ids (SO_AudioLibrary)")]
        [SerializeField] private string _calmTrack = "music.stage.calm";
        [SerializeField] private string _intenseTrack = "music.stage.intense";
        [SerializeField] private string _bossTrack = "music.boss";

        [Header("Pressure thresholds (live enemies)")]
        [SerializeField] private int _intenseEnter = 60;
        [SerializeField] private int _intenseExit = 35;
        [SerializeField] private float _crossfadeSeconds = 2.5f;
        [SerializeField] private float _evaluateInterval = 1f; // once per second is plenty for music

        private IAudioService _audio;
        private IEventBus _events;
        private bool _intense;
        private bool _bossMode;
        private float _evalTimer;

        private void Awake()
        {
            _audio = ServiceLocator.Get<IAudioService>();
            _events = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<BossSpawnedEvent>(OnBossSpawned);
            _audio.PlayMusic(_calmTrack, 1f);
        }

        private void OnDestroy() => _events.Unsubscribe<BossSpawnedEvent>(OnBossSpawned);

        private void Update()
        {
            if (_bossMode) return;

            _evalTimer -= Time.deltaTime;
            if (_evalTimer > 0f) return;
            _evalTimer = _evaluateInterval;

            int pressure = _enemies.LiveCount;
            if (!_intense && pressure >= _intenseEnter)
            {
                _intense = true;
                _audio.PlayMusic(_intenseTrack, _crossfadeSeconds);
            }
            else if (_intense && pressure <= _intenseExit)
            {
                _intense = false;
                _audio.PlayMusic(_calmTrack, _crossfadeSeconds);
            }
        }

        private void OnBossSpawned(BossSpawnedEvent _)
        {
            _bossMode = true;
            _audio.PlayMusic(_bossTrack, 1.2f);
        }
    }
}
