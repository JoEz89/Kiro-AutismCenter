namespace AutismCenter.Application.Features.ContentManagement.Commands.ActivateLocalizedContent;

public record ActivateLocalizedContentResponse(
    Guid Id,
    string Key,
    bool IsActive,
    string Message
);