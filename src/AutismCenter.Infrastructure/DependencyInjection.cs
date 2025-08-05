using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Amazon.S3;
using StackExchange.Redis;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;
using AutismCenter.Infrastructure.Repositories;
using AutismCenter.Infrastructure.Services;

namespace AutismCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database with connection pooling
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                
                // Enable connection pooling
                npgsqlOptions.CommandTimeout(30);
            });
            
            // Enable sensitive data logging in development only
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            // Enable query splitting for better performance
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Memory Cache
        services.AddMemoryCache();

        // Redis Cache
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "AutismCenter";
            });
            
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(redisConnectionString));
            
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            // Fallback to in-memory cache if Redis is not configured
            services.AddScoped<ICacheService, InMemoryCacheService>();
        }

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IVideoStreamingSessionRepository, VideoStreamingSessionRepository>();
        services.AddScoped<IVideoAccessLogRepository, VideoAccessLogRepository>();
        services.AddScoped<ILocalizedContentRepository, LocalizedContentRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // Authentication Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Payment Services
        services.Configure<StripeSettings>(options =>
        {
            configuration.GetSection(StripeSettings.SectionName).Bind(options);
        });
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();

        // AWS S3 Service
        services.AddAWSService<IAmazonS3>();

        // Video Streaming Services
        services.AddScoped<IVideoAccessService, VideoAccessService>();
        services.AddScoped<IVideoStreamingService, AwsS3VideoStreamingService>();

        // Certificate Services
        services.AddScoped<ICertificateService, CertificateService>();

        // Localization Services
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IContentFormattingService, ContentFormattingService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        // User Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}