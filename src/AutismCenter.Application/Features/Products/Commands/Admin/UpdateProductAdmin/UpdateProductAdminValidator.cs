using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;

public class UpdateProductAdminValidator : AbstractValidator<UpdateProductAdminCommand>
{
    public UpdateProductAdminValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .MaximumLength(255).WithMessage("English name must not exceed 255 characters");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .MaximumLength(255).WithMessage("Arabic name must not exceed 255 characters");

        RuleFor(x => x.DescriptionEn)
            .NotEmpty().WithMessage("English description is required")
            .MaximumLength(2000).WithMessage("English description must not exceed 2000 characters");

        RuleFor(x => x.DescriptionAr)
            .NotEmpty().WithMessage("Arabic description is required")
            .MaximumLength(2000).WithMessage("Arabic description must not exceed 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(BeValidCurrency).WithMessage("Currency must be USD or BHD");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.ImageUrls)
            .Must(BeValidImageUrls).WithMessage("All image URLs must be valid URLs")
            .When(x => x.ImageUrls != null);
    }

    private static bool BeValidCurrency(string currency)
    {
        return currency is "USD" or "BHD";
    }

    private static bool BeValidImageUrls(IEnumerable<string>? imageUrls)
    {
        if (imageUrls == null) return true;
        
        return imageUrls.All(url => Uri.TryCreate(url, UriKind.Absolute, out _));
    }
}