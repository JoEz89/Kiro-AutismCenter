using FluentValidation;

namespace AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;

public class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}