using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteProductAdmin;

public class DeleteProductAdminHandler : IRequestHandler<DeleteProductAdminCommand, DeleteProductAdminResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public DeleteProductAdminHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<DeleteProductAdminResponse> Handle(DeleteProductAdminCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.ProductId} not found");
        }

        // Check if product has been ordered
        var hasOrders = await _orderRepository.HasOrdersForProductAsync(request.ProductId, cancellationToken);
        if (hasOrders)
        {
            // Instead of deleting, deactivate the product to preserve order history
            product.Deactivate();
            await _productRepository.UpdateAsync(product, cancellationToken);

            return new DeleteProductAdminResponse(
                true,
                $"Product '{product.NameEn}' has been deactivated to preserve order history. It will no longer be available for purchase.");
        }

        // Safe to delete if no orders exist
        await _productRepository.DeleteAsync(product, cancellationToken);

        return new DeleteProductAdminResponse(
            true,
            $"Product '{product.NameEn}' has been successfully deleted");
    }
}