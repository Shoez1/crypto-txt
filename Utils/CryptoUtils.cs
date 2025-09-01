using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace CryptoTxt.Utils
{
    public static class CryptoUtils
    {
        // Chave AES-256 fixa (32 bytes)
        private static readonly byte[] key = new byte[32] {
            0x2F, 0xC8, 0xA1, 0xB7, 0x43, 0xD5, 0xE9, 0x16,
            0x7B, 0x5C, 0x0A, 0xF4, 0x3D, 0x82, 0x69, 0x1E,
            0xC3, 0x54, 0x98, 0x20, 0xAD, 0x71, 0xEF, 0x36,
            0x8B, 0xD0, 0x25, 0xFA, 0x47, 0x6E, 0x13, 0x9C
        };
        // IV fixo (16 bytes)
        private static readonly byte[] iv = new byte[16] {
            0x6A, 0xC1, 0x3F, 0xB2, 0xD8, 0xE7, 0x45, 0x0C,
            0xF2, 0x9D, 0x18, 0x57, 0xA3, 0x24, 0xE6, 0x5B
        };

        private static byte[]? importedKey = null;
        private static byte[]? importedIV = null;

        public static void ImportKeyAndIV(byte[] customKey, byte[] customIV)
        {
            if (customKey == null || customKey.Length != 32) throw new ArgumentException("Chave deve ter 32 bytes");
            if (customIV == null || customIV.Length != 16) throw new ArgumentException("IV deve ter 16 bytes");
            importedKey = customKey;
            importedIV = customIV;
        }

        public static void ClearImportedKeyAndIV()
        {
            importedKey = null;
            importedIV = null;
        }

        public static bool IsCustomKeyActive => importedKey != null && importedIV != null;

        public static byte[] GetCurrentKey() => importedKey ?? key;
        public static byte[] GetCurrentIV() => importedIV ?? iv;

        public static void ExportKeyAndIV(string filePath)
        {
            byte[] all = new byte[48];
            Array.Copy(key, 0, all, 0, 32);
            Array.Copy(iv, 0, all, 32, 16);
            string base64 = Convert.ToBase64String(all);
            File.WriteAllText(filePath, base64);
        }

        public static bool ImportKeyAndIVFromFile(string filePath)
        {
            byte[] all;
            string content = File.ReadAllText(filePath).Trim();
            try { all = Convert.FromBase64String(content); }
            catch { all = null; }
            if (all == null || all.Length != 48)
            {
                all = File.ReadAllBytes(filePath);
            }
            if (all.Length != 48) return false;
            ImportKeyAndIV(all.Take(32).ToArray(), all.Skip(32).Take(16).ToArray());
            return true;
        }

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetCurrentKey();
                aes.IV = GetCurrentIV();
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var bytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetCurrentKey();
                aes.IV = GetCurrentIV();
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
