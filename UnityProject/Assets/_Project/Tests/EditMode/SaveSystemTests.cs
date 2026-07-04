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
        public void AesTransform_RejectsPlaintext()
        {
            // Security: plaintext must never be accepted — that would let cheaters
            // bypass encryption by writing raw JSON into the save file.
            var transform = new AesPayloadTransform("test.salt", "device-123");
            Assert.Catch(() => transform.Decode("{\"Version\":1,\"Coins\":999999}"));
        }

        [Test]
        public void AesTransform_RejectsTamperedCiphertext()
        {
            var transform = new AesPayloadTransform("test.salt", "device-123");
            string encoded = transform.Encode("{\"Coins\":1}");

            byte[] raw = System.Convert.FromBase64String(encoded);
            raw[raw.Length / 2] ^= 0xFF; // flip one ciphertext byte
            string tampered = System.Convert.ToBase64String(raw);

            Assert.Catch(() => transform.Decode(tampered)); // HMAC must reject it
        }

        [Test]
        public void AesTransform_RejectsWrongDeviceKey()
        {
            var deviceA = new AesPayloadTransform("test.salt", "device-A");
            var deviceB = new AesPayloadTransform("test.salt", "device-B");
            string encoded = deviceA.Encode("{\"Coins\":1}");
            Assert.Catch(() => deviceB.Decode(encoded));
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
