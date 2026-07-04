namespace MobCrush.Core.Save
{
    /// <summary>
    /// Persistence facade. Loop 4 ships JSON + atomic writes + migrations;
    /// Loop 20 adds encryption, slots, backups and autosave triggers behind this same interface.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>The live model. Mutate it, then call Save(). Never null after Load().</summary>
        SaveModel Data { get; }

        void Load();
        void Save();

        /// <summary>Deletes persisted data and resets to a fresh model (debug / GDPR erasure).</summary>
        void DeleteAll();
    }
}
