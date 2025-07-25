using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AutismCenter.Domain.Tests.Entities;

public class OrderTests
{
    private readonly Address _shippingAddress;
    private readonly Address _billingAddress;
    private readonly Product _product;

    public OrderTests()
    {
        _shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        _billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        _product = Product.Create("Test Product", "منتج تجريبي", "Test Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
    }

    [Fact]
    public void Create_ValidParameters_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";

        // Act
        var order = Order.Create(userId, _shippingAddress, _billingAddress, orderNumber);

        // Assert
        order.Should().NotBeNull();
        order.UserId.Should().Be(userId);
        order.OrderNumber.Should().Be(orderNumber);
        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.ShippingAddress.Should().Be(_shippingAddress);
        order.BillingAddress.Should().Be(_billingAddress);
        order.TotalAmount.Amount.Should().Be(0);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyOrderNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Order.Create(userId, _shippingAddress, _billingAddress, ""));
    }

    [Fact]
    public void AddItem_ValidItem_ShouldAddItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var unitPrice = Money.Create(100, "BHD");

        // Act
        order.AddItem(_product, 2, unitPrice);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(_product.Id);
        order.Items.First().Quantity.Should().Be(2);
        order.TotalAmount.Amount.Should().Be(200);
    }

    [Fact]
    public void AddItem_SameProductTwice_ShouldUpdateQuantity()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var unitPrice = Money.Create(100, "BHD");

        // Act
        order.AddItem(_product, 2, unitPrice);
        order.AddItem(_product, 3, unitPrice);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.TotalAmount.Amount.Should().Be(500);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var unitPrice = Money.Create(100, "BHD");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddItem(_product, 0, unitPrice));
    }

    [Fact]
    public void AddItem_ConfirmedOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            order.AddItem(_product, 1, Money.Create(100, "BHD")));
    }

    [Fact]
    public void RemoveItem_ExistingItem_ShouldRemoveItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var unitPrice = Money.Create(100, "BHD");
        order.AddItem(_product, 2, unitPrice);

        // Act
        order.RemoveItem(_product.Id);

        // Assert
        order.Items.Should().BeEmpty();
        order.TotalAmount.Amount.Should().Be(0);
    }

    [Fact]
    public void UpdateItemQuantity_ValidQuantity_ShouldUpdateQuantityAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var unitPrice = Money.Create(100, "BHD");
        order.AddItem(_product, 2, unitPrice);

        // Act
        order.UpdateItemQuantity(_product.Id, 5);

        // Assert
        order.Items.First().Quantity.Should().Be(5);
        order.TotalAmount.Amount.Should().Be(500);
    }

    [Fact]
    public void UpdateItemQuantity_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 2, Money.Create(100, "BHD"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.UpdateItemQuantity(_product.Id, 0));
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentItem_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            order.UpdateItemQuantity(Guid.NewGuid(), 5));
    }

    [Fact]
    public void Confirm_PendingOrderWithItems_ShouldConfirmOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_OrderWithoutItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void StartProcessing_ConfirmedOrder_ShouldStartProcessing()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act
        order.StartProcessing();

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void StartProcessing_PendingOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.StartProcessing());
    }

    [Fact]
    public void Ship_ProcessingOrder_ShouldShipOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
        order.ShippedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Ship_ConfirmedOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Ship());
    }

    [Fact]
    public void Deliver_ShippedOrder_ShouldDeliverOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
        order.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deliver_ProcessingOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Deliver());
    }

    [Fact]
    public void Cancel_PendingOrder_ShouldCancelOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ConfirmedOrder_ShouldCancelOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void MarkPaymentCompleted_ValidPaymentId_ShouldMarkPaymentCompleted()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var paymentId = "payment_123456";

        // Act
        order.MarkPaymentCompleted(paymentId);

        // Assert
        order.PaymentId.Should().Be(paymentId);
        order.PaymentStatus.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void MarkPaymentCompleted_EmptyPaymentId_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.MarkPaymentCompleted(""));
    }

    [Fact]
    public void MarkPaymentFailed_ShouldMarkPaymentFailed()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act
        order.MarkPaymentFailed();

        // Assert
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void ProcessRefund_CompletedPayment_ShouldProcessRefund()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.MarkPaymentCompleted("payment_123456");

        // Act
        order.ProcessRefund();

        // Assert
        order.Status.Should().Be(OrderStatus.Refunded);
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void ProcessRefund_PendingPayment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ProcessRefund());
    }

    [Fact]
    public void CanBeCancelled_PendingOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        order.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_ConfirmedOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act & Assert
        order.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_DeliveredOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        // Act & Assert
        order.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void IsModifiable_PendingOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");

        // Act & Assert
        order.IsModifiable().Should().BeTrue();
    }

    [Fact]
    public void IsModifiable_ConfirmedOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        order.AddItem(_product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        // Act & Assert
        order.IsModifiable().Should().BeFalse();
    }

    [Fact]
    public void GetTotalItemCount_MultipleItems_ShouldReturnCorrectCount()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), _shippingAddress, _billingAddress, "ORD-2024-123456");
        var product2 = Product.Create("Product 2", "منتج 2", "Description 2", "وصف 2", Money.Create(50, "BHD"), 10, Guid.NewGuid(), "PRD-002");
        
        order.AddItem(_product, 3, Money.Create(100, "BHD"));
        order.AddItem(product2, 2, Money.Create(50, "BHD"));

        // Act & Assert
        order.GetTotalItemCount().Should().Be(5);
    }
}