using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? PreferredLanguage = null
) : IRequest<UpdateUserResponse>;