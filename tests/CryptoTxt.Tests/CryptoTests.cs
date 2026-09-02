using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CryptoCommon;
using CryptoTxt.Utils;

namespace CryptoTxt.Tests;

[TestClass]
public sealed class CryptoTests
{
    [TestMethod]
    public void EncryptDecrypt_DefaultKey_PreservesExactContent()
    {
        string original = "Teste de criptografia com acentuação: Olá, mundo! 🔒 12345\r\nSegunda linha.";
        string encrypted = CryptoUtils.Encrypt(original);
        Assert.IsFalse(string.IsNullOrWhiteSpace(encrypted));
        Assert.AreNotEqual(original, encrypted);

        string decrypted = CryptoUtils.Decrypt(encrypted);
        Assert.AreEqual(original, decrypted);
    }

    [TestMethod]
    public void EncryptDecrypt_CustomKeyMaterial_PreservesExactContent()
    {
        (byte[] key, byte[] iv) = SharedCrypto.GenerateNewKeyMaterial();
        string original = "Conteúdo seguro com chave personalizada CSK3!";
        byte[] originalBytes = Encoding.UTF8.GetBytes(original);

        byte[] encrypted = SharedCrypto.EncryptBytesWithKey(originalBytes, key, iv);
        Assert.IsTrue(SharedCrypto.HasAuthenticatedFormat(encrypted));

        byte[] decryptedBytes = SharedCrypto.DecryptBytesWithKey(encrypted, key, iv);
        string decrypted = Encoding.UTF8.GetString(decryptedBytes);
        Assert.AreEqual(original, decrypted);
    }

    [TestMethod]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        byte[] data = Encoding.UTF8.GetBytes("Mensagem confidencial");
        (byte[] key, byte[] iv) = SharedCrypto.GetOrCreateUserKeyMaterial();

        byte[] encrypted = SharedCrypto.EncryptBytesWithKey(data, key, iv);
        // Flip a bit in the ciphertext payload
        encrypted[^1] ^= 0x01;

