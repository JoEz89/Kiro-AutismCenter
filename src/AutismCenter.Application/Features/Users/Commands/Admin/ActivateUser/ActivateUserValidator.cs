using FluentValidation;

namespace AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;

public class ActivateUserValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}