using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace AutismCenter.WebApi.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateSecureToken(int length = 32);
    string EncryptSensitiveData(string data);
    string DecryptSensitiveData(string encryptedData);
}

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _dataProtector;
    private readonly IDataProtector _sensitiveDataProtector;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider, ILogger<EncryptionService> logger)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("AutismCenter.GeneralData");
        _sensitiveDataProtector = dataProtectionProvider.CreateProtector("AutismCenter.SensitiveData.PII");
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            return _dataProtector.Protect(plainText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            return _dataProtector.Unprotect(cipherText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            throw new InvalidOperationException("Decryption failed", ex);
        }
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            // Use BCrypt for password hashing (industry standard)
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            throw new InvalidOperationException("Password hashing failed", ex);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify password");
            return false;
        }
    }

    public string GenerateSecureToken(int length = 32)
    {
        try
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate secure token");
            throw new InvalidOperationException("Token generation failed", ex);
        }
    }

    public string EncryptSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return string.Empty;

        try
        {
            // Use separate protector for sensitive PII data
            return _sensitiveDataProtector.Protect(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt sensitive data");
            throw new InvalidOperationException("Sensitive data encryption failed", ex);
        }
    }

    public string DecryptSensitiveData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return string.Empty;

        try
        {
            return _sensitiveDataProtector.Unprotect(encryptedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt sensitive data");
            throw new InvalidOperationException("Sensitive data decryption failed", ex);
        }
    }
}

/// <summary>
/// Extension methods for encrypting/decrypting sensitive data in entities
/// </summary>
public static class EncryptionExtensions
{
    public static string EncryptIfNotEmpty(this IEncryptionService encryptionService, string? value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : encryptionService.EncryptSensitiveData(value);
    }

    public static string? DecryptIfNotEmpty(this IEncryptionService encryptionService, string? encryptedValue)
    {
        return string.IsNullOrEmpty(encryptedValue) ? null : encryptionService.DecryptSensitiveData(encryptedValue);
    }
}