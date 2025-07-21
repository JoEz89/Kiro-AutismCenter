using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private CartItem() { } // For EF Core

    private CartItem(Guid cartId, Guid productId, int quantity, Money unitPrice)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static CartItem Create(Guid cartId, Guid productId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new CartItem(cartId, productId, quantity, unitPrice);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        UpdateTimestamp();
    }

    public void UpdateUnitPrice(Money newUnitPrice)
    {
        UnitPrice = newUnitPrice;
        UpdateTimestamp();
    }

    public Money GetTotalPrice()
    {
        return Money.Create(UnitPrice.Amount * Quantity, UnitPrice.Currency);
    }
}