namespace AutismCenter.Application.Common.Models;

public record GoogleUserInfo(
    string GoogleId,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified
);