using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentByKey;

public record GetContentByKeyQuery(
    string Key
) : IRequest<GetContentByKeyResponse?>;