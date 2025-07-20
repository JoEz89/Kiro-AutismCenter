using AutismCenter.Application.Features.Users.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        return user == null ? null : UserProfileDto.FromEntity(user);
    }
}