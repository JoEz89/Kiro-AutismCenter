namespace AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;

public record BulkUpdateLocalizedContentResult(
    string Key,
    string Language,
    bool Success,
    string? ErrorMessage = null
);

public record BulkUpdateLocalizedContentResponse(
    int TotalItems,
    int SuccessfulUpdates,
    int FailedUpdates,
    IEnumerable<BulkUpdateLocalizedContentResult> Results
);