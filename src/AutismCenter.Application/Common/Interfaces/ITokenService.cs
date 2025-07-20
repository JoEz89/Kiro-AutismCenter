using AutismCenter.Application.Common.Models;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    TokenValidationResult ValidateToken(string token);
    DateTime GetTokenExpiration(string token);
}