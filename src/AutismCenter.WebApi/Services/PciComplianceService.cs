using System.Text.RegularExpressions;

namespace AutismCenter.WebApi.Services;

public interface IPciComplianceService
{
    string MaskCreditCardNumber(string cardNumber);
    bool IsValidCreditCardNumber(string cardNumber);
    string TokenizeCardData(string cardNumber, string expiryDate, string cvv);
    bool ValidateCardDataFormat(string cardNumber, string expiryDate, string cvv);
    Task LogPaymentDataAccessAsync(string action, string? userId, string? orderId = null);
    string SanitizePaymentData(string data);
    bool IsPaymentDataPresent(string data);
}

public class PciComplianceService : IPciComplianceService
{
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PciComplianceService> _logger;

    // PCI DSS compliant credit card patterns
    private static readonly Dictionary<string, Regex> CardPatterns = new()
    {
        { "Visa", new Regex(@"^4[0-9]{12}(?:[0-9]{3})?$") },
        { "MasterCard", new Regex(@"^5[1-5][0-9]{14}$") },
        { "American Express", new Regex(@"^3[47][0-9]{13}$") },
        { "Discover", new Regex(@"^6(?:011|5[0-9]{2})[0-9]{12}$") }
    };

    // Patterns to detect credit card data in text
    private static readonly Regex[] CreditCardDetectionPatterns = {
        new(@"\b4[0-9]{12}(?:[0-9]{3})?\b"), // Visa
        new(@"\b5[1-5][0-9]{14}\b"), // MasterCard
        new(@"\b3[47][0-9]{13}\b"), // American Express
        new(@"\b6(?:011|5[0-9]{2})[0-9]{12}\b"), // Discover
        new(@"\b[0-9]{4}[\s\-]?[0-9]{4}[\s\-]?[0-9]{4}[\s\-]?[0-9]{4}\b"), // Generic card pattern
        new(@"\b[0-9]{3,4}\b.*\b[0-9]{2}/[0-9]{2}\b"), // CVV and expiry pattern
    };

    public PciComplianceService(
        IAuditLoggingService auditLoggingService,
        IEncryptionService encryptionService,
        ILogger<PciComplianceService> logger)
    {
        _auditLoggingService = auditLoggingService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public string MaskCreditCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return string.Empty;

        // Remove any non-digit characters
        var digitsOnly = Regex.Replace(cardNumber, @"[^\d]", "");

        if (digitsOnly.Length < 8)
            return "****";

        // Show first 4 and last 4 digits, mask the rest
        var firstFour = digitsOnly[..4];
        var lastFour = digitsOnly[^4..];
        var maskedMiddle = new string('*', digitsOnly.Length - 8);

        return $"{firstFour}{maskedMiddle}{lastFour}";
    }

    public bool IsValidCreditCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return false;

        // Remove any non-digit characters
        var digitsOnly = Regex.Replace(cardNumber, @"[^\d]", "");

        // Check if it matches any known card pattern
        var isValidPattern = CardPatterns.Values.Any(pattern => pattern.IsMatch(digitsOnly));

        if (!isValidPattern)
            return false;

