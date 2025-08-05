using AutismCenter.WebApi.Middleware;
using AutismCenter.WebApi.Services;
using Microsoft.AspNetCore.DataProtection;

namespace AutismCenter.WebApi.Configuration;

public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Data Protection with enhanced security
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("./keys"))
            .SetApplicationName("AutismCenter")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
            .UseCryptographicAlgorithms(new Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ValidationAlgorithm.HMACSHA256
            });

        // Add security services
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAuditLoggingService, AuditLoggingService>();
        services.AddScoped<IPciComplianceService, PciComplianceService>();
        services.AddScoped<IDataBackupService, DataBackupService>();

        // Add background services
        services.AddHostedService<Services.BackgroundServices.BackupBackgroundService>();

        // Configure Rate Limiting
        services.AddSingleton<RateLimitOptions>(provider =>
        {
            var options = new RateLimitOptions();
            configuration.GetSection("RateLimit").Bind(options);
            return options;
        });

        // Configure DDoS Protection
        services.AddSingleton<DDoSProtectionOptions>(provider =>
        {
            var options = new DDoSProtectionOptions();
            configuration.GetSection("DDoSProtection").Bind(options);
            return options;
        });

        // Configure Security Headers
        services.AddSingleton<SecurityHeadersOptions>(provider =>
        {
            var options = new SecurityHeadersOptions();
            configuration.GetSection("SecurityHeaders").Bind(options);
            return options;
        });

        // Configure CORS with strict settings
        services.AddCors(options =>
        {
            options.AddPolicy("SecureCorsPolicy", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        // Configure HTTPS redirection
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = 443;
        });

        // Configure HSTS
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        return services;
    }

    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Security headers should be first
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // DDoS protection
        app.UseMiddleware<DDoSProtectionMiddleware>();

        // Rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        // Input validation and sanitization
        app.UseMiddleware<InputValidationMiddleware>();

        // PCI DSS data leakage prevention
        app.UseMiddleware<PciDataLeakagePreventionMiddleware>();

        // HTTPS redirection (only in production)
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }

        return app;
    }
}