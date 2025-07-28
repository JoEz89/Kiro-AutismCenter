using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;

public record BulkUpdateLocalizedContentItem(
    string Key,
    Language Language,
    string Content,
    string? Description = null
);

public record BulkUpdateLocalizedContentCommand(
    string Category,
    IEnumerable<BulkUpdateLocalizedContentItem> Items
) : IRequest<BulkUpdateLocalizedContentResponse>;