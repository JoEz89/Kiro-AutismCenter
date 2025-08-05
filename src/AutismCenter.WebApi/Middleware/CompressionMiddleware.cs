using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace AutismCenter.WebApi.Middleware;

public static class CompressionMiddlewareExtensions
{
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            
            // MIME types to compress
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/javascript",
                "text/css",
                "text/html",
                "text/json",
                "text/plain",
                "text/xml",
                "application/xml",
                "image/svg+xml",
                "application/font-woff",
                "application/font-woff2"
            });
        });

        // Configure compression levels
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        return services;
    }
}