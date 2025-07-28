using FluentValidation;

namespace AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;

public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role");
    }
}