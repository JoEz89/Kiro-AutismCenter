using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string? Password,
    string Role = "Patient",
    string PreferredLanguage = "en",
    string? PhoneNumber = null
) : IRequest<CreateUserResponse>;