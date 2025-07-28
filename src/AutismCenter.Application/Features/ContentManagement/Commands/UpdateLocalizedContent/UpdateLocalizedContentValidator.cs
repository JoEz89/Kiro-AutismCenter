using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;

public class UpdateLocalizedContentValidator : AbstractValidator<UpdateLocalizedContentCommand>
{
    public UpdateLocalizedContentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(10000)
            .WithMessage("Content must not exceed 10000 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}