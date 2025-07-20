using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using System.Security.Cryptography;

namespace AutismCenter.Infrastructure.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmailVerificationService(
        IEmailVerificationTokenRepository tokenRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
    }

    public string GenerateVerificationToken(Guid userId)
    {
        // Generate a secure random token
        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");

        // Create and store the token
        var verificationToken = EmailVerificationToken.Create(token, userId, 24); // 24 hours expiration
        _tokenRepository.AddAsync(verificationToken);

        return token;
    }

    public async Task<bool> ValidateVerificationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var storedToken = await _tokenRepository.GetByTokenAsync(token);
        return storedToken?.IsValid == true;
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var storedToken = await _tokenRepository.GetByTokenAsync(token);
        return storedToken?.IsValid == true ? storedToken.UserId : null;
    }

    public async Task RevokeTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var storedToken = await _tokenRepository.GetByTokenAsync(token);
        if (storedToken != null && storedToken.IsValid)
        {
            storedToken.MarkAsUsed();
            await _tokenRepository.UpdateAsync(storedToken);
        }
    }
}