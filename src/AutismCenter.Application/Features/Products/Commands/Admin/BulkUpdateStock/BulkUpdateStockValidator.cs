using FluentValidation;

namespace AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;

public class BulkUpdateStockValidator : AbstractValidator<BulkUpdateStockCommand>
{
    public BulkUpdateStockValidator()
    {
        RuleFor(x => x.StockUpdates)
            .NotEmpty().WithMessage("At least one stock update is required")
            .Must(HaveUniqueProductIds).WithMessage("Duplicate product IDs are not allowed");

        RuleForEach(x => x.StockUpdates)
            .SetValidator(new StockUpdateItemValidator());
    }

    private static bool HaveUniqueProductIds(IEnumerable<StockUpdateItem> stockUpdates)
    {
        var productIds = stockUpdates.Select(x => x.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}

public class StockUpdateItemValidator : AbstractValidator<StockUpdateItem>
{
    public StockUpdateItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");
    }
}