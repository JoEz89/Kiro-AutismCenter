namespace AutismCenter.WebApi.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing the request
        AddSecurityHeaders(context);

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var response = context.Response;
        var headers = response.Headers;

        // Remove server information
        if (headers.ContainsKey("Server"))
        {
            headers.Remove("Server");
        }

        // X-Content-Type-Options: Prevent MIME type sniffing
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Add("X-Content-Type-Options", "nosniff");
        }

        // X-Frame-Options: Prevent clickjacking
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Add("X-Frame-Options", _options.XFrameOptions);
        }

        // X-XSS-Protection: Enable XSS filtering
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Add("X-XSS-Protection", "1; mode=block");
        }

        // Referrer-Policy: Control referrer information
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Add("Referrer-Policy", _options.ReferrerPolicy);
        }

        // Content-Security-Policy: Prevent XSS and data injection attacks
        if (!headers.ContainsKey("Content-Security-Policy") && !string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            headers.Add("Content-Security-Policy", _options.ContentSecurityPolicy);
        }

        // Strict-Transport-Security: Enforce HTTPS
        if (context.Request.IsHttps && !headers.ContainsKey("Strict-Transport-Security"))
        {
            headers.Add("Strict-Transport-Security", _options.StrictTransportSecurity);
        }

        // Permissions-Policy: Control browser features
        if (!headers.ContainsKey("Permissions-Policy") && !string.IsNullOrEmpty(_options.PermissionsPolicy))
        {
            headers.Add("Permissions-Policy", _options.PermissionsPolicy);
        }

        // X-Permitted-Cross-Domain-Policies: Control cross-domain policies
        if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
        {
            headers.Add("X-Permitted-Cross-Domain-Policies", "none");
        }

        // Cache-Control for sensitive endpoints
        if (IsSensitiveEndpoint(context.Request.Path))
        {
            if (!headers.ContainsKey("Cache-Control"))
            {
                headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            }
            if (!headers.ContainsKey("Pragma"))
            {
                headers.Add("Pragma", "no-cache");
            }
            if (!headers.ContainsKey("Expires"))
            {
                headers.Add("Expires", "0");
            }
        }

        // Custom security headers
        foreach (var customHeader in _options.CustomHeaders)
        {
            if (!headers.ContainsKey(customHeader.Key))
            {
                headers.Add(customHeader.Key, customHeader.Value);
            }
        }
    }

    private static bool IsSensitiveEndpoint(PathString path)
    {
        var sensitivePaths = new[]
        {
            "/api/auth",
            "/api/payment",
            "/api/admin",
            "/api/users",
            "/api/orders"
        };

        return sensitivePaths.Any(sensitivePath => 
            path.StartsWithSegments(sensitivePath, StringComparison.OrdinalIgnoreCase));
    }
}

public class SecurityHeadersOptions
{
    public string XFrameOptions { get; set; } = "DENY";
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
    public string StrictTransportSecurity { get; set; } = "max-age=31536000; includeSubDomains; preload";
    
    public string ContentSecurityPolicy { get; set; } = 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://apis.google.com https://www.gstatic.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://api.stripe.com https://zoom.us; " +
        "frame-src 'self' https://accounts.google.com https://zoom.us; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'; " +
        "upgrade-insecure-requests;";
    
    public string PermissionsPolicy { get; set; } = 
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=()";

    public Dictionary<string, string> CustomHeaders { get; set; } = new();
}