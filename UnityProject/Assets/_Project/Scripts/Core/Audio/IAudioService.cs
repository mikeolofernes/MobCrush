using UnityEngine;

namespace MobCrush.Core.Audio
{
    /// <summary>Music + SFX playback behind one interface (Loop 3 §9). Gameplay refers to clips by id, never by reference.</summary>
    public interface IAudioService
    {
        void PlaySfx(string clipId);
        void PlaySfxAt(string clipId, Vector3 worldPosition);
        void PlayMusic(string trackId, float crossfadeSeconds = 1f);
        void StopMusic(float fadeOutSeconds = 0.5f);

        /// <summary>0..1 linear volumes, persisted via settings save.</summary>
        void SetMusicVolume(float linear);
        void SetSfxVolume(float linear);
    }
}
