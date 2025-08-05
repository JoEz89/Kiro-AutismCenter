using System.Security.Claims;
using System.Text.Json;

namespace AutismCenter.WebApi.Services;

public interface IAuditLoggingService
{
    Task LogUserActionAsync(string action, string? userId, string? details = null, string? ipAddress = null);
    Task LogPaymentActionAsync(string action, string? orderId, string? paymentId, string? userId, decimal? amount = null, string? details = null);
    Task LogDataAccessAsync(string dataType, string action, string? userId, string? recordId = null, string? details = null);
    Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null, string? userAgent = null);
    Task LogSystemEventAsync(string eventType, string description, string? details = null);
    Task LogAuthenticationEventAsync(string eventType, string? userId, string? email, bool success, string? ipAddress = null, string? failureReason = null);
}

public class AuditLoggingService : IAuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLoggingService(ILogger<AuditLoggingService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogUserActionAsync(string action, string? userId, string? details = null, string? ipAddress = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "USER_ACTION",
            Action = action,
            UserId = userId,
            Details = details,
            IpAddress = ipAddress ?? GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId()
        };

        await LogAuditEntryAsync(auditLog);
    }

    public async Task LogPaymentActionAsync(string action, string? orderId, string? paymentId, string? userId, decimal? amount = null, string? details = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "PAYMENT_ACTION",
            Action = action,
            UserId = userId,
            OrderId = orderId,
            PaymentId = paymentId,
            Amount = amount,
            Details = details,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            Severity = "HIGH" // Payment actions are always high severity for PCI DSS
        };

        await LogAuditEntryAsync(auditLog);
    }

    public async Task LogDataAccessAsync(string dataType, string action, string? userId, string? recordId = null, string? details = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "DATA_ACCESS",
            Action = $"{action}_{dataType}",
            UserId = userId,
            RecordId = recordId,
            DataType = dataType,
            Details = details,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            Severity = GetDataAccessSeverity(dataType, action)
        };

        await LogAuditEntryAsync(auditLog);
    }

    public async Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "SECURITY_EVENT",
            Action = eventType,
            UserId = userId,
            Description = description,
            IpAddress = ipAddress ?? GetClientIpAddress(),
            UserAgent = userAgent ?? GetUserAgent(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            Severity = "HIGH" // Security events are always high severity
        };

        await LogAuditEntryAsync(auditLog);
    }

    public async Task LogSystemEventAsync(string eventType, string description, string? details = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "SYSTEM_EVENT",
            Action = eventType,
            Description = description,
            Details = details,
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            Severity = "MEDIUM"
        };

        await LogAuditEntryAsync(auditLog);
    }

    public async Task LogAuthenticationEventAsync(string eventType, string? userId, string? email, bool success, string? ipAddress = null, string? failureReason = null)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "AUTHENTICATION",
            Action = eventType,
            UserId = userId,
            Email = email,
            Success = success,
            FailureReason = failureReason,
            IpAddress = ipAddress ?? GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            Severity = success ? "MEDIUM" : "HIGH"
        };

        await LogAuditEntryAsync(auditLog);
    }

    private async Task LogAuditEntryAsync(AuditLogEntry auditLog)
    {
        try
        {
            // Serialize the audit log entry
            var logJson = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // Log with appropriate level based on severity
            var logLevel = auditLog.Severity switch
            {
                "HIGH" => LogLevel.Warning,
                "CRITICAL" => LogLevel.Critical,
                "MEDIUM" => LogLevel.Information,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel, "AUDIT: {AuditLog}", logJson);

            // In a production environment, you might also want to:
            // 1. Store in a separate audit database
            // 2. Send to a SIEM system
            // 3. Write to a secure audit file
            // 4. Send to external audit service

            await Task.CompletedTask; // Placeholder for async operations
        }
        catch (Exception ex)
        {
            // Never let audit logging failures break the application
            _logger.LogError(ex, "Failed to write audit log entry");
        }
    }

    private string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        // Check for forwarded IP first
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP))
        {
            return realIP;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers.UserAgent.FirstOrDefault();
    }

    private string? GetTraceId()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.TraceIdentifier;
    }

    private string GetDataAccessSeverity(string dataType, string action)
    {
        // Sensitive data types require higher severity logging
        var sensitiveDataTypes = new[] { "PAYMENT", "PERSONAL_INFO", "MEDICAL_INFO", "FINANCIAL" };
        var highRiskActions = new[] { "DELETE", "EXPORT", "BULK_ACCESS" };

        if (sensitiveDataTypes.Contains(dataType.ToUpperInvariant()) || 
            highRiskActions.Contains(action.ToUpperInvariant()))
        {
            return "HIGH";
        }

        return "MEDIUM";
    }
}

public class AuditLogEntry
{
    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? OrderId { get; set; }
    public string? PaymentId { get; set; }
    public string? RecordId { get; set; }
    public string? DataType { get; set; }
    public decimal? Amount { get; set; }
    public bool? Success { get; set; }
    public string? FailureReason { get; set; }
    public string? Description { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "MEDIUM";
}