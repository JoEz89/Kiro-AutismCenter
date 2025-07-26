using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;

public record GetSecureVideoUrlQuery(
    Guid UserId,
    Guid ModuleId,
    int ExpirationMinutes = 60
) : IRequest<GetSecureVideoUrlResponse>;