namespace AutismCenter.Application.Features.ContentManagement.Commands.DeactivateLocalizedContent;

public record DeactivateLocalizedContentResponse(
    Guid Id,
    string Key,
    bool IsActive,
    string Message
);