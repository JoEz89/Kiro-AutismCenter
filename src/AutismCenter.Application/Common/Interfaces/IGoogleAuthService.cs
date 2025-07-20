using AutismCenter.Application.Common.Models;

namespace AutismCenter.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string googleToken);
}