using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword,
    string PreferredLanguage = "en"
) : IRequest<RegisterUserResponse>;