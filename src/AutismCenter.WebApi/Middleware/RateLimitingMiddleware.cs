using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace AutismCenter.WebApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    
    // Store request counts per IP
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
    
    // Cleanup timer
    private static readonly Timer _cleanupTimer = new(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);
        
        if (ShouldSkipRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var rateLimitRule = GetRateLimitRule(endpoint);
        
        if (!IsRequestAllowed(clientId, endpoint, rateLimitRule))
        {
            await HandleRateLimitExceeded(context, rateLimitRule);
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims first (for authenticated requests)
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return $"ip:{forwardedFor.Split(',')[0].Trim()}";
        }

        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();
        
        // Group similar endpoints
        if (path.StartsWith("/api/auth/"))
            return "auth";
        if (path.StartsWith("/api/products"))
            return "products";
        if (path.StartsWith("/api/orders"))
            return "orders";
        if (path.StartsWith("/api/appointments"))
            return "appointments";
        if (path.StartsWith("/api/courses"))
            return "courses";
        if (path.StartsWith("/api/cart"))
            return "cart";
        if (path.StartsWith("/api/payment"))
            return "payment";
        if (path.StartsWith("/api/admin"))
            return "admin";
        
        return "general";
    }

    private RateLimitRule GetRateLimitRule(string endpoint)
    {
        return endpoint switch
        {
            "auth" => _options.AuthEndpoints,
            "payment" => _options.PaymentEndpoints,
            "admin" => _options.AdminEndpoints,
            "products" => _options.ProductEndpoints,
            "orders" => _options.OrderEndpoints,
            "appointments" => _options.AppointmentEndpoints,
            "courses" => _options.CourseEndpoints,
            "cart" => _options.CartEndpoints,
            _ => _options.GeneralEndpoints
        };
    }

    private bool ShouldSkipRateLimit(PathString path)
    {
        var skipPaths = new[] { "/health", "/swagger", "/api/health" };
        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsRequestAllowed(string clientId, string endpoint, RateLimitRule rule)
    {
        var now = DateTime.UtcNow;
        var key = $"{clientId}:{endpoint}";
        
        var clientInfo = _clients.AddOrUpdate(key, 
            new ClientRequestInfo { RequestCount = 1, WindowStart = now },
            (_, existing) =>
            {
                // Reset window if expired
                if (now - existing.WindowStart > rule.Window)
                {
                    existing.RequestCount = 1;
                    existing.WindowStart = now;
                }
                else
                {
                    existing.RequestCount++;
                }
                return existing;
            });

        var isAllowed = clientInfo.RequestCount <= rule.MaxRequests;
        
        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Count: {Count}, Limit: {Limit}", 
                clientId, endpoint, clientInfo.RequestCount, rule.MaxRequests);
        }

        return isAllowed;
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitRule rule)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        
        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", rule.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Window", rule.Window.TotalSeconds.ToString());
        context.Response.Headers.Add("Retry-After", rule.Window.TotalSeconds.ToString());

        var response = new
        {
            message = "Rate limit exceeded",
            code = "RATE_LIMIT_EXCEEDED",
            details = $"Maximum {rule.MaxRequests} requests per {rule.Window.TotalMinutes} minutes allowed",
            retryAfter = rule.Window.TotalSeconds,
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
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var expiredKeys = _clients
            .Where(kvp => kvp.Value.WindowStart < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _clients.TryRemove(key, out _);
        }
    }
}

public class ClientRequestInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
}

public class RateLimitRule
{
    public int MaxRequests { get; set; }
    public TimeSpan Window { get; set; }
}

public class RateLimitOptions
{
    public RateLimitRule GeneralEndpoints { get; set; } = new() { MaxRequests = 100, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule AuthEndpoints { get; set; } = new() { MaxRequests = 5, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule PaymentEndpoints { get; set; } = new() { MaxRequests = 10, Window = TimeSpan.FromMinutes(5) };
    public RateLimitRule AdminEndpoints { get; set; } = new() { MaxRequests = 50, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule ProductEndpoints { get; set; } = new() { MaxRequests = 200, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule OrderEndpoints { get; set; } = new() { MaxRequests = 20, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule AppointmentEndpoints { get; set; } = new() { MaxRequests = 30, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule CourseEndpoints { get; set; } = new() { MaxRequests = 100, Window = TimeSpan.FromMinutes(1) };
    public RateLimitRule CartEndpoints { get; set; } = new() { MaxRequests = 50, Window = TimeSpan.FromMinutes(1) };
}