using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTxt.Utils
{
    public static class CryptoUtils
    {
        // Nova chave AES-256 fixa (32 bytes) para publicação aberta
        private static readonly byte[] key = new byte[32] {
            0x7A, 0xC3, 0x1F, 0xB2, 0xD4, 0xE5, 0xA7, 0x5C,
            0x9F, 0x22, 0x38, 0x4B, 0x6E, 0x11, 0x87, 0xDA,
            0x3C, 0x5E, 0x8B, 0xA1, 0xF0, 0x2D, 0x74, 0xC6,
            0x1A, 0xB8, 0xE3, 0x59, 0x6D, 0xC2, 0x4F, 0x90
        };
        // Novo IV fixo (16 bytes)
        private static readonly byte[] iv = new byte[16] {
            0x21, 0x43, 0x65, 0x87, 0xA9, 0xCB, 0xED, 0x0F,
            0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE
        };



        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
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
                aes.Key = key;
                aes.IV = iv;
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
