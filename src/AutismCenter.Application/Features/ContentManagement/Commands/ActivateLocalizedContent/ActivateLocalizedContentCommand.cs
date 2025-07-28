using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.ActivateLocalizedContent;

public record ActivateLocalizedContentCommand(
    Guid Id
) : IRequest<ActivateLocalizedContentResponse>;