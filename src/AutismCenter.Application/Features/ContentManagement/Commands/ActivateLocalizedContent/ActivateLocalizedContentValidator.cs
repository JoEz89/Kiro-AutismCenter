using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.ActivateLocalizedContent;

public class ActivateLocalizedContentValidator : AbstractValidator<ActivateLocalizedContentCommand>
{
    public ActivateLocalizedContentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
    }
}