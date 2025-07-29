using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace AutismCenter.Application.Features.Authentication.Commands.GoogleLogin;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthenticationResult>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public GoogleLoginHandler(
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthenticationResult> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GoogleToken))
        {
            throw new ArgumentException("Google token is required");
        }

        // Validate Google token
        var googleUserInfo = await _googleAuthService.ValidateGoogleTokenAsync(request.GoogleToken);
        if (googleUserInfo == null)
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }

        // Check if user exists by email
        var email = Email.Create(googleUserInfo.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email);

        User user;

        if (existingUser != null)
        {
            // User exists - link Google account if not already linked
            if (string.IsNullOrEmpty(existingUser.GoogleId))
            {
                // Link Google account to existing user
                existingUser.SetGoogleId(googleUserInfo.GoogleId);
                
                // Verify email if Google account is verified
                if (googleUserInfo.EmailVerified && !existingUser.IsEmailVerified)
                {
                    existingUser.VerifyEmail();
                }

                await _userRepository.UpdateAsync(existingUser);
            }
            else if (existingUser.GoogleId != googleUserInfo.GoogleId)
            {
                throw new InvalidOperationException("This email is associated with a different Google account");
            }

            user = existingUser;
        }
        else
        {
            // Create new user with Google account
            user = User.CreateWithGoogle(
                email,
                googleUserInfo.FirstName,
                googleUserInfo.LastName,
                googleUserInfo.GoogleId,
                UserRole.Patient,
                Language.English
            );

            await _userRepository.AddAsync(user);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        // Store refresh token
        var refreshTokenEntity = Domain.Entities.RefreshToken.Create(refreshToken, user.Id, refreshTokenExpiration);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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