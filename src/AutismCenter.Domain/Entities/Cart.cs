using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    // Navigation properties
    public User User { get; private set; } = null!;

    private Cart() { } // For EF Core

    private Cart(Guid userId, DateTime? expiresAt = null)
    {
        UserId = userId;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30); // Default 30-day expiration
    }

    public static Cart Create(Guid userId, DateTime? expiresAt = null)
    {
        var cart = new Cart(userId, expiresAt);
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, cart.UserId));
        return cart;
    }

    public void AddItem(Guid productId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (IsExpired())
            throw new InvalidOperationException("Cannot add items to expired cart");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = CartItem.Create(Id, productId, quantity, unitPrice);
            _items.Add(newItem);
        }

        UpdateTimestamp();
        AddDomainEvent(new CartItemAddedEvent(Id, productId, quantity));
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot update items in expired cart");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in cart");

        if (newQuantity <= 0)
        {
            RemoveItem(productId);
            return;
        }

        var oldQuantity = item.Quantity;
        item.UpdateQuantity(newQuantity);
        UpdateTimestamp();

        AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, productId, oldQuantity, newQuantity));
    }

    public void RemoveItem(Guid productId)
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot remove items from expired cart");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            return;

        _items.Remove(item);
        UpdateTimestamp();

        AddDomainEvent(new CartItemRemovedEvent(Id, productId));
    }

    public void Clear()
    {
        if (!_items.Any())
            return;

        _items.Clear();
        UpdateTimestamp();

        AddDomainEvent(new CartClearedEvent(Id));
    }

    public Money GetTotalAmount()
    {
        if (!_items.Any())
            return Money.Create(0, "BHD");

        var total = _items.Sum(item => item.GetTotalPrice().Amount);
        return Money.Create(total, _items.First().UnitPrice.Currency);
    }

    public int GetTotalItemCount()
    {
        return _items.Sum(item => item.Quantity);
    }

    public bool IsEmpty() => !_items.Any();

    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public void ExtendExpiration(int days = 30)
    {
        ExpiresAt = DateTime.UtcNow.AddDays(days);
        UpdateTimestamp();
    }

    public bool HasItem(Guid productId)
    {
        return _items.Any(i => i.ProductId == productId);
    }

    public CartItem? GetItem(Guid productId)
    {
        return _items.FirstOrDefault(i => i.ProductId == productId);
    }

    public void ValidateStock(IEnumerable<Product> products)
    {
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var item in _items)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException($"Product {item.ProductId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.GetName(false)} is no longer available");

            if (!product.HasSufficientStock(item.Quantity))
                throw new InvalidOperationException($"Insufficient stock for {product.GetName(false)}. Available: {product.StockQuantity}, Required: {item.Quantity}");
        }
    }
}