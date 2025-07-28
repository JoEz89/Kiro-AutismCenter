using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContent;

public record GetLocalizedContentQuery(
    Guid Id
) : IRequest<GetLocalizedContentResponse?>;