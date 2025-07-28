using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;

public record UpdateLocalizedContentCommand(
    Guid Id,
    string Content,
    string? Description = null
) : IRequest<UpdateLocalizedContentResponse>;