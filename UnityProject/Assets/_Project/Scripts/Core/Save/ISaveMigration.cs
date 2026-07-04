using Newtonsoft.Json.Linq;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// One schema step (FromVersion -> FromVersion+1), applied to the raw JSON before
    /// deserialization. why JObject: migrations must be able to read fields that no longer
    /// exist on the current SaveModel type.
    /// </summary>
    public interface ISaveMigration
    {
        int FromVersion { get; }
        void Apply(JObject saveJson);
    }
}
