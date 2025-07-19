using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // For EF Core

    private Order(string orderNumber, Guid userId, Address shippingAddress, Address billingAddress)
    {
        OrderNumber = orderNumber;
        UserId = userId;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        TotalAmount = Money.Create(0);
    }

    public static Order Create(Guid userId, Address shippingAddress, Address billingAddress, string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));

        var order = new Order(orderNumber, userId, shippingAddress, billingAddress);
        
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.OrderNumber, userId));
        
        return order;
    }

    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = OrderItem.Create(Id, product.Id, quantity, unitPrice);
            _items.Add(orderItem);
        }

        RecalculateTotal();
        UpdateTimestamp();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
            UpdateTimestamp();
        }
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Item not found in order");

        item.UpdateQuantity(newQuantity);
        RecalculateTotal();
        UpdateTimestamp();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order with status {Status}");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        UpdateTimestamp();

        AddDomainEvent(new OrderConfirmedEvent(Id, OrderNumber, UserId, TotalAmount));
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot start processing order with status {Status}");

        Status = OrderStatus.Processing;
        UpdateTimestamp();

        AddDomainEvent(new OrderProcessingStartedEvent(Id, OrderNumber));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot ship order with status {Status}");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new OrderShippedEvent(Id, OrderNumber, ShippedAt.Value));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order with status {Status}");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new OrderDeliveredEvent(Id, OrderNumber, DeliveredAt.Value));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order with status {Status}");

        Status = OrderStatus.Cancelled;
        UpdateTimestamp();

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, UserId));
    }

    public void MarkPaymentCompleted(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
            throw new ArgumentException("Payment ID cannot be empty", nameof(paymentId));

        PaymentId = paymentId;
        PaymentStatus = PaymentStatus.Completed;
        UpdateTimestamp();

        AddDomainEvent(new OrderPaymentCompletedEvent(Id, OrderNumber, paymentId, TotalAmount));
    }

    public void MarkPaymentFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
        UpdateTimestamp();

        AddDomainEvent(new OrderPaymentFailedEvent(Id, OrderNumber));
    }

    public void ProcessRefund()
    {
        if (PaymentStatus != PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot refund order that hasn't been paid");

        Status = OrderStatus.Refunded;
        PaymentStatus = PaymentStatus.Refunded;
        UpdateTimestamp();

        AddDomainEvent(new OrderRefundedEvent(Id, OrderNumber, TotalAmount));
    }

    private void RecalculateTotal()
    {
        var total = _items.Sum(item => item.GetTotalPrice().Amount);
        var currency = _items.FirstOrDefault()?.UnitPrice.Currency ?? "BHD";
        TotalAmount = Money.Create(total, currency);
    }

    public bool CanBeCancelled() => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;

    public bool IsModifiable() => Status == OrderStatus.Pending;

    public int GetTotalItemCount() => _items.Sum(i => i.Quantity);
}