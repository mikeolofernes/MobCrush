using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// File-backed <see cref="ISaveService"/>.
    /// Guarantees: atomic writes (temp file + replace) so a crash mid-write never corrupts the
    /// only copy; ordered migrations so any historical save loads; corrupt file falls back to
    /// backup, then to a fresh model — the game must never crash on bad save data (DoD rule 7).
    /// Encryption + multi-slot arrive in Loop 20 via the IPayloadTransform seam below.
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        /// <summary>Seam for Loop 20's encryption without changing this class's logic.</summary>
        public interface IPayloadTransform
        {
            string Encode(string plainJson);
            string Decode(string stored);
        }

        private sealed class PassthroughTransform : IPayloadTransform
        {
            public string Encode(string plainJson) => plainJson;
            public string Decode(string stored) => stored;
        }

        public SaveModel Data { get; private set; }

        private readonly string _path;
        private readonly string _backupPath;
        private readonly IReadOnlyList<ISaveMigration> _migrations;
        private readonly IPayloadTransform _transform;

        public SaveService(string fileName, IEnumerable<ISaveMigration> migrations, IPayloadTransform transform = null)
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
            _backupPath = _path + ".bak";
            _migrations = (migrations ?? Array.Empty<ISaveMigration>()).OrderBy(m => m.FromVersion).ToList();
            _transform = transform ?? new PassthroughTransform();
        }

        public void Load()
        {
            Data = TryLoadFrom(_path) ?? TryLoadFrom(_backupPath) ?? new SaveModel();
        }

        public void Save()
        {
            if (Data == null) Data = new SaveModel();
            Data.LastSavedUtcTicks = DateTime.UtcNow.Ticks;

            string json = _transform.Encode(JsonConvert.SerializeObject(Data));
            string tmp = _path + ".tmp";

            try
            {
                // Atomic-ish sequence: write tmp → rotate current to backup → move tmp into place.
                File.WriteAllText(tmp, json);
                if (File.Exists(_path))
                {
                    File.Copy(_path, _backupPath, overwrite: true);
                    File.Delete(_path);
                }
                File.Move(tmp, _path);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveService: write failed — {e.Message}");
            }
        }

        public void DeleteAll()
        {
            try
            {
                if (File.Exists(_path)) File.Delete(_path);
                if (File.Exists(_backupPath)) File.Delete(_backupPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveService: delete failed — {e.Message}");
            }
            Data = new SaveModel();
        }

        private SaveModel TryLoadFrom(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;

                var jObject = JObject.Parse(_transform.Decode(File.ReadAllText(path)));
                int version = jObject.Value<int?>("Version") ?? 1;

                // Run each applicable migration exactly once, in order. The local version
                // is advanced alongside the JSON so the comparison stays correct even if a
                // future migration list is not strictly monotonic.
                foreach (var migration in _migrations)
                {
                    if (migration.FromVersion >= version)
                    {
                        migration.Apply(jObject);
                        version = migration.FromVersion + 1;
                        jObject["Version"] = version;
                    }
                }

                return jObject.ToObject<SaveModel>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: could not load '{path}' ({e.Message}); trying fallback.");
                return null;
            }
        }
    }
}
