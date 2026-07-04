using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// Save-file protection (Loop 20, hardened post-review): AES-256-CBC with
    /// encrypt-then-MAC (HMAC-SHA256), plugged into SaveService's IPayloadTransform seam.
    ///
    /// why encrypt-then-MAC: CBC alone is malleable and gives zero integrity — tampering
    /// was only caught if padding happened to break. The HMAC turns any modification into
    /// a deterministic InvalidDataException, which SaveService already treats as corrupt
    /// (falls back to backup, then fresh model).
    ///
    /// why NO plaintext fallback: accepting plaintext would let a cheater bypass the
    /// scheme by simply writing unencrypted JSON. Nothing shipped before this format,
    /// so there are no legacy saves to migrate.
    ///
    /// Purpose remains deterrence, NOT DRM: keys derive from data present on the device
    /// (appSalt ships in the build, deviceId is local), so a rooted device can always
    /// extract them. Server authority is the only real answer and is out of scope offline.
    ///
    /// Format: base64( magic 'M','C', formatVersion(1) , IV[16] , ciphertext , HMAC[32] )
    /// HMAC covers magic + version + IV + ciphertext. Distinct enc/mac keys via domain
    /// separation suffixes. Fresh random IV per write.
    /// </summary>
    public sealed class AesPayloadTransform : SaveService.IPayloadTransform
    {
        private const byte Magic0 = (byte)'M';
        private const byte Magic1 = (byte)'C';
        private const byte FormatVersion = 2;
        private const int IvSize = 16;
        private const int MacSize = 32;
        private const int HeaderSize = 3; // magic(2) + version(1)

        private readonly byte[] _encKey;
        private readonly byte[] _macKey;

        public AesPayloadTransform(string appSalt, string deviceId)
        {
            using var sha = SHA256.Create();
            _encKey = sha.ComputeHash(Encoding.UTF8.GetBytes(appSalt + deviceId + ":enc"));
            _macKey = sha.ComputeHash(Encoding.UTF8.GetBytes(appSalt + deviceId + ":mac"));
        }

        public string Encode(string plainJson)
        {
            using var aes = Aes.Create();
            aes.Key = _encKey;
            aes.GenerateIV();

            byte[] cipher;
            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] plain = Encoding.UTF8.GetBytes(plainJson);
                cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
            }

            byte[] output = new byte[HeaderSize + IvSize + cipher.Length + MacSize];
            output[0] = Magic0;
            output[1] = Magic1;
            output[2] = FormatVersion;
            Buffer.BlockCopy(aes.IV, 0, output, HeaderSize, IvSize);
            Buffer.BlockCopy(cipher, 0, output, HeaderSize + IvSize, cipher.Length);

            using (var hmac = new HMACSHA256(_macKey))
            {
                byte[] mac = hmac.ComputeHash(output, 0, HeaderSize + IvSize + cipher.Length);
                Buffer.BlockCopy(mac, 0, output, output.Length - MacSize, MacSize);
            }

            return Convert.ToBase64String(output);
        }

        public string Decode(string stored)
        {
            // Any failure below throws; SaveService's corrupt-file path handles it.
            byte[] raw = Convert.FromBase64String(stored);

            if (raw.Length < HeaderSize + IvSize + MacSize + 1 ||
                raw[0] != Magic0 || raw[1] != Magic1 || raw[2] != FormatVersion)
                throw new InvalidDataException("Save payload has an unknown format.");

            // Verify MAC before touching the cipher (encrypt-then-MAC discipline).
            int macOffset = raw.Length - MacSize;
            using (var hmac = new HMACSHA256(_macKey))
            {
                byte[] expected = hmac.ComputeHash(raw, 0, macOffset);
                if (!FixedTimeEquals(expected, raw, macOffset))
                    throw new InvalidDataException("Save payload failed integrity check.");
            }

            using var aes = Aes.Create();
            aes.Key = _encKey;
            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(raw, HeaderSize, iv, 0, IvSize);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            int cipherLength = macOffset - HeaderSize - IvSize;
            byte[] plain = decryptor.TransformFinalBlock(raw, HeaderSize + IvSize, cipherLength);
            return Encoding.UTF8.GetString(plain);
        }

        /// <summary>Constant-time comparison — no early exit that could leak match length via timing.</summary>
        private static bool FixedTimeEquals(byte[] expected, byte[] actual, int actualOffset)
        {
            if (actual.Length - actualOffset != expected.Length) return false;
            int diff = 0;
            for (int i = 0; i < expected.Length; i++)
                diff |= expected[i] ^ actual[actualOffset + i];
            return diff == 0;
        }
    }
}
