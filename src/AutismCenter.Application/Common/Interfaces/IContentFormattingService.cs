using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Common.Interfaces;

public interface IContentFormattingService
{
    string FormatContentForLanguage(string content, Language language);
    string FormatDateForLanguage(DateTime date, Language language);
    string FormatCurrencyForLanguage(decimal amount, string currencyCode, Language language);
    string FormatNumberForLanguage(decimal number, Language language);
    bool IsRightToLeft(Language language);
    string GetLanguageDirection(Language language);
    string SanitizeHtmlContent(string htmlContent);
    string ValidateContentForLanguage(string content, Language language);
}