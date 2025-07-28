using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;

public class BulkUpdateLocalizedContentValidator : AbstractValidator<BulkUpdateLocalizedContentCommand>
{
    public BulkUpdateLocalizedContentValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .MaximumLength(100)
            .WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required")
            .Must(items => items.Count() <= 100)
            .WithMessage("Cannot update more than 100 items at once");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Key)
                .NotEmpty()
                .WithMessage("Key is required")
                .MaximumLength(255)
                .WithMessage("Key must not exceed 255 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .WithMessage("Key can only contain letters, numbers, dots, underscores, and hyphens");

            item.RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required")
                .MaximumLength(10000)
                .WithMessage("Content must not exceed 10000 characters");

            item.RuleFor(x => x.Language)
                .IsInEnum()
                .WithMessage("Invalid language specified");

            item.RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        });
    }
}