using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeactivateLocalizedContent;

public class DeactivateLocalizedContentValidator : AbstractValidator<DeactivateLocalizedContentCommand>
{
    public DeactivateLocalizedContentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
    }
}