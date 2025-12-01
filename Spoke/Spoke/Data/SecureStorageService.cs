using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Spoke.Data;

/// <summary>
/// Service for secure storage of sensitive data using platform-specific secure storage
/// </summary>
public class SecureStorageService
{
    private readonly string _encryptionKey;

    public SecureStorageService(string encryptionKey)
    {
        _encryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
    }

    /// <summary>
    /// Store encrypted data securely
    /// </summary>
    public async Task<bool> StoreAsync<T>(string key, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var encryptedData = Encrypt(json);
            await SecureStorage.Default.SetAsync(key, Convert.ToBase64String(encryptedData));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieve and decrypt data from secure storage
    /// </summary>
    public async Task<T?> RetrieveAsync<T>(string key)
    {
        try
        {
            var encryptedBase64 = await SecureStorage.Default.GetAsync(key);
            if (string.IsNullOrEmpty(encryptedBase64))
                return default;

            var encryptedData = Convert.FromBase64String(encryptedBase64);
            var json = Decrypt(encryptedData);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Remove data from secure storage
    /// </summary>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if key exists in secure storage
    /// </summary>
    public async Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Encrypt data using AES with the service's encryption key
    /// </summary>
    private byte[] Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        var key = DeriveKey(_encryptionKey);
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to encrypted data
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

        return result;
    }

    /// <summary>
    /// Decrypt data using AES with the service's encryption key
    /// </summary>
    private string Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        var key = DeriveKey(_encryptionKey);

        // Extract IV from beginning of data
        var iv = new byte[16];
        var cipherText = new byte[encryptedData.Length - 16];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
        Buffer.BlockCopy(encryptedData, 16, cipherText, 0, cipherText.Length);

        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// Derive a 256-bit key from the encryption key string
    /// </summary>
    private byte[] DeriveKey(string key)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes("SpokeSalt"), 10000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32); // 256 bits
    }
}