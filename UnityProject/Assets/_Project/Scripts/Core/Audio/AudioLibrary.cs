using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCrush.Core.Audio
{
    /// <summary>
    /// ScriptableObject registry mapping string ids to clips + per-clip settings.
    /// why: gameplay code referencing an id ("sfx.enemy.die") instead of an AudioClip keeps
    /// MobCrush.Gameplay free of asset references and lets audio be swapped without code churn.
    /// </summary>
    [CreateAssetMenu(menuName = "MobCrush/Audio/Audio Library", fileName = "SO_AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Id;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume = 1f;
            [Tooltip("Random pitch spread to de-machine-gun repeated SFX.")]
            [Range(0f, 0.5f)] public float PitchVariance = 0.05f;
        }

        [SerializeField] private List<Entry> _entries = new();

        private Dictionary<string, Entry> _lookup;

        public bool TryGet(string id, out Entry entry)
        {
            // Lazy-build once; SO data is immutable at runtime (Loop 3 §5).
            _lookup ??= BuildLookup();
            return _lookup.TryGetValue(id, out entry);
        }

        private Dictionary<string, Entry> BuildLookup()
        {
            var map = new Dictionary<string, Entry>(_entries.Count);
            foreach (var e in _entries)
            {
                if (string.IsNullOrEmpty(e.Id) || e.Clip == null) continue;
                map[e.Id] = e;
            }
            return map;
        }
    }
}
