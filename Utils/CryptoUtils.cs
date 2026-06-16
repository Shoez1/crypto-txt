using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace CryptoTxt.Utils
{
    public static class CryptoUtils
    {
        private static readonly byte[] EmbeddedCompatibilityKey = new byte[32]
        {
            0x2F, 0xC8, 0xA1, 0xB7, 0x43, 0xD5, 0xE9, 0x16,
            0x7B, 0x5C, 0x0A, 0xF4, 0x3D, 0x82, 0x69, 0x1E,
            0xC3, 0x54, 0x98, 0x20, 0xAD, 0x71, 0xEF, 0x36,
            0x8B, 0xD0, 0x25, 0xFA, 0x47, 0x6E, 0x13, 0x9C
        };

        // Mantido apenas para descriptografar arquivos antigos em AES-CBC.
        private static readonly byte[] EmbeddedCompatibilityIV = new byte[16]
        {
            0x6A, 0xC1, 0x3F, 0xB2, 0xD8, 0xE7, 0x45, 0x0C,
            0xF2, 0x9D, 0x18, 0x57, 0xA3, 0x24, 0xE6, 0x5B
        };

        private static readonly byte[] UserKeyMagic = Encoding.ASCII.GetBytes("CTXU1");
        private static readonly byte[] UserKeyEntropy = Encoding.ASCII.GetBytes("CryptoTxt user key store v1");
        private static readonly byte[] FormatMagicV2 = Encoding.ASCII.GetBytes("CTX2");
        private static readonly byte[] FileKeyInfo = Encoding.ASCII.GetBytes("CryptoTxt file encryption v2");
        private const int KeySize = 32;
        private const int LegacyIVSize = 16;
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private static byte[]? importedKey;
        private static byte[]? importedIV;

        public static bool IsCustomKeyActive => importedKey != null && importedIV != null;

        public static void ImportKeyAndIV(byte[] customKey, byte[] customIV)
        {
            ValidateKey(customKey);
            ValidateIv(customIV);
            ClearImportedKeyAndIV();
            importedKey = (byte[])customKey.Clone();
            importedIV = (byte[])customIV.Clone();
        }

        public static void ClearImportedKeyAndIV()
        {
            ClearSensitiveBytes(importedKey);
            ClearSensitiveBytes(importedIV);
            importedKey = null;
            importedIV = null;
        }

        public static void ExportKeyAndIV(string filePath)
        {
            (byte[] keyBytes, byte[] ivBytes) = GetActiveKeyMaterial();
            byte[] all = new byte[KeySize + LegacyIVSize];

            try
            {
                Buffer.BlockCopy(keyBytes, 0, all, 0, KeySize);
                Buffer.BlockCopy(ivBytes, 0, all, KeySize, LegacyIVSize);
                string base64 = Convert.ToBase64String(all);
                File.WriteAllText(filePath, base64, new UTF8Encoding(false));
            }
            finally
            {
                ClearSensitiveBytes(keyBytes);
                ClearSensitiveBytes(ivBytes);
                ClearSensitiveBytes(all);
            }
        }

        public static bool ImportKeyAndIVFromFile(string filePath)
        {
            byte[] all;
            string content = File.ReadAllText(filePath).Trim();

            try
            {
                all = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                all = File.ReadAllBytes(filePath);
            }

            if (all.Length != KeySize + LegacyIVSize)
            {
                ClearSensitiveBytes(all);
                return false;
            }

            byte[] keyBytes = all.Take(KeySize).ToArray();
            byte[] ivBytes = all.Skip(KeySize).Take(LegacyIVSize).ToArray();

            try
            {
                ImportKeyAndIV(keyBytes, ivBytes);
                return true;
            }
            finally
            {
                ClearSensitiveBytes(all);
                ClearSensitiveBytes(keyBytes);
                ClearSensitiveBytes(ivBytes);
            }
        }

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            try
            {
                return Convert.ToBase64String(EncryptBytes(plainBytes));
            }
            finally
            {
                ClearSensitiveBytes(plainBytes);
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = DecryptBytes(encryptedBytes);

            try
            {
                return Encoding.UTF8.GetString(plainBytes);
            }
            finally
            {
                ClearSensitiveBytes(encryptedBytes);
                ClearSensitiveBytes(plainBytes);
            }
        }

        public static byte[] EncryptBytes(byte[] data)
        {
            (byte[] keyBytes, byte[] ivBytes) = GetActiveKeyMaterial();

            try
            {
                return EncryptBytesWithKey(data, keyBytes, ivBytes);
            }
            finally
            {
                ClearSensitiveBytes(keyBytes);
                ClearSensitiveBytes(ivBytes);
            }
        }

        public static byte[] DecryptBytes(byte[] data)
        {
            (byte[] keyBytes, byte[] ivBytes) = GetActiveKeyMaterial();

            try
            {
                try
                {
                    return DecryptBytesWithKey(data, keyBytes, ivBytes);
                }
                catch (CryptographicException) when (!IsCustomKeyActive && !UsesEmbeddedCompatibilityKey(keyBytes))
                {
                    return DecryptBytesWithKey(data, EmbeddedCompatibilityKey, EmbeddedCompatibilityIV);
                }
            }
            finally
            {
                ClearSensitiveBytes(keyBytes);
                ClearSensitiveBytes(ivBytes);
            }
        }

        public static byte[] DecryptBytesWithKey(byte[] data, byte[] customKey, byte[] customIV)
        {
            ValidateKey(customKey);

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (IsFormat(data, FormatMagicV2))
            {
                return DecryptCurrentFormatV2(data, customKey);
            }

            ValidateIv(customIV);
            return DecryptLegacyFormat(data, customKey, customIV);
        }

        public static byte[] EncryptBytesWithKey(byte[] data, byte[] customKey, byte[] customIV)
        {
            ValidateKey(customKey);

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] encryptionKey = DeriveFileKey(customKey, salt);
            byte[] ciphertext = new byte[data.Length];
            byte[] tag = new byte[TagSize];

            try
            {
                using (var aes = new AesGcm(encryptionKey, TagSize))
                {
                    aes.Encrypt(nonce, data, ciphertext, tag);
                }

                using var output = new MemoryStream(FormatMagicV2.Length + SaltSize + NonceSize + TagSize + ciphertext.Length);
                output.Write(FormatMagicV2, 0, FormatMagicV2.Length);
                output.Write(salt, 0, salt.Length);
                output.Write(nonce, 0, nonce.Length);
                output.Write(tag, 0, tag.Length);
                output.Write(ciphertext, 0, ciphertext.Length);
                return output.ToArray();
            }
            finally
            {
                ClearSensitiveBytes(encryptionKey);
                ClearSensitiveBytes(salt);
                ClearSensitiveBytes(nonce);
                ClearSensitiveBytes(tag);
                ClearSensitiveBytes(ciphertext);
            }
        }

        public static void ClearSensitiveBytes(byte[]? data)
        {
            if (data != null && data.Length > 0)
            {
                CryptographicOperations.ZeroMemory(data);
            }
        }

        private static byte[] DecryptCurrentFormatV2(byte[] data, byte[] customKey)
        {
            int minimumSize = FormatMagicV2.Length + SaltSize + NonceSize + TagSize;
            if (data.Length < minimumSize)
            {
                throw new CryptographicException("Arquivo criptografado inválido ou truncado.");
            }

            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[data.Length - minimumSize];
            byte[] encryptionKey = Array.Empty<byte>();

            Buffer.BlockCopy(data, FormatMagicV2.Length, salt, 0, salt.Length);
            Buffer.BlockCopy(data, FormatMagicV2.Length + salt.Length, nonce, 0, nonce.Length);
            Buffer.BlockCopy(data, FormatMagicV2.Length + salt.Length + nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(data, minimumSize, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[ciphertext.Length];

            try
            {
                encryptionKey = DeriveFileKey(customKey, salt);
                using var aes = new AesGcm(encryptionKey, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);
                return plaintext;
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException(
                    "Falha ao validar os dados criptografados. A chave pode estar incorreta ou o arquivo pode ter sido alterado.",
                    ex);
            }
            finally
            {
                ClearSensitiveBytes(encryptionKey);
                ClearSensitiveBytes(salt);
                ClearSensitiveBytes(nonce);
                ClearSensitiveBytes(tag);
                ClearSensitiveBytes(ciphertext);
            }
        }

        private static byte[] DecryptLegacyFormat(byte[] data, byte[] customKey, byte[] customIV)
        {
            using Aes aes = Aes.Create();
            aes.Key = customKey;
            aes.IV = customIV;

            using var input = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var output = new MemoryStream();
            cryptoStream.CopyTo(output);
            return output.ToArray();
        }

        private static (byte[] Key, byte[] IV) GetActiveKeyMaterial()
        {
            if (importedKey != null && importedIV != null)
            {
                return ((byte[])importedKey.Clone(), (byte[])importedIV.Clone());
            }

            return GetOrCreateUserKeyMaterial();
        }

        private static (byte[] Key, byte[] IV) GetOrCreateUserKeyMaterial()
        {
            string keyFilePath = GetUserKeyFilePath();
            string keyDirectory = Path.GetDirectoryName(keyFilePath)
                ?? throw new InvalidOperationException("Não foi possível resolver a pasta da chave local.");

            Directory.CreateDirectory(keyDirectory);

            if (File.Exists(keyFilePath))
            {
                return ReadUserKeyMaterial(keyFilePath);
            }

            byte[] payload = CreateUserKeyPayload();
            byte[]? protectedPayload = null;

            try
            {
                protectedPayload = ProtectedData.Protect(payload, UserKeyEntropy, DataProtectionScope.CurrentUser);

                try
                {
                    using var keyFile = new FileStream(keyFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    keyFile.Write(protectedPayload, 0, protectedPayload.Length);
                }
                catch (IOException) when (File.Exists(keyFilePath))
                {
                    return ReadUserKeyMaterial(keyFilePath);
                }

                return ParseUserKeyPayload(payload);
            }
            finally
            {
                ClearSensitiveBytes(payload);
                ClearSensitiveBytes(protectedPayload);
            }
        }

        private static string GetUserKeyFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "CryptoTxt", "user-key.dat");
        }

        private static byte[] CreateUserKeyPayload()
        {
            byte[] payload = new byte[UserKeyMagic.Length + KeySize + LegacyIVSize];
            byte[] keyBytes = RandomNumberGenerator.GetBytes(KeySize);
            byte[] ivBytes = RandomNumberGenerator.GetBytes(LegacyIVSize);

            try
            {
                Buffer.BlockCopy(UserKeyMagic, 0, payload, 0, UserKeyMagic.Length);
                Buffer.BlockCopy(keyBytes, 0, payload, UserKeyMagic.Length, keyBytes.Length);
                Buffer.BlockCopy(ivBytes, 0, payload, UserKeyMagic.Length + keyBytes.Length, ivBytes.Length);
                return payload;
            }
            finally
            {
                ClearSensitiveBytes(keyBytes);
                ClearSensitiveBytes(ivBytes);
            }
        }

        private static (byte[] Key, byte[] IV) ReadUserKeyMaterial(string keyFilePath)
        {
            byte[] protectedPayload = File.ReadAllBytes(keyFilePath);
            byte[]? payload = null;

            try
            {
                payload = ProtectedData.Unprotect(protectedPayload, UserKeyEntropy, DataProtectionScope.CurrentUser);
                return ParseUserKeyPayload(payload);
            }
            finally
            {
                ClearSensitiveBytes(protectedPayload);
                ClearSensitiveBytes(payload);
            }
        }

        private static (byte[] Key, byte[] IV) ParseUserKeyPayload(byte[] payload)
        {
            int expectedSize = UserKeyMagic.Length + KeySize + LegacyIVSize;
            if (payload.Length != expectedSize || !IsFormat(payload, UserKeyMagic))
            {
                throw new CryptographicException("Arquivo de chave local inválido.");
            }

            byte[] keyBytes = new byte[KeySize];
            byte[] ivBytes = new byte[LegacyIVSize];
            Buffer.BlockCopy(payload, UserKeyMagic.Length, keyBytes, 0, keyBytes.Length);
            Buffer.BlockCopy(payload, UserKeyMagic.Length + keyBytes.Length, ivBytes, 0, ivBytes.Length);
            return (keyBytes, ivBytes);
        }

        private static byte[] DeriveFileKey(byte[] rootKey, byte[] salt)
        {
            return HkdfSha256(rootKey, salt, FileKeyInfo, KeySize);
        }

        private static byte[] HkdfSha256(byte[] inputKeyMaterial, byte[] salt, byte[] info, int outputLength)
        {
            using var extract = new HMACSHA256(salt);
            byte[] pseudoRandomKey = extract.ComputeHash(inputKeyMaterial);

            try
            {
                using var expand = new HMACSHA256(pseudoRandomKey);
                byte[] output = new byte[outputLength];
                byte[] previousBlock = Array.Empty<byte>();
                int bytesWritten = 0;
                byte counter = 1;

                while (bytesWritten < outputLength)
                {
                    byte[] blockInput = new byte[previousBlock.Length + info.Length + 1];
                    Buffer.BlockCopy(previousBlock, 0, blockInput, 0, previousBlock.Length);
                    Buffer.BlockCopy(info, 0, blockInput, previousBlock.Length, info.Length);
                    blockInput[blockInput.Length - 1] = counter;

                    byte[] currentBlock = expand.ComputeHash(blockInput);
                    int bytesToCopy = Math.Min(currentBlock.Length, outputLength - bytesWritten);
                    Buffer.BlockCopy(currentBlock, 0, output, bytesWritten, bytesToCopy);

                    ClearSensitiveBytes(previousBlock);
                    ClearSensitiveBytes(blockInput);
                    previousBlock = currentBlock;
                    bytesWritten += bytesToCopy;
                    counter++;
                }

                ClearSensitiveBytes(previousBlock);
                return output;
            }
            finally
            {
                ClearSensitiveBytes(pseudoRandomKey);
            }
        }

        private static bool UsesEmbeddedCompatibilityKey(byte[] keyBytes)
        {
            return keyBytes.Length == EmbeddedCompatibilityKey.Length
                && CryptographicOperations.FixedTimeEquals(keyBytes, EmbeddedCompatibilityKey);
        }

        private static bool IsFormat(byte[] data, byte[] magic)
        {
            if (data.Length < magic.Length)
            {
                return false;
            }

            for (int index = 0; index < magic.Length; index++)
            {
                if (data[index] != magic[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateKey(byte[] customKey)
        {
            if (customKey == null)
            {
                throw new ArgumentNullException(nameof(customKey));
            }

            if (customKey.Length != KeySize)
            {
                throw new ArgumentException("A chave precisa ter 32 bytes para AES-256.", nameof(customKey));
            }
        }

        private static void ValidateIv(byte[] customIV)
        {
            if (customIV == null)
            {
                throw new ArgumentNullException(nameof(customIV));
            }

            if (customIV.Length != LegacyIVSize)
            {
                throw new ArgumentException("O IV precisa ter 16 bytes para compatibilidade com o formato antigo.", nameof(customIV));
            }
        }
    }
}