        // Validate using Luhn algorithm
        return IsValidLuhn(digitsOnly);
    }

    public string TokenizeCardData(string cardNumber, string expiryDate, string cvv)
    {
        try
        {
            // In a real PCI DSS environment, this would integrate with a payment processor's tokenization service
            // For this implementation, we'll create a secure token that doesn't contain actual card data
            
            var cardData = new
            {
                CardNumber = MaskCreditCardNumber(cardNumber),
                ExpiryDate = expiryDate,
                Timestamp = DateTime.UtcNow
            };

            var token = _encryptionService.GenerateSecureToken(32);
            
            // Log the tokenization event
            _ = Task.Run(async () => await _auditLoggingService.LogPaymentActionAsync(
                "CARD_TOKENIZATION", 
                null, 
                token, 
                null, 
                null, 
                $"Card ending in {cardNumber[^4..]} tokenized"));

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tokenize card data");
            throw new InvalidOperationException("Card tokenization failed", ex);
        }
    }

    public bool ValidateCardDataFormat(string cardNumber, string expiryDate, string cvv)
    {
        // Validate card number
        if (!IsValidCreditCardNumber(cardNumber))
            return false;

        // Validate expiry date format (MM/YY or MM/YYYY)
        if (!Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])\/([0-9]{2}|[0-9]{4})$"))
            return false;

        // Validate CVV format (3 or 4 digits)
        if (!Regex.IsMatch(cvv, @"^[0-9]{3,4}$"))
            return false;

        // Check if expiry date is not in the past
        if (!IsValidExpiryDate(expiryDate))
            return false;

        return true;
    }

    public async Task LogPaymentDataAccessAsync(string action, string? userId, string? orderId = null)
    {
        await _auditLoggingService.LogPaymentActionAsync(
            action,
            orderId,
            null,
            userId,
            null,
            "Payment data access logged for PCI DSS compliance");
    }

    public string SanitizePaymentData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        var sanitized = data;

        // Replace potential credit card numbers with masked versions
        foreach (var pattern in CreditCardDetectionPatterns)
        {
            sanitized = pattern.Replace(sanitized, match =>
            {
                var cardNumber = Regex.Replace(match.Value, @"[^\d]", "");
                if (cardNumber.Length >= 8)
                {
                    return MaskCreditCardNumber(cardNumber);
                }
                return match.Value;
            });
        }

        // Remove potential CVV codes (3-4 digit numbers that might be CVVs)
        sanitized = Regex.Replace(sanitized, @"\b[0-9]{3,4}\b", "***");

        return sanitized;
    }

    public bool IsPaymentDataPresent(string data)
    {
        if (string.IsNullOrEmpty(data))
            return false;

        // Check for credit card patterns
        foreach (var pattern in CreditCardDetectionPatterns)
        {
            if (pattern.IsMatch(data))
            {
                _logger.LogWarning("Potential payment data detected in content");
                return true;
            }
        }

        // Check for common payment-related keywords
        var paymentKeywords = new[] { "cvv", "cvc", "card number", "credit card", "debit card", "expiry", "expiration" };
        var lowerData = data.ToLowerInvariant();
        
        if (paymentKeywords.Any(keyword => lowerData.Contains(keyword)))
        {
            _logger.LogWarning("Payment-related keywords detected in content");
            return true;
        }

        return false;
    }

    private bool IsValidLuhn(string cardNumber)
    {
        var sum = 0;
        var alternate = false;

        for (var i = cardNumber.Length - 1; i >= 0; i--)
        {
            var digit = int.Parse(cardNumber[i].ToString());

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit = (digit % 10) + 1;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    private bool IsValidExpiryDate(string expiryDate)
    {
        try
        {
            var parts = expiryDate.Split('/');
            var month = int.Parse(parts[0]);
            var year = int.Parse(parts[1]);

            // Convert 2-digit year to 4-digit year
            if (year < 100)
                year += 2000;

            var expiryDateTime = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return expiryDateTime >= DateTime.Now.Date;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Middleware to detect and prevent payment data leakage
/// </summary>
public class PciDataLeakagePreventionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPciComplianceService _pciComplianceService;
    private readonly ILogger<PciDataLeakagePreventionMiddleware> _logger;

    public PciDataLeakagePreventionMiddleware(
        RequestDelegate next,
        IPciComplianceService pciComplianceService,
        ILogger<PciDataLeakagePreventionMiddleware> logger)
    {
        _next = next;
        _pciComplianceService = pciComplianceService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Check response for payment data leakage
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(responseBody).ReadToEndAsync();

        if (_pciComplianceService.IsPaymentDataPresent(responseContent))
        {
            _logger.LogError("Payment data detected in response - potential PCI DSS violation");
            
            // Sanitize the response
            var sanitizedContent = _pciComplianceService.SanitizePaymentData(responseContent);
            var sanitizedBytes = System.Text.Encoding.UTF8.GetBytes(sanitizedContent);
            
            context.Response.ContentLength = sanitizedBytes.Length;
            await originalBodyStream.WriteAsync(sanitizedBytes);
        }
        else
        {
            // Copy the response back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}