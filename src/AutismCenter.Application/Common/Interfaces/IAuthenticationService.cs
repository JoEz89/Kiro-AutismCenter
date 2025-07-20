using AutismCenter.Application.Common.Models;

namespace AutismCenter.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string email, string password);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
}