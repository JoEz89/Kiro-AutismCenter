using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Services;

public class OrderService
{
    private readonly IProductRepository _productRepository;

    public OrderService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Order> CreateOrderAsync(Guid userId, Address shippingAddress, Address billingAddress, 
        List<(Guid ProductId, int Quantity)> items, string orderNumber, CancellationToken cancellationToken = default)
    {
        // Order number should be provided by the application layer
        
        // Create order
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);

        // Add items to order
        foreach (var (productId, quantity) in items)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.GetName(false)} is not active");

            if (!product.HasSufficientStock(quantity))
                throw new InvalidOperationException($"Insufficient stock for product {product.GetName(false)}. Available: {product.StockQuantity}, Requested: {quantity}");

            order.AddItem(product, quantity, product.Price);
        }

        return order;
    }

    public async Task<bool> CanFulfillOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null || !product.IsActive || !product.HasSufficientStock(item.Quantity))
            {
                return false;
            }
        }

        return true;
    }

    public async Task ReserveInventoryAsync(Order order, CancellationToken cancellationToken = default)
    {
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found");

            product.ReduceStock(item.Quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }
    }

    public async Task ReleaseInventoryAsync(Order order, CancellationToken cancellationToken = default)
    {
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product != null)
            {
                product.RestoreStock(item.Quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
        }
    }

    public Money CalculateOrderTotal(IEnumerable<(Product Product, int Quantity)> items)
    {
        if (!items.Any())
            return Money.Create(0);

        var firstItem = items.First();
        var currency = firstItem.Product.Price.Currency;
        var total = 0m;

        foreach (var (product, quantity) in items)
        {
            if (product.Price.Currency != currency)
                throw new InvalidOperationException("All products must have the same currency");

            total += product.Price.Amount * quantity;
        }

        return Money.Create(total, currency);
    }
}