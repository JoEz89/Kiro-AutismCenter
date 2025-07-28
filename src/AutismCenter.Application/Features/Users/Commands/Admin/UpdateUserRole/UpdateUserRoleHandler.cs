using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;

public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, UpdateUserRoleResponse>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserRoleHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UpdateUserRoleResponse> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        user.ChangeRole(request.Role);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UpdateUserRoleResponse(
            user.Id,
            user.Email.Value,
            user.GetFullName(),
            user.Role,
            user.UpdatedAt);
    }
}