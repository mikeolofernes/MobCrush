using MobCrush.Core.Save;
using NUnit.Framework;

namespace MobCrush.Tests
{
    /// <summary>Save encryption + model round-trip tests (Loop 22).</summary>
    public sealed class SaveSystemTests
    {
        [Test]
        public void AesTransform_RoundTrips()
        {
            var transform = new AesPayloadTransform("test.salt", "device-123");
            const string json = "{\"Version\":1,\"Coins\":42}";

            string encoded = transform.Encode(json);
            Assert.AreNotEqual(json, encoded);
            Assert.AreEqual(json, transform.Decode(encoded));
        }

        [Test]
        public void AesTransform_UniqueIvPerWrite()
        {
            var transform = new AesPayloadTransform("test.salt", "device-123");
            const string json = "{\"Coins\":1}";
            Assert.AreNotEqual(transform.Encode(json), transform.Encode(json)); // random IV → distinct ciphertexts
        }

        [Test]
        public void AesTransform_PassesThroughLegacyPlaintext()
        {
            var transform = new AesPayloadTransform("test.salt", "device-123");
            const string legacy = "{\"Version\":1}"; // not valid base64 → treated as pre-encryption save
            Assert.AreEqual(legacy, transform.Decode(legacy));
        }

        [Test]
        public void SaveModel_Defaults_AreSafe()
        {
            var model = new SaveModel();
            Assert.AreEqual(1, model.Version);
            Assert.IsNotNull(model.Inventory);
            Assert.IsNotNull(model.TalentLevels);
            Assert.IsNotNull(model.EquippedBySlot);
        }
    }
}
