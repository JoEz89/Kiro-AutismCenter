using Ganss.Xss;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AutismCenter.WebApi.Middleware;

public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;
    private readonly HtmlSanitizer _htmlSanitizer;

    // Common SQL injection patterns
    private static readonly string[] SqlInjectionPatterns = {
        @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)",
        @"(\b(AND|OR)\b.{1,6}?(=|>|<|\!=|<>|<=|>=))",
        @"(\b(AND|OR)\b.{1,6}?\b(IS( +NOT)?( +NULL)?|(NOT( +)?)?IN|EXISTS|(NOT( +)?)?BETWEEN|LIKE|REGEXP|RLIKE|SOUNDS( +LIKE)?)\b)",
        @"(\bCAST\s*\()",
        @"(\bCONVERT\s*\()",
        @"(\bCHAR\s*\()",
        @"(\bCONCAT\s*\()",
        @"(\bSUBSTRING\s*\()",
        @"(\bUNION\s+(ALL\s+)?SELECT)",
        @"(\b(GRANT|REVOKE)\b)",
        @"(\bINSERT\s+INTO\b)",
        @"(\bUPDATE\s+.+\s+SET\b)",
        @"(\bDELETE\s+FROM\b)",
        @"(\bDROP\s+(TABLE|DATABASE|INDEX|VIEW|PROCEDURE|FUNCTION)\b)",
        @"(\bCREATE\s+(TABLE|DATABASE|INDEX|VIEW|PROCEDURE|FUNCTION)\b)",
        @"(\bALTER\s+(TABLE|DATABASE|INDEX|VIEW|PROCEDURE|FUNCTION)\b)",
        @"(\bTRUNCATE\s+TABLE\b)",
        @"(\bSHUTDOWN\b)",
        @"(\bxp_cmdshell\b)",
        @"(\bsp_executesql\b)"
    };

    // XSS patterns
    private static readonly string[] XssPatterns = {
        @"<script[^>]*>.*?</script>",
        @"javascript:",
        @"vbscript:",
        @"onload\s*=",
        @"onerror\s*=",
        @"onclick\s*=",
        @"onmouseover\s*=",
        @"onfocus\s*=",
        @"onblur\s*=",
        @"onchange\s*=",
        @"onsubmit\s*=",
        @"<iframe[^>]*>.*?</iframe>",
        @"<object[^>]*>.*?</object>",
        @"<embed[^>]*>.*?</embed>",
        @"<link[^>]*>",
        @"<meta[^>]*>",
        @"<style[^>]*>.*?</style>",
        @"expression\s*\(",
        @"url\s*\(",
        @"@import",
        @"<img[^>]*src\s*=\s*[""']?javascript:",
        @"<img[^>]*src\s*=\s*[""']?vbscript:",
        @"<img[^>]*src\s*=\s*[""']?data:"
    };

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        
        // Configure HTML sanitizer
        _htmlSanitizer = new HtmlSanitizer();
        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedCssProperties.Clear();
        _htmlSanitizer.AllowedSchemes.Clear();
        _htmlSanitizer.AllowedSchemes.Add("http");
        _htmlSanitizer.AllowedSchemes.Add("https");
        _htmlSanitizer.AllowedSchemes.Add("mailto");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for certain endpoints (like health checks)
        if (ShouldSkipValidation(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Validate and sanitize request
        if (!await ValidateAndSanitizeRequest(context))
        {
            return; // Response already written
        }

        await _next(context);
    }

    private static bool ShouldSkipValidation(PathString path)
    {
        var skipPaths = new[] { "/health", "/swagger", "/api/health" };
        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> ValidateAndSanitizeRequest(HttpContext context)
    {
        try
        {
            // Validate headers
            if (!ValidateHeaders(context.Request.Headers))
            {
                await WriteSecurityViolationResponse(context, "Invalid headers detected");
                return false;
            }

            // Validate query parameters
            if (!ValidateQueryParameters(context.Request.Query))
            {
                await WriteSecurityViolationResponse(context, "Invalid query parameters detected");
                return false;
            }

            // Validate and sanitize request body for POST/PUT requests
            if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
            {
                if (!await ValidateAndSanitizeRequestBody(context))
                {
                    await WriteSecurityViolationResponse(context, "Invalid request body detected");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during input validation");
            await WriteSecurityViolationResponse(context, "Request validation failed");
            return false;
        }
    }

    private bool ValidateHeaders(IHeaderDictionary headers)
    {
        foreach (var header in headers)
        {
            if (ContainsSqlInjection(header.Value.ToString()) || ContainsXss(header.Value.ToString()))
            {
                _logger.LogWarning("Malicious content detected in header: {HeaderName}", header.Key);
                return false;
            }
        }
        return true;
    }

    private bool ValidateQueryParameters(IQueryCollection query)
    {
        foreach (var param in query)
        {
            var value = param.Value.ToString();
            if (ContainsSqlInjection(value) || ContainsXss(value))
            {
                _logger.LogWarning("Malicious content detected in query parameter: {ParamName}", param.Key);
                return false;
            }
        }
        return true;
    }

    private async Task<bool> ValidateAndSanitizeRequestBody(HttpContext context)
    {
        if (context.Request.ContentLength == 0)
            return true;

        // Enable buffering to allow multiple reads
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrEmpty(body))
            return true;

        // Check for malicious content
        if (ContainsSqlInjection(body) || ContainsXss(body))
        {
            _logger.LogWarning("Malicious content detected in request body");
            return false;
        }

        // For JSON content, sanitize string values
        if (context.Request.ContentType?.Contains("application/json") == true)
        {
            try
            {
                var sanitizedBody = SanitizeJsonContent(body);
                if (sanitizedBody != body)
                {
                    // Replace the request body with sanitized content
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    context.Request.Body = new MemoryStream(sanitizedBytes);
                    context.Request.ContentLength = sanitizedBytes.Length;
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Invalid JSON in request body");
                return false;
            }
        }

        return true;
    }

    private string SanitizeJsonContent(string jsonContent)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var sanitizedJson = SanitizeJsonElement(document.RootElement);
            return JsonSerializer.Serialize(sanitizedJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
        catch
        {
            return jsonContent; // Return original if parsing fails
        }
    }

    private object? SanitizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => SanitizeJsonElement(prop.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(SanitizeJsonElement).ToArray(),
            JsonValueKind.String => _htmlSanitizer.Sanitize(element.GetString() ?? string.Empty),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return SqlInjectionPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private bool ContainsXss(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return XssPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private async Task WriteSecurityViolationResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "Security violation detected",
            code = "SECURITY_VIOLATION",
            details = message,
            timestamp = DateTime.UtcNow,
            traceId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}