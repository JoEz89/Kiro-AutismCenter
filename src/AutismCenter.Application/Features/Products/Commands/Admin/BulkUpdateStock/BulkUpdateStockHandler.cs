using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;

public class BulkUpdateStockHandler : IRequestHandler<BulkUpdateStockCommand, BulkUpdateStockResponse>
{
    private readonly IProductRepository _productRepository;

    public BulkUpdateStockHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<BulkUpdateStockResponse> Handle(BulkUpdateStockCommand request, CancellationToken cancellationToken)
    {
        var results = new List<StockUpdateResult>();
        var totalUpdated = 0;
        var totalFailed = 0;

        foreach (var stockUpdate in request.StockUpdates)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(stockUpdate.ProductId, cancellationToken);
                if (product == null)
                {
                    results.Add(new StockUpdateResult(
                        stockUpdate.ProductId,
                        false,
                        "Product not found",
                        null,
                        null));
                    totalFailed++;
                    continue;
                }

                var oldQuantity = product.StockQuantity;
                product.UpdateStock(stockUpdate.NewQuantity);
                await _productRepository.UpdateAsync(product, cancellationToken);

                results.Add(new StockUpdateResult(
                    stockUpdate.ProductId,
                    true,
                    null,
                    oldQuantity,
                    stockUpdate.NewQuantity));
                totalUpdated++;
            }
            catch (Exception ex)
            {
                results.Add(new StockUpdateResult(
                    stockUpdate.ProductId,
                    false,
                    ex.Message,
                    null,
                    null));
                totalFailed++;
            }
        }

        return new BulkUpdateStockResponse(totalUpdated, totalFailed, results);
    }
}