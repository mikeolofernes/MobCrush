using MobCrush.Core.Audio;
using MobCrush.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace MobCrush.UI
{
    /// <summary>
    /// Settings panel (Loop 17): music/SFX sliders persisted via PlayerPrefs (settings are
    /// device preferences, not progress — they deliberately bypass the encrypted save).
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        private const string MusicKey = "settings.music";
        private const string SfxKey = "settings.sfx";

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        private IAudioService _audio;

        private void Awake()
        {
            _audio = ServiceLocator.Get<IAudioService>();

            _musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 0.8f);
            _sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 1f);
            Apply();

            _musicSlider.onValueChanged.AddListener(_ => Apply());
            _sfxSlider.onValueChanged.AddListener(_ => Apply());
        }

        private void Apply()
        {
            _audio.SetMusicVolume(_musicSlider.value);
            _audio.SetSfxVolume(_sfxSlider.value);
            PlayerPrefs.SetFloat(MusicKey, _musicSlider.value);
            PlayerPrefs.SetFloat(SfxKey, _sfxSlider.value);
        }
    }
}
