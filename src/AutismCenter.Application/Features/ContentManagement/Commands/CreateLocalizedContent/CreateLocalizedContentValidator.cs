using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;

public class CreateLocalizedContentValidator : AbstractValidator<CreateLocalizedContentCommand>
{
    public CreateLocalizedContentValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Key is required")
            .MaximumLength(255)
            .WithMessage("Key must not exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Key can only contain letters, numbers, dots, underscores, and hyphens");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(10000)
            .WithMessage("Content must not exceed 10000 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .MaximumLength(100)
            .WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Language)
            .IsInEnum()
            .WithMessage("Invalid language specified");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}