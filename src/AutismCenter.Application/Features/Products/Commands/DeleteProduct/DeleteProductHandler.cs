using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, DeleteProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return new DeleteProductResponse(false, "Product not found");
        }

        // Check if product has been ordered (soft delete by deactivating instead)
        var hasOrders = await _orderRepository.HasProductBeenOrderedAsync(request.Id, cancellationToken);
        if (hasOrders)
        {
            // Soft delete by deactivating the product
            product.Deactivate();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteProductResponse(true, "Product has been deactivated as it has existing orders");
        }

        // Hard delete if no orders exist
        await _productRepository.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteProductResponse(true, "Product deleted successfully");
    }
}