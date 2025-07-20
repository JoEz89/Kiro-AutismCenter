namespace AutismCenter.Application.Common.Models;

public record TokenValidationResult(
    bool IsValid,
    Guid? UserId = null,
    string? Email = null,
    string? Role = null,
    string? ErrorMessage = null
);