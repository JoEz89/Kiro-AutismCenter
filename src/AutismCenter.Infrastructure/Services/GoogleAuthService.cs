using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string googleToken)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google Client ID is not configured");
                return null;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            if (payload == null)
            {
                _logger.LogWarning("Google token validation failed - payload is null");
                return null;
            }

            // Extract name parts
            var nameParts = payload.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var firstName = nameParts.Length > 0 ? nameParts[0] : payload.GivenName ?? "Unknown";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : payload.FamilyName ?? "User";

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                firstName,
                lastName,
                payload.EmailVerified
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }
}