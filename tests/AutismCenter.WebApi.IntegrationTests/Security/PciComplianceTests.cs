using AutismCenter.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Security;

public class PciComplianceTests
{
    private readonly IPciComplianceService _pciComplianceService;
    private readonly Mock<IAuditLoggingService> _auditLoggingServiceMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;

    public PciComplianceTests()
    {
        _auditLoggingServiceMock = new Mock<IAuditLoggingService>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        var loggerMock = new Mock<ILogger<PciComplianceService>>();

        _encryptionServiceMock.Setup(x => x.GenerateSecureToken(It.IsAny<int>()))
            .Returns("secure_token_12345");

        _pciComplianceService = new PciComplianceService(
            _auditLoggingServiceMock.Object,
            _encryptionServiceMock.Object,
            loggerMock.Object);
    }

    [Theory]
    [InlineData("4111111111111111", "4111********1111")] // Visa
    [InlineData("5555555555554444", "5555********4444")] // MasterCard
    [InlineData("378282246310005", "3782*******0005")] // American Express
    [InlineData("6011111111111117", "6011********1117")] // Discover
    public void MaskCreditCardNumber_ShouldMaskCorrectly(string cardNumber, string expectedMask)
    {
        // Act
        var result = _pciComplianceService.MaskCreditCardNumber(cardNumber);

        // Assert
        Assert.Equal(expectedMask, result);
    }

    [Theory]
    [InlineData("4111111111111111")] // Visa
    [InlineData("5555555555554444")] // MasterCard
    [InlineData("378282246310005")] // American Express
    [InlineData("6011111111111117")] // Discover
    public void IsValidCreditCardNumber_WithValidCards_ShouldReturnTrue(string cardNumber)
    {
        // Act
        var result = _pciComplianceService.IsValidCreditCardNumber(cardNumber);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("1234567890123456")] // Invalid Luhn
    [InlineData("4111111111111112")] // Invalid Luhn
    [InlineData("123")] // Too short
    [InlineData("abcd1234567890123")] // Contains letters
    public void IsValidCreditCardNumber_WithInvalidCards_ShouldReturnFalse(string cardNumber)
    {
        // Act
        var result = _pciComplianceService.IsValidCreditCardNumber(cardNumber);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("4111111111111111", "12/25", "123", true)]
    [InlineData("5555555555554444", "01/2025", "456", true)]
    [InlineData("1234567890123456", "12/25", "123", false)] // Invalid card
    [InlineData("4111111111111111", "13/25", "123", false)] // Invalid month
    [InlineData("4111111111111111", "12/20", "123", false)] // Expired
    [InlineData("4111111111111111", "12/25", "12", false)] // Invalid CVV
    public void ValidateCardDataFormat_ShouldValidateCorrectly(string cardNumber, string expiryDate, string cvv, bool expected)
    {
        // Act
        var result = _pciComplianceService.ValidateCardDataFormat(cardNumber, expiryDate, cvv);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TokenizeCardData_ShouldGenerateTokenAndLogEvent()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var expiryDate = "12/25";
        var cvv = "123";

        // Act
        var token = _pciComplianceService.TokenizeCardData(cardNumber, expiryDate, cvv);

        // Assert
        Assert.Equal("secure_token_12345", token);
        _encryptionServiceMock.Verify(x => x.GenerateSecureToken(32), Times.Once);
        
        // Verify audit logging was called
        _auditLoggingServiceMock.Verify(x => x.LogPaymentActionAsync(
            "CARD_TOKENIZATION",
            null,
            "secure_token_12345",
            null,
            null,
            It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("Please process payment for card 4111111111111111", true)]
    [InlineData("CVV code is 123", true)]
    [InlineData("Credit card number: 5555-5555-5555-4444", true)]
    [InlineData("Regular text without payment data", false)]
    [InlineData("Order total is $100", false)]
    public void IsPaymentDataPresent_ShouldDetectPaymentData(string data, bool expected)
    {
        // Act
        var result = _pciComplianceService.IsPaymentDataPresent(data);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Card number: 4111111111111111", "Card number: 4111********1111")]
    [InlineData("CVV: 123", "CVV: ***")]
    [InlineData("Process payment 5555-5555-5555-4444", "Process payment 5555********4444")]
    [InlineData("Regular text", "Regular text")]
    public void SanitizePaymentData_ShouldSanitizeCorrectly(string input, string expected)
    {
        // Act
        var result = _pciComplianceService.SanitizePaymentData(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task LogPaymentDataAccessAsync_ShouldLogAuditEvent()
    {
        // Arrange
        var action = "VIEW_PAYMENT_DETAILS";
        var userId = "user123";
        var orderId = "order456";

        // Act
        await _pciComplianceService.LogPaymentDataAccessAsync(action, userId, orderId);

        // Assert
        _auditLoggingServiceMock.Verify(x => x.LogPaymentActionAsync(
            action,
            orderId,
            null,
            userId,
            null,
            "Payment data access logged for PCI DSS compliance"), Times.Once);
    }
}

public class EncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        services.AddLogging();
        services.AddScoped<IEncryptionService, EncryptionService>();

        var serviceProvider = services.BuildServiceProvider();
        _encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();
    }

    [Fact]
    public void Encrypt_Decrypt_ShouldRoundTrip()
    {
        // Arrange
        var plainText = "Sensitive data that needs encryption";

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptSensitiveData_DecryptSensitiveData_ShouldRoundTrip()
    {
        // Arrange
        var sensitiveData = "Personal Identifiable Information";

        // Act
        var encrypted = _encryptionService.EncryptSensitiveData(sensitiveData);
        var decrypted = _encryptionService.DecryptSensitiveData(encrypted);

        // Assert
        Assert.NotEqual(sensitiveData, encrypted);
        Assert.Equal(sensitiveData, decrypted);
    }

    [Fact]
    public void HashPassword_ShouldCreateValidHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _encryptionService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.True(hash.Length > 50); // BCrypt hashes are typically 60 characters
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _encryptionService.HashPassword(password);

        // Act
        var result = _encryptionService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _encryptionService.HashPassword(password);

        // Act
        var result = _encryptionService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSecureToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = _encryptionService.GenerateSecureToken();
        var token2 = _encryptionService.GenerateSecureToken();

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.True(token1.Length > 0);
        Assert.True(token2.Length > 0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Encrypt_WithEmptyInput_ShouldReturnEmpty(string input)
    {
        // Act
        var result = _encryptionService.Encrypt(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void VerifyPassword_WithEmptyInput_ShouldReturnFalse(string input)
    {
        // Arrange
        var validHash = _encryptionService.HashPassword("ValidPassword123!");

        // Act
        var result = _encryptionService.VerifyPassword(input, validHash);

        // Assert
        Assert.False(result);
    }
}