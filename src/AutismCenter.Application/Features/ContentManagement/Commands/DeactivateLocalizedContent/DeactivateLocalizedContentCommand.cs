using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeactivateLocalizedContent;

public record DeactivateLocalizedContentCommand(
    Guid Id
) : IRequest<DeactivateLocalizedContentResponse>;