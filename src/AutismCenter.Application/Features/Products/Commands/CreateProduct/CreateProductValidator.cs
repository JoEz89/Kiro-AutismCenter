using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
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
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(BeValidCurrency).WithMessage("Currency must be USD or BHD");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.ProductSku)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters");

        RuleForEach(x => x.ImageUrls)
            .Must(BeValidUrl).WithMessage("Image URL must be a valid URL")
            .When(x => x.ImageUrls != null);
    }

    private static bool BeValidCurrency(string currency)
    {
        return currency is "USD" or "BHD";
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}