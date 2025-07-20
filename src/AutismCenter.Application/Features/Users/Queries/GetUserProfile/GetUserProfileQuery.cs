using AutismCenter.Application.Features.Users.Common;
using MediatR;

namespace AutismCenter.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;