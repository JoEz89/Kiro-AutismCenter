using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.UpdateStock;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, UpdateStockResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateStockResponse> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        var oldQuantity = product.StockQuantity;

        // Update stock
        product.UpdateStock(request.NewQuantity);

        // Save changes
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateStockResponse(
            product.Id,
            oldQuantity,
            product.StockQuantity,
            "Stock updated successfully"
        );
    }
}