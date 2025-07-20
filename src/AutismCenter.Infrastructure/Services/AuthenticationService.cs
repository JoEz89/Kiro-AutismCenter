using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace AutismCenter.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password are required");

        var emailValue = Email.Create(email);
        var user = await _userRepository.GetByEmailAsync(emailValue);

        if (user == null || !user.CanLogin())
            throw new UnauthorizedAccessException("Invalid credentials or account not verified");

        if (user.PasswordHash == null || !_passwordService.VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return await GenerateAuthenticationResultAsync(user);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = storedToken.User;
        if (!user.CanLogin())
            throw new UnauthorizedAccessException("User account is not active");

        // Revoke the used refresh token
        storedToken.Revoke("Token refreshed");
        await _refreshTokenRepository.UpdateAsync(storedToken);

        // Generate new tokens
        var result = await GenerateAuthenticationResultAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.Revoke("Manual revocation");
            await _refreshTokenRepository.UpdateAsync(storedToken);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        return storedToken?.IsActive == true;
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        // Store refresh token
        var refreshTokenEntity = RefreshToken.Create(refreshToken, user.Id, refreshTokenExpiration);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthenticationResult(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            accessToken,
            refreshToken,
            accessTokenExpiration,
            refreshTokenExpiration
        );
    }
}