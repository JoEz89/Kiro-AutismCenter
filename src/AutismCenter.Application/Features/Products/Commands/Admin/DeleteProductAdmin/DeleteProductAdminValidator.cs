using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteProductAdmin;

public class DeleteProductAdminValidator : AbstractValidator<DeleteProductAdminCommand>
{
    public DeleteProductAdminValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}