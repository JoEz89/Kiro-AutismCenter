using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace AutismCenter.WebApi.Middleware;

public class DDoSProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DDoSProtectionMiddleware> _logger;
    private readonly DDoSProtectionOptions _options;
    
    // Track suspicious IPs
    private static readonly ConcurrentDictionary<string, SuspiciousActivity> _suspiciousIPs = new();
    private static readonly ConcurrentDictionary<string, DateTime> _blockedIPs = new();
    
    // Cleanup timer
    private static readonly Timer _cleanupTimer = new(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

    public DDoSProtectionMiddleware(RequestDelegate next, ILogger<DDoSProtectionMiddleware> logger, DDoSProtectionOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIP = GetClientIP(context);
        
        // Check if IP is blocked
        if (IsIPBlocked(clientIP))
        {
            await HandleBlockedIP(context, clientIP);
            return;
        }

        // Check for suspicious activity
        if (IsSuspiciousActivity(clientIP, context))
        {
            await HandleSuspiciousActivity(context, clientIP);
            return;
        }

        // Track request
        TrackRequest(clientIP, context);

        await _next(context);
    }

    private string GetClientIP(HttpContext context)
    {
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

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private bool IsIPBlocked(string clientIP)
    {
        if (!_blockedIPs.TryGetValue(clientIP, out var blockedUntil))
            return false;

        if (DateTime.UtcNow > blockedUntil)
        {
            _blockedIPs.TryRemove(clientIP, out _);
            _suspiciousIPs.TryRemove(clientIP, out _);
            return false;
        }

        return true;
    }

    private bool IsSuspiciousActivity(string clientIP, HttpContext context)
    {
        var now = DateTime.UtcNow;
        var activity = _suspiciousIPs.AddOrUpdate(clientIP,
            new SuspiciousActivity { RequestCount = 1, WindowStart = now, LastRequest = now },
            (_, existing) =>
            {
                // Reset window if expired
                if (now - existing.WindowStart > _options.DetectionWindow)
                {
                    existing.RequestCount = 1;
                    existing.WindowStart = now;
                    existing.SuspiciousPatterns = 0;
                }
                else
                {
                    existing.RequestCount++;
                }
                
                existing.LastRequest = now;
                return existing;
            });

        // Check for rapid requests
        if (activity.RequestCount > _options.MaxRequestsPerWindow)
        {
            _logger.LogWarning("DDoS: High request rate detected from IP {ClientIP}. Count: {Count} in {Window} seconds", 
                clientIP, activity.RequestCount, _options.DetectionWindow.TotalSeconds);
            return true;
        }

        // Check for suspicious patterns
        if (HasSuspiciousPatterns(context, activity))
        {
            _logger.LogWarning("DDoS: Suspicious patterns detected from IP {ClientIP}", clientIP);
            return true;
        }

        // Check for rapid consecutive requests (potential bot behavior)
        if (activity.RequestCount > 1)
        {
            var timeBetweenRequests = now - activity.LastRequest;
            if (timeBetweenRequests < _options.MinTimeBetweenRequests)
            {
                activity.SuspiciousPatterns++;
                if (activity.SuspiciousPatterns > _options.MaxSuspiciousPatterns)
                {
                    _logger.LogWarning("DDoS: Bot-like behavior detected from IP {ClientIP}", clientIP);
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasSuspiciousPatterns(HttpContext context, SuspiciousActivity activity)
    {
        var request = context.Request;
        
        // Check for missing or suspicious User-Agent
        var userAgent = request.Headers.UserAgent.ToString();
        if (string.IsNullOrEmpty(userAgent) || IsSuspiciousUserAgent(userAgent))
        {
            activity.SuspiciousPatterns++;
        }

        // Check for unusual request patterns
        if (request.Path.Value?.Contains("..") == true || 
            request.Path.Value?.Contains("//") == true)
        {
            activity.SuspiciousPatterns++;
        }

        // Check for excessive query parameters
        if (request.Query.Count > _options.MaxQueryParameters)
        {
            activity.SuspiciousPatterns++;
        }

        // Check for large request bodies on GET requests
        if (request.Method == "GET" && request.ContentLength > 0)
        {
            activity.SuspiciousPatterns++;
        }

        return activity.SuspiciousPatterns > _options.MaxSuspiciousPatterns;
    }

    private bool IsSuspiciousUserAgent(string userAgent)
    {
        var suspiciousAgents = new[]
        {
            "bot", "crawler", "spider", "scraper", "curl", "wget", "python", "java",
            "scanner", "test", "monitor", "check", "probe", "attack"
        };

        return suspiciousAgents.Any(agent => 
            userAgent.Contains(agent, StringComparison.OrdinalIgnoreCase));
    }

    private void TrackRequest(string clientIP, HttpContext context)
    {
        // This method can be extended to track additional metrics
        // For now, the tracking is handled in IsSuspiciousActivity
    }

    private async Task HandleBlockedIP(HttpContext context, string clientIP)
    {
        _logger.LogWarning("DDoS: Blocked request from IP {ClientIP}", clientIP);
        
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "Access temporarily blocked due to suspicious activity",
            code = "ACCESS_BLOCKED",
            timestamp = DateTime.UtcNow,
            traceId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task HandleSuspiciousActivity(HttpContext context, string clientIP)
    {
        // Block the IP
        var blockUntil = DateTime.UtcNow.Add(_options.BlockDuration);
        _blockedIPs.TryAdd(clientIP, blockUntil);

        _logger.LogWarning("DDoS: IP {ClientIP} blocked until {BlockUntil} due to suspicious activity", 
            clientIP, blockUntil);

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "Suspicious activity detected. Access temporarily blocked.",
            code = "SUSPICIOUS_ACTIVITY",
            blockedUntil = blockUntil,
            timestamp = DateTime.UtcNow,
            traceId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        
        // Clean up expired blocked IPs
        var expiredBlocks = _blockedIPs
            .Where(kvp => now > kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var ip in expiredBlocks)
        {
            _blockedIPs.TryRemove(ip, out _);
        }

        // Clean up old suspicious activity records
        var cutoff = now.AddHours(-1);
        var expiredActivity = _suspiciousIPs
            .Where(kvp => kvp.Value.WindowStart < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var ip in expiredActivity)
        {
            _suspiciousIPs.TryRemove(ip, out _);
        }
    }
}

public class SuspiciousActivity
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime LastRequest { get; set; }
    public int SuspiciousPatterns { get; set; }
}

public class DDoSProtectionOptions
{
    public int MaxRequestsPerWindow { get; set; } = 100;
    public TimeSpan DetectionWindow { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan BlockDuration { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan MinTimeBetweenRequests { get; set; } = TimeSpan.FromMilliseconds(100);
    public int MaxSuspiciousPatterns { get; set; } = 3;
    public int MaxQueryParameters { get; set; } = 50;
}