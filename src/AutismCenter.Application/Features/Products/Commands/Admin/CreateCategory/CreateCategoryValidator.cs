using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .MaximumLength(255).WithMessage("English name must not exceed 255 characters");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .MaximumLength(255).WithMessage("Arabic name must not exceed 255 characters");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(1000).WithMessage("English description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionEn));

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(1000).WithMessage("Arabic description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));
    }
}