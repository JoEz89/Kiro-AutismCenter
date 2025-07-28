using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly Mock<IEmailTemplateRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<EmailTemplateService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        _mockRepository = new Mock<IEmailTemplateRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<EmailTemplateService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _service = new EmailTemplateService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetEmailTemplateAsync_WithExistingTemplate_ShouldReturnTemplate()
    {
        // Arrange
        var templateKey = "welcome_email";
        var language = Language.English;
        var expectedSubject = "Welcome to our platform";
        var expectedBody = "Hello {{firstName}}, welcome!";
        var emailTemplate = new EmailTemplate(templateKey, language, expectedSubject, expectedBody);

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync(emailTemplate);

        // Act
        var result = await _service.GetEmailTemplateAsync(templateKey, language);

        // Assert
        Assert.Equal(expectedSubject, result.Subject);
        Assert.Equal(expectedBody, result.Body);
        _mockRepository.Verify(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language), Times.Once);
    }

    [Fact]
    public async Task GetEmailTemplateAsync_WithNonExistingTemplate_ShouldReturnDefaultTemplate()
    {
        // Arrange
        var templateKey = "non_existing_template";
        var language = Language.English;

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _service.GetEmailTemplateAsync(templateKey, language);

        // Assert
        Assert.Equal($"Subject for {templateKey}", result.Subject);
        Assert.Equal($"Body for {templateKey}", result.Body);
        _mockRepository.Verify(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language), Times.Once);
    }

    [Fact]
    public async Task GetEmailTemplateAsync_WithArabicNotFoundButEnglishExists_ShouldReturnEnglishTemplate()
    {
        // Arrange
        var templateKey = "welcome_email";
        var expectedSubject = "Welcome to our platform";
        var expectedBody = "Hello {{firstName}}, welcome!";
        var englishTemplate = new EmailTemplate(templateKey, Language.English, expectedSubject, expectedBody);

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, Language.Arabic))
            .ReturnsAsync((EmailTemplate?)null);
        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, Language.English))
            .ReturnsAsync(englishTemplate);

        // Act
        var result = await _service.GetEmailTemplateAsync(templateKey, Language.Arabic);

        // Assert
        Assert.Equal(expectedSubject, result.Subject);
        Assert.Equal(expectedBody, result.Body);
        _mockRepository.Verify(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, Language.Arabic), Times.Once);
        _mockRepository.Verify(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, Language.English), Times.Once);
    }

    [Fact]
    public async Task ProcessEmailTemplateAsync_WithValidTemplate_ShouldProcessParameters()
    {
        // Arrange
        var templateKey = "welcome_email";
        var language = Language.English;
        var subject = "Welcome {{firstName}}";
        var body = "Hello {{firstName}}, your email is {{email}}";
        var emailTemplate = new EmailTemplate(templateKey, language, subject, body);
        var parameters = new Dictionary<string, object>
        {
            { "firstName", "John" },
            { "email", "john@example.com" }
        };

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync(emailTemplate);

        // Act
        var result = await _service.ProcessEmailTemplateAsync(templateKey, language, parameters);

        // Assert
        Assert.Contains("Welcome John", result);
        Assert.Contains("Hello John, your email is john@example.com", result);
    }

    [Fact]
    public async Task SetEmailTemplateAsync_WithNewTemplate_ShouldAddTemplate()
    {
        // Arrange
        var templateKey = "new_template";
        var language = Language.English;
        var subject = "New Subject";
        var body = "New Body";
        var updatedBy = "admin";

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync((EmailTemplate?)null);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SetEmailTemplateAsync(templateKey, language, subject, body, updatedBy);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<EmailTemplate>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetEmailTemplateAsync_WithExistingTemplate_ShouldUpdateTemplate()
    {
        // Arrange
        var templateKey = "existing_template";
        var language = Language.English;
        var newSubject = "Updated Subject";
        var newBody = "Updated Body";
        var updatedBy = "admin";
        var existingTemplate = new EmailTemplate(templateKey, language, "Old Subject", "Old Body");

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync(existingTemplate);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SetEmailTemplateAsync(templateKey, language, newSubject, newBody, updatedBy);

        // Assert
        Assert.True(result);
        Assert.Equal(newSubject, existingTemplate.Subject);
        Assert.Equal(newBody, existingTemplate.Body);
        _mockRepository.Verify(r => r.UpdateAsync(existingTemplate), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailTemplateAsync_WithExistingTemplate_ShouldUpdateTemplate()
    {
        // Arrange
        var templateKey = "existing_template";
        var language = Language.English;
        var newSubject = "Updated Subject";
        var newBody = "Updated Body";
        var updatedBy = "admin";
        var existingTemplate = new EmailTemplate(templateKey, language, "Old Subject", "Old Body");

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync(existingTemplate);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateEmailTemplateAsync(templateKey, language, newSubject, newBody, updatedBy);

        // Assert
        Assert.True(result);
        Assert.Equal(newSubject, existingTemplate.Subject);
        Assert.Equal(newBody, existingTemplate.Body);
        _mockRepository.Verify(r => r.UpdateAsync(existingTemplate), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailTemplateAsync_WithNonExistingTemplate_ShouldReturnFalse()
    {
        // Arrange
        var templateKey = "non_existing_template";
        var language = Language.English;
        var newSubject = "Updated Subject";
        var newBody = "Updated Body";
        var updatedBy = "admin";

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _service.UpdateEmailTemplateAsync(templateKey, language, newSubject, newBody, updatedBy);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailTemplate>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEmailTemplateAsync_WithExistingTemplate_ShouldDeleteTemplate()
    {
        // Arrange
        var templateKey = "existing_template";
        var language = Language.English;
        var existingTemplate = new EmailTemplate(templateKey, language, "Subject", "Body");

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync(existingTemplate);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteEmailTemplateAsync(templateKey, language);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(existingTemplate.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmailTemplateAsync_WithNonExistingTemplate_ShouldReturnFalse()
    {
        // Arrange
        var templateKey = "non_existing_template";
        var language = Language.English;

        _mockRepository.Setup(r => r.GetByTemplateKeyAndLanguageAsync(templateKey, language))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _service.DeleteEmailTemplateAsync(templateKey, language);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TemplateExistsAsync_WithExistingTemplate_ShouldReturnTrue()
    {
        // Arrange
        var templateKey = "existing_template";
        var language = Language.English;

        _mockRepository.Setup(r => r.ExistsAsync(templateKey, language))
            .ReturnsAsync(true);

        // Act
        var result = await _service.TemplateExistsAsync(templateKey, language);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(templateKey, language), Times.Once);
    }

    [Fact]
    public async Task TemplateExistsAsync_WithNonExistingTemplate_ShouldReturnFalse()
    {
        // Arrange
        var templateKey = "non_existing_template";
        var language = Language.English;

        _mockRepository.Setup(r => r.ExistsAsync(templateKey, language))
            .ReturnsAsync(false);

        // Act
        var result = await _service.TemplateExistsAsync(templateKey, language);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.ExistsAsync(templateKey, language), Times.Once);
    }

    [Fact]
    public async Task GetTemplateKeysAsync_ShouldReturnTemplateKeys()
    {
        // Arrange
        var expectedKeys = new List<string> { "welcome_email", "password_reset", "verification" };

        _mockRepository.Setup(r => r.GetTemplateKeysAsync())
            .ReturnsAsync(expectedKeys);

        // Act
        var result = await _service.GetTemplateKeysAsync();

        // Assert
        Assert.Equal(expectedKeys, result);
        _mockRepository.Verify(r => r.GetTemplateKeysAsync(), Times.Once);
    }
}