        Assert.ThrowsExactly<CryptographicException>(() =>
        {
            SharedCrypto.DecryptBytesWithKey(encrypted, key, iv);
        });
    }

    [TestMethod]
    public void Decrypt_WrongKey_ThrowsCryptographicException()
    {
        byte[] data = Encoding.UTF8.GetBytes("Mensagem com chave 1");
        (byte[] key1, byte[] iv1) = SharedCrypto.GenerateNewKeyMaterial();
        (byte[] key2, byte[] iv2) = SharedCrypto.GenerateNewKeyMaterial();

        byte[] encrypted = SharedCrypto.EncryptBytesWithKey(data, key1, iv1);

        Assert.ThrowsExactly<CryptographicException>(() =>
        {
            SharedCrypto.DecryptBytesWithKey(encrypted, key2, iv2);
        });
    }

    [TestMethod]
    public void CSK3KeyFile_CreateAndParse_RoundTripsSuccessfully()
    {
        (byte[] key, byte[] iv) = SharedCrypto.GenerateNewKeyMaterial();
        byte[] keyFileBytes = SharedCrypto.CreateProtectedKeyFileBytes(key, iv);

        Assert.IsTrue(SharedCrypto.IsProtectedKeyFile(keyFileBytes));

        (byte[] parsedKey, byte[] parsedIv) = SharedCrypto.ParseProtectedKeyFile(keyFileBytes);
        CollectionAssert.AreEqual(key, parsedKey);
        CollectionAssert.AreEqual(iv, parsedIv);
    }

    [TestMethod]
    public void CSK3KeyFile_InvalidMagic_ThrowsInvalidOperationException()
    {
        byte[] corrupted = new byte[52];
        Array.Fill<byte>(corrupted, 0xFF);

        Assert.IsFalse(SharedCrypto.IsProtectedKeyFile(corrupted));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            SharedCrypto.ParseProtectedKeyFile(corrupted);
        });
    }

    [TestMethod]
    public void LoginConfiguration_ValidateCredentials_SuccessAndFailure()
    {
        string username = "admin";
        string password = "StrongPassword@123";
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        int iterations = 100000;
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);

        var config = new LoginConfiguration
        {
            UserName = username,
            PasswordSalt = salt,
            PasswordHash = hash,
            HashIterations = iterations,
            Hint = "Dica"
        };

        Assert.IsTrue(config.ValidateCredentials("admin", "StrongPassword@123"));
        Assert.IsFalse(config.ValidateCredentials("admin", "WrongPassword"));
        Assert.IsFalse(config.ValidateCredentials("wronguser", "StrongPassword@123"));
        Assert.IsFalse(config.ValidateCredentials("", ""));
    }

    [TestMethod]
    public void EncryptFile_DecryptFile_RoundTripFilePreserved()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CryptoTxtTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string sourceFile = Path.Combine(tempDir, "sample.txt");
            string encFile = Path.Combine(tempDir, "sample.txt.enc");
            string restoredFile = Path.Combine(tempDir, "sample_restored.txt");

            string content = "Linha 1 com acentuação: áéíóú çã\r\nLinha 2 com caracteres especiais: @#$%&*()!\r\nLinha 3: Final.";
            File.WriteAllText(sourceFile, content, Encoding.UTF8);

            CryptoUtils.EncryptFile(sourceFile, encFile);
            Assert.IsTrue(File.Exists(encFile));

            CryptoUtils.DecryptFile(encFile, restoredFile);
            Assert.IsTrue(File.Exists(restoredFile));

            string restoredContent = File.ReadAllText(restoredFile, Encoding.UTF8);
            Assert.AreEqual(content, restoredContent);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [TestMethod]
    public void ImportExportKey_FileRoundTrip()
    {
        string tempKeyPath = Path.Combine(Path.GetTempPath(), "test_csk3_" + Guid.NewGuid().ToString("N") + ".txt");

        try
        {
            (byte[] key, byte[] iv) = CryptoUtils.GenerateNewKeyMaterial();
            CryptoUtils.ImportKeyAndIV(key, iv);
            Assert.IsTrue(CryptoUtils.IsCustomKeyActive);
            Assert.AreEqual("Chave Personalizada (CSK3)", CryptoUtils.ActiveKeyDescription);

            CryptoUtils.ExportKeyAndIV(tempKeyPath);
            Assert.IsTrue(File.Exists(tempKeyPath));

            CryptoUtils.ClearImportedKeyAndIV();
            Assert.IsFalse(CryptoUtils.IsCustomKeyActive);
            Assert.AreEqual("Chave Padrão (Compartilhada)", CryptoUtils.ActiveKeyDescription);

            bool imported = CryptoUtils.ImportKeyAndIVFromFile(tempKeyPath);
            Assert.IsTrue(imported);
            Assert.IsTrue(CryptoUtils.IsCustomKeyActive);
        }
        finally
        {
            CryptoUtils.ClearImportedKeyAndIV();
            if (File.Exists(tempKeyPath))
            {
                File.Delete(tempKeyPath);
            }
        }
    }

    [TestMethod]
    public void BatchDirectoryEncryption_MultipleFiles_PreservesAllContents()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "CryptoTxtBatch_" + Guid.NewGuid().ToString("N"));
        string subDir = Path.Combine(baseDir, "subdir");
        Directory.CreateDirectory(subDir);

        try
        {
            string f1 = Path.Combine(baseDir, "doc1.txt");
            string f2 = Path.Combine(baseDir, "doc2.txt");
            string f3 = Path.Combine(subDir, "doc3.txt");

            File.WriteAllText(f1, "Conteúdo do documento 1", Encoding.UTF8);
            File.WriteAllText(f2, "Conteúdo do documento 2 com caracteres especiais: ®©™", Encoding.UTF8);
            File.WriteAllText(f3, "Conteúdo do documento 3 em subpasta", Encoding.UTF8);

            string[] txtFiles = Directory.GetFiles(baseDir, "*.txt", SearchOption.AllDirectories);
            Assert.HasCount(3, txtFiles);

            foreach (string file in txtFiles)
            {
                CryptoUtils.EncryptFile(file, file + ".enc");
                File.Delete(file); // remove original to test recovery
            }

            string[] encFiles = Directory.GetFiles(baseDir, "*.txt.enc", SearchOption.AllDirectories);
            Assert.HasCount(3, encFiles);

            foreach (string encFile in encFiles)
            {
                string targetPath = encFile.Substring(0, encFile.Length - ".enc".Length);
                CryptoUtils.DecryptFile(encFile, targetPath);
            }

            Assert.AreEqual("Conteúdo do documento 1", File.ReadAllText(f1, Encoding.UTF8));
            Assert.AreEqual("Conteúdo do documento 2 com caracteres especiais: ®©™", File.ReadAllText(f2, Encoding.UTF8));
            Assert.AreEqual("Conteúdo do documento 3 em subpasta", File.ReadAllText(f3, Encoding.UTF8));
        }
        finally
        {
            if (Directory.Exists(baseDir))
            {
                Directory.Delete(baseDir, true);
            }
        }
    }

    [TestMethod]
    public void EncryptDecrypt_EmptyStringAndLargeText_HandledCorrectly()
    {
        // Test empty string
        string emptyDecrypted = CryptoUtils.Decrypt(CryptoUtils.Encrypt(string.Empty));
        Assert.AreEqual(string.Empty, emptyDecrypted);

        // Test 500KB multiline text
        var sb = new StringBuilder();
        for (int i = 0; i < 5000; i++)
        {
            sb.AppendLine($"Linha número {i}: Registrando informações criptográficas com segurança no CryptoTxt.");
        }
        string largeText = sb.ToString();

        string encryptedLarge = CryptoUtils.Encrypt(largeText);
        string decryptedLarge = CryptoUtils.Decrypt(encryptedLarge);
        Assert.AreEqual(largeText, decryptedLarge);
    }
}
