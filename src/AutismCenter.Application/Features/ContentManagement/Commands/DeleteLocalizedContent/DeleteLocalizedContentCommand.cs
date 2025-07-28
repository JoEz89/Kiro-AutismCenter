using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeleteLocalizedContent;

public record DeleteLocalizedContentCommand(
    Guid Id
) : IRequest<DeleteLocalizedContentResponse>;