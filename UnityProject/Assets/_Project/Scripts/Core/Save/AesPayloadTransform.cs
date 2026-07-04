using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MobCrush.Core.Save
{
    /// <summary>
    /// AES-256-CBC save encryption (Loop 20) plugged into SaveService's IPayloadTransform seam.
    /// Purpose: raise the bar against casual editing of local saves — NOT DRM (a rooted
    /// device with the binary can always extract the key; server authority is the only
    /// real answer and out of scope for an offline game).
    /// Format: base64( IV[16] + ciphertext ). Fresh random IV per write.
    /// Key = SHA-256(appSalt + deviceId): saves don't trivially copy between devices,
    /// yet the scheme needs no key storage or user secrets.
    /// </summary>
    public sealed class AesPayloadTransform : SaveService.IPayloadTransform
    {
        private readonly byte[] _key;

        public AesPayloadTransform(string appSalt, string deviceId)
        {
            using var sha = SHA256.Create();
            _key = sha.ComputeHash(Encoding.UTF8.GetBytes(appSalt + deviceId));
        }

        public string Encode(string plainJson)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] plain = Encoding.UTF8.GetBytes(plainJson);
            byte[] cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);

            byte[] output = new byte[aes.IV.Length + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, output, 0, aes.IV.Length);
            Buffer.BlockCopy(cipher, 0, output, aes.IV.Length, cipher.Length);
            return Convert.ToBase64String(output);
        }

        public string Decode(string stored)
        {
            byte[] raw;
            try
            {
                raw = Convert.FromBase64String(stored);
            }
            catch (FormatException)
            {
                // Pre-encryption save (Loop 4-19 players): plain JSON passes through once,
                // and the next Save() writes it encrypted — a free in-place migration.
                return stored;
            }
            if (raw.Length <= 16) throw new InvalidDataException("Save payload too short.");

            using var aes = Aes.Create();
            aes.Key = _key;
            byte[] iv = new byte[16];
            Buffer.BlockCopy(raw, 0, iv, 0, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] plain = decryptor.TransformFinalBlock(raw, 16, raw.Length - 16);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
