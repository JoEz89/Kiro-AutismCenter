using AutismCenter.Application.Features.Users.Common;
using MediatR;

namespace AutismCenter.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<UserDto?>;