using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using Xunit;

namespace AutismCenter.Domain.Tests.Entities;

public class EmailTemplateTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEmailTemplate()
    {
        // Arrange
        var templateKey = "welcome_email";
        var language = Language.English;
        var subject = "Welcome to our platform";
        var body = "Hello {{firstName}}, welcome to our platform!";
        var description = "Welcome email template";
        var createdBy = "admin";

        // Act
        var emailTemplate = new EmailTemplate(templateKey, language, subject, body, description, createdBy);

        // Assert
        Assert.Equal(templateKey, emailTemplate.TemplateKey);
        Assert.Equal(language, emailTemplate.Language);
        Assert.Equal(subject, emailTemplate.Subject);
        Assert.Equal(body, emailTemplate.Body);
        Assert.Equal(description, emailTemplate.Description);
        Assert.Equal(createdBy, emailTemplate.CreatedBy);
        Assert.True(emailTemplate.IsActive);
        Assert.True(emailTemplate.Id != Guid.Empty);
    }

    [Fact]
    public void Constructor_WithNullTemplateKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        string templateKey = null!;
        var language = Language.English;
        var subject = "Welcome";
        var body = "Hello";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EmailTemplate(templateKey, language, subject, body));
    }

    [Fact]
    public void Constructor_WithNullSubject_ShouldThrowArgumentNullException()
    {
        // Arrange
        var templateKey = "welcome_email";
        var language = Language.English;
        string subject = null!;
        var body = "Hello";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EmailTemplate(templateKey, language, subject, body));
    }

    [Fact]
    public void Constructor_WithNullBody_ShouldThrowArgumentNullException()
    {
        // Arrange
        var templateKey = "welcome_email";
        var language = Language.English;
        var subject = "Welcome";
        string body = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EmailTemplate(templateKey, language, subject, body));
    }

    [Fact]
    public void UpdateTemplate_WithValidParameters_ShouldUpdateTemplateAndTimestamp()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "old subject", "old body");
        var newSubject = "new subject";
        var newBody = "new body";
        var updatedBy = "admin";
        var originalUpdatedAt = emailTemplate.UpdatedAt;

        // Act
        emailTemplate.UpdateTemplate(newSubject, newBody, updatedBy);

        // Assert
        Assert.Equal(newSubject, emailTemplate.Subject);
        Assert.Equal(newBody, emailTemplate.Body);
        Assert.Equal(updatedBy, emailTemplate.UpdatedBy);
        Assert.True(emailTemplate.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateTemplate_WithNullSubject_ShouldThrowArgumentNullException()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "subject", "body");
        string newSubject = null!;
        var newBody = "new body";
        var updatedBy = "admin";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            emailTemplate.UpdateTemplate(newSubject, newBody, updatedBy));
    }

    [Fact]
    public void UpdateTemplate_WithNullBody_ShouldThrowArgumentNullException()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "subject", "body");
        var newSubject = "new subject";
        string newBody = null!;
        var updatedBy = "admin";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            emailTemplate.UpdateTemplate(newSubject, newBody, updatedBy));
    }

    [Fact]
    public void UpdateDescription_WithValidParameters_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "subject", "body");
        var newDescription = "new description";
        var updatedBy = "admin";
        var originalUpdatedAt = emailTemplate.UpdatedAt;

        // Act
        emailTemplate.UpdateDescription(newDescription, updatedBy);

        // Assert
        Assert.Equal(newDescription, emailTemplate.Description);
        Assert.Equal(updatedBy, emailTemplate.UpdatedBy);
        Assert.True(emailTemplate.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrueAndUpdateTimestamp()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "subject", "body");
        emailTemplate.Deactivate("admin");
        var updatedBy = "admin";
        var originalUpdatedAt = emailTemplate.UpdatedAt;

        // Act
        emailTemplate.Activate(updatedBy);

        // Assert
        Assert.True(emailTemplate.IsActive);
        Assert.Equal(updatedBy, emailTemplate.UpdatedBy);
        Assert.True(emailTemplate.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        // Arrange
        var emailTemplate = new EmailTemplate("key", Language.English, "subject", "body");
        var updatedBy = "admin";
        var originalUpdatedAt = emailTemplate.UpdatedAt;

        // Act
        emailTemplate.Deactivate(updatedBy);

        // Assert
        Assert.False(emailTemplate.IsActive);
        Assert.Equal(updatedBy, emailTemplate.UpdatedBy);
        Assert.True(emailTemplate.UpdatedAt > originalUpdatedAt);
    }
}