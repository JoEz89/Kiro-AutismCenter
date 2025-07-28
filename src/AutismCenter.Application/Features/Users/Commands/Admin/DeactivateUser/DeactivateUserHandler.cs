using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;

public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, DeactivateUserResponse>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<DeactivateUserResponse> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        user.Deactivate();

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new DeactivateUserResponse(
            user.Id,
            user.Email.Value,
            user.GetFullName(),
            user.IsActive,
            user.UpdatedAt);
    }
}