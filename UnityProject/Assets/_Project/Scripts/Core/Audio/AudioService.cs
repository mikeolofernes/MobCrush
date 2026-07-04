using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MobCrush.Core.Audio
{
    /// <summary>
    /// MonoBehaviour <see cref="IAudioService"/> living on the persistent bootstrap object.
    /// Design (Loop 3 §9): fixed pool of AudioSources for SFX (no runtime AddComponent),
    /// two music sources for crossfading, per-frame duplicate-SFX throttling,
    /// volumes mapped to mixer dB. MonoBehaviour because playback needs coroutines + scene presence.
    /// </summary>
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioLibrary _library;
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private int _sfxSourceCount = 16;

        private AudioSource[] _sfxSources;
        private int _nextSfxIndex;
        private AudioSource _musicA, _musicB;
        private bool _musicOnA = true;

        // Duplicate throttle: same id at most twice per frame, else 100 dying enemies = white noise.
        private string _lastSfxId;
        private int _lastSfxFrame;
        private int _lastSfxCountThisFrame;

        private void Awake()
        {
            _sfxSources = new AudioSource[_sfxSourceCount];
            for (int i = 0; i < _sfxSourceCount; i++)
                _sfxSources[i] = CreateSource("SFX_" + i, _sfxGroup, loop: false);

            _musicA = CreateSource("Music_A", _musicGroup, loop: true);
            _musicB = CreateSource("Music_B", _musicGroup, loop: true);
        }

        public void PlaySfx(string clipId) => PlaySfxInternal(clipId, Vector3.zero, spatial: false);
        public void PlaySfxAt(string clipId, Vector3 worldPosition) => PlaySfxInternal(clipId, worldPosition, spatial: true);

        public void PlayMusic(string trackId, float crossfadeSeconds = 1f)
        {
            if (_library == null || !_library.TryGet(trackId, out var entry)) return;

            var incoming = _musicOnA ? _musicB : _musicA;
            var outgoing = _musicOnA ? _musicA : _musicB;
            _musicOnA = !_musicOnA;

            incoming.clip = entry.Clip;
            incoming.volume = 0f;
            incoming.Play();
            StopAllCoroutines();
            StartCoroutine(Crossfade(outgoing, incoming, entry.Volume, crossfadeSeconds));
        }

        public void StopMusic(float fadeOutSeconds = 0.5f)
        {
            StopAllCoroutines();
            var active = _musicOnA ? _musicA : _musicB;
            StartCoroutine(Crossfade(active, null, 0f, fadeOutSeconds));
        }

        public void SetMusicVolume(float linear) => SetMixerVolume("MusicVolume", linear);
        public void SetSfxVolume(float linear) => SetMixerVolume("SfxVolume", linear);

        private void SetMixerVolume(string param, float linear)
        {
            // why log10: mixer attenuation is in dB; a linear 0..1 slider mapped directly sounds wrong.
            float dB = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
            if (_mixer != null) _mixer.SetFloat(param, dB);
        }

        private void PlaySfxInternal(string clipId, Vector3 position, bool spatial)
        {
            if (_library == null || !_library.TryGet(clipId, out var entry)) return;

            if (Time.frameCount == _lastSfxFrame && clipId == _lastSfxId)
            {
                if (++_lastSfxCountThisFrame > 2) return; // throttle
            }
            else
            {
                _lastSfxId = clipId;
                _lastSfxFrame = Time.frameCount;
                _lastSfxCountThisFrame = 1;
            }

            var source = _sfxSources[_nextSfxIndex];
            _nextSfxIndex = (_nextSfxIndex + 1) % _sfxSources.Length; // round-robin voice stealing

            source.transform.position = position;
            source.spatialBlend = spatial ? 1f : 0f;
            source.pitch = 1f + Random.Range(-entry.PitchVariance, entry.PitchVariance);
            source.PlayOneShot(entry.Clip, entry.Volume);
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to, float targetVolume, float seconds)
        {
            float fromStart = from != null ? from.volume : 0f;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime; // music keeps fading while paused
                float k = seconds <= 0f ? 1f : t / seconds;
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (to != null) to.volume = Mathf.Lerp(0f, targetVolume, k);
                yield return null;
            }
            if (from != null) { from.Stop(); from.volume = 0f; }
            if (to != null) to.volume = targetVolume;
        }

        private AudioSource CreateSource(string name, AudioMixerGroup group, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = group;
            src.playOnAwake = false;
            src.loop = loop;
            return src;
        }
    }
}
