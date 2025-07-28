using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;

public record CreateLocalizedContentCommand(
    string Key,
    Language Language,
    string Content,
    string Category,
    string? Description = null
) : IRequest<CreateLocalizedContentResponse>;