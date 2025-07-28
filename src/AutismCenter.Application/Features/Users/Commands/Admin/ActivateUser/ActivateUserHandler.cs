using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;

public class ActivateUserHandler : IRequestHandler<ActivateUserCommand, ActivateUserResponse>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ActivateUserResponse> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        user.Activate();

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ActivateUserResponse(
            user.Id,
            user.Email.Value,
            user.GetFullName(),
            user.IsActive,
            user.UpdatedAt);
    }
}