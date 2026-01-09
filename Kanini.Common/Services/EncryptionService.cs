using Kanini.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Kanini.Common.Services;

public class EncryptionService : IEncryptionService
{
    private readonly ILogger<EncryptionService> _logger;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(ILogger<EncryptionService> logger)
    {
        _logger = logger;
        // In production, get from secure configuration
        _key = Encoding.UTF8.GetBytes("MySecretKey12345MySecretKey12345"); // 32 bytes for AES-256
        _iv = Encoding.UTF8.GetBytes("MyInitVector1234"); // 16 bytes for AES
    }

    public string Encrypt(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cs);
            
            writer.Write(plainText);
            writer.Close();
            
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.EncryptionFailed, plainText?.Length ?? 0);
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.DecryptionFailed, cipherText?.Length ?? 0);
            throw;
        }
    }

    public string Hash(string input)
    {
        try
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.HashingFailed, input?.Length ?? 0);
            throw;
        }
    }
}