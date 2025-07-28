using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AutismCenter.Infrastructure.Services;

public class ContentFormattingService : IContentFormattingService
{
    private readonly Dictionary<Language, CultureInfo> _languageCultures = new()
    {
        { Language.English, new CultureInfo("en-US") },
        { Language.Arabic, new CultureInfo("ar-BH") }
    };

    public string FormatContentForLanguage(string content, Language language)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var formattedContent = content;

        // Apply language-specific formatting
        if (language == Language.Arabic)
        {
            // Convert numbers to Arabic-Indic numerals if needed
            formattedContent = ConvertToArabicNumerals(formattedContent);
            
            // Ensure proper RTL text direction
            if (!formattedContent.StartsWith("\u202B") && ContainsArabicText(formattedContent))
            {
                formattedContent = "\u202B" + formattedContent + "\u202C"; // RTL override
            }
        }
        else if (language == Language.English)
        {
            // Ensure LTR text direction for mixed content
            if (ContainsArabicText(formattedContent))
            {
                formattedContent = "\u202D" + formattedContent + "\u202C"; // LTR override
            }
        }

        return formattedContent;
    }

    public string FormatDateForLanguage(DateTime date, Language language)
    {
        var culture = _languageCultures[language];
        
        return language switch
        {
            Language.Arabic => date.ToString("dd/MM/yyyy", culture),
            Language.English => date.ToString("MM/dd/yyyy", culture),
            _ => date.ToString("yyyy-MM-dd")
        };
    }

    public string FormatCurrencyForLanguage(decimal amount, string currencyCode, Language language)
    {
        var culture = _languageCultures[language];
        
        return language switch
        {
            Language.Arabic => $"{amount:N2} {currencyCode}",
            Language.English => $"{currencyCode} {amount:N2}",
            _ => $"{amount:N2} {currencyCode}"
        };
    }

    public string FormatNumberForLanguage(decimal number, Language language)
    {
        var culture = _languageCultures[language];
        var formatted = number.ToString("N", culture);
        
        if (language == Language.Arabic)
        {
            formatted = ConvertToArabicNumerals(formatted);
        }
        
        return formatted;
    }

    public bool IsRightToLeft(Language language)
    {
        return language == Language.Arabic;
    }

    public string GetLanguageDirection(Language language)
    {
        return IsRightToLeft(language) ? "rtl" : "ltr";
    }

    public string SanitizeHtmlContent(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
            return htmlContent;

        // Remove potentially dangerous HTML tags and attributes
        var allowedTags = new[] { "p", "br", "strong", "em", "u", "span", "div", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li" };
        var allowedAttributes = new[] { "class", "style", "dir" };

        // This is a basic implementation - in production, use a proper HTML sanitizer like HtmlSanitizer
        var sanitized = htmlContent;
        
        // Remove script tags
        sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        // Remove dangerous attributes
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
        
        return sanitized;
    }

    public string ValidateContentForLanguage(string content, Language language)
    {
        if (string.IsNullOrEmpty(content))
            return "Content cannot be empty";

        if (language == Language.Arabic)
        {
            if (!ContainsArabicText(content) && content.Length > 10)
            {
                return "Warning: Content marked as Arabic but contains no Arabic characters";
            }
        }
        else if (language == Language.English)
        {
            if (!ContainsLatinText(content) && content.Length > 10)
            {
                return "Warning: Content marked as English but contains no Latin characters";
            }
        }

        return string.Empty;
    }

    private string ConvertToArabicNumerals(string text)
    {
        var arabicNumerals = new Dictionary<char, char>
        {
            {'0', '٠'}, {'1', '١'}, {'2', '٢'}, {'3', '٣'}, {'4', '٤'},
            {'5', '٥'}, {'6', '٦'}, {'7', '٧'}, {'8', '٨'}, {'9', '٩'}
        };

        var result = text.ToCharArray();
        for (int i = 0; i < result.Length; i++)
        {
            if (arabicNumerals.ContainsKey(result[i]))
            {
                result[i] = arabicNumerals[result[i]];
            }
        }

        return new string(result);
    }

    private bool ContainsArabicText(string text)
    {
        return text.Any(c => c >= 0x0600 && c <= 0x06FF); // Arabic Unicode block
    }

    private bool ContainsLatinText(string text)
    {
        return text.Any(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));
    }
}