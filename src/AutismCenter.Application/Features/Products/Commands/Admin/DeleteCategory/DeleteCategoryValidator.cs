using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");
    }
}