using AutismCenter.Domain.Enums;
using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class ContentFormattingServiceTests
{
    private readonly ContentFormattingService _service;

    public ContentFormattingServiceTests()
    {
        _service = new ContentFormattingService();
    }

    [Fact]
    public void FormatContentForLanguage_WithArabicContent_ShouldApplyRTLFormatting()
    {
        // Arrange
        var arabicContent = "مرحبا بكم في موقعنا";
        
        // Act
        var result = _service.FormatContentForLanguage(arabicContent, Language.Arabic);
        
        // Assert
        Assert.Contains("\u202B", result); // RTL override character
        Assert.Contains("\u202C", result); // Pop directional formatting character
    }

    [Fact]
    public void FormatContentForLanguage_WithEnglishContent_ShouldNotApplyRTLFormatting()
    {
        // Arrange
        var englishContent = "Welcome to our website";
        
        // Act
        var result = _service.FormatContentForLanguage(englishContent, Language.English);
        
        // Assert
        Assert.Equal(englishContent, result);
    }

    [Fact]
    public void FormatDateForLanguage_WithArabicLanguage_ShouldReturnDDMMYYYYFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);
        
        // Act
        var result = _service.FormatDateForLanguage(date, Language.Arabic);
        
        // Assert
        // Arabic culture may include RTL marks, so we check if it contains the expected date parts
        Assert.Contains("15", result);
        Assert.Contains("03", result);
        Assert.Contains("2024", result);
        Assert.Contains("/", result);
    }

    [Fact]
    public void FormatDateForLanguage_WithEnglishLanguage_ShouldReturnMMDDYYYYFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);
        
        // Act
        var result = _service.FormatDateForLanguage(date, Language.English);
        
        // Assert
        Assert.Equal("03/15/2024", result);
    }

    [Fact]
    public void FormatCurrencyForLanguage_WithArabicLanguage_ShouldPutCurrencyAfterAmount()
    {
        // Arrange
        var amount = 123.45m;
        var currencyCode = "BHD";
        
        // Act
        var result = _service.FormatCurrencyForLanguage(amount, currencyCode, Language.Arabic);
        
        // Assert
        Assert.Equal("123.45 BHD", result);
    }

    [Fact]
    public void FormatCurrencyForLanguage_WithEnglishLanguage_ShouldPutCurrencyBeforeAmount()
    {
        // Arrange
        var amount = 123.45m;
        var currencyCode = "BHD";
        
        // Act
        var result = _service.FormatCurrencyForLanguage(amount, currencyCode, Language.English);
        
        // Assert
        Assert.Equal("BHD 123.45", result);
    }

    [Fact]
    public void IsRightToLeft_WithArabicLanguage_ShouldReturnTrue()
    {
        // Act
        var result = _service.IsRightToLeft(Language.Arabic);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRightToLeft_WithEnglishLanguage_ShouldReturnFalse()
    {
        // Act
        var result = _service.IsRightToLeft(Language.English);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetLanguageDirection_WithArabicLanguage_ShouldReturnRTL()
    {
        // Act
        var result = _service.GetLanguageDirection(Language.Arabic);
        
        // Assert
        Assert.Equal("rtl", result);
    }

    [Fact]
    public void GetLanguageDirection_WithEnglishLanguage_ShouldReturnLTR()
    {
        // Act
        var result = _service.GetLanguageDirection(Language.English);
        
        // Assert
        Assert.Equal("ltr", result);
    }

    [Fact]
    public void SanitizeHtmlContent_WithScriptTags_ShouldRemoveScriptTags()
    {
        // Arrange
        var htmlContent = "<p>Safe content</p><script>alert('dangerous');</script><p>More safe content</p>";
        
        // Act
        var result = _service.SanitizeHtmlContent(htmlContent);
        
        // Assert
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("alert", result);
        Assert.Contains("<p>Safe content</p>", result);
        Assert.Contains("<p>More safe content</p>", result);
    }

    [Fact]
    public void SanitizeHtmlContent_WithDangerousAttributes_ShouldRemoveDangerousAttributes()
    {
        // Arrange
        var htmlContent = "<p onclick='alert(\"dangerous\")'>Content</p>";
        
        // Act
        var result = _service.SanitizeHtmlContent(htmlContent);
        
        // Assert
        Assert.DoesNotContain("onclick", result);
        Assert.Contains("<p", result);
        Assert.Contains("Content", result);
    }

    [Fact]
    public void ValidateContentForLanguage_WithArabicContentAndArabicLanguage_ShouldReturnEmpty()
    {
        // Arrange
        var arabicContent = "مرحبا بكم في موقعنا الإلكتروني";
        
        // Act
        var result = _service.ValidateContentForLanguage(arabicContent, Language.Arabic);
        
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateContentForLanguage_WithEnglishContentAndArabicLanguage_ShouldReturnWarning()
    {
        // Arrange
        var englishContent = "Welcome to our website with lots of content";
        
        // Act
        var result = _service.ValidateContentForLanguage(englishContent, Language.Arabic);
        
        // Assert
        Assert.Contains("Warning", result);
        Assert.Contains("Arabic", result);
    }

    [Fact]
    public void ValidateContentForLanguage_WithEmptyContent_ShouldReturnError()
    {
        // Arrange
        var emptyContent = "";
        
        // Act
        var result = _service.ValidateContentForLanguage(emptyContent, Language.English);
        
        // Assert
        Assert.Equal("Content cannot be empty", result);
    }
}