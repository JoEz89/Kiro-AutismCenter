using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Commands.CreateOrder;
using AutismCenter.Application.Features.Orders.Services;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOrderNumberService> _orderNumberServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _orderNumberServiceMock = new Mock<IOrderNumberService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateOrderHandler(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _userRepositoryMock.Object,
            _orderNumberServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";

        var user = User.Create(
            Email.Create("test@example.com"),
            "John",
            "Doe"
        );

        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "BHD"),
            10,
            Guid.NewGuid(),
            "PRD-001"
        );

        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemDto>
            {
                new(productId, 2)
            },
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country")
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderNumberServiceMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderNumber);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be(orderNumber);
        result.UserId.Should().Be(userId);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(productId);
        result.Items.First().Quantity.Should().Be(2);

        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemDto> { new(Guid.NewGuid(), 1) },
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country")
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID {userId} not found");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var user = User.Create(
            Email.Create("test@example.com"),
            "John",
            "Doe"
        );

        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemDto> { new(productId, 1) },
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country")
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found");
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var user = User.Create(
            Email.Create("test@example.com"),
            "John",
            "Doe"
        );

        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "BHD"),
            1, // Only 1 in stock
            Guid.NewGuid(),
            "PRD-001"
        );

        var command = new CreateOrderCommand(
            userId,
            new List<CreateOrderItemDto> { new(productId, 5) }, // Requesting 5
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country")
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Insufficient stock for product {product.GetName(false)}. Available: 1, Requested: 5");
    }
}



