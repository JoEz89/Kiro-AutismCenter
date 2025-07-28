using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .MaximumLength(255).WithMessage("English name must not exceed 255 characters");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .MaximumLength(255).WithMessage("Arabic name must not exceed 255 characters");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(2000).WithMessage("English description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionEn));

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(2000).WithMessage("Arabic description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));
    }
}