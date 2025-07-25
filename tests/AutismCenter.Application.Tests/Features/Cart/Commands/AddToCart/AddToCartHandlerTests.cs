using Moq;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Cart.Commands.AddToCart;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Tests.Features.Cart.Commands.AddToCart;

public class AddToCartHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddToCartHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public AddToCartHandlerTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new AddToCartHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldAddItemToExistingCart()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 2);
        
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(10.50m, "BHD"), 10, _categoryId, "SKU001");
        var existingCart = Domain.Entities.Cart.Create(_userId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepositoryMock.Setup(x => x.GetActiveByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCart);
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(existingCart.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCart);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Item added to cart successfully", result.Message);
        Assert.Single(result.Cart.Items);
        Assert.Equal(2, result.Cart.TotalItemCount);

        _cartRepositoryMock.Verify(x => x.UpdateAsync(existingCart, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoExistingCart_ShouldCreateNewCart()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 1);
        
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(15.00m, "BHD"), 5, _categoryId, "SKU002");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepositoryMock.Setup(x => x.GetActiveByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Cart?)null);

        // Setup to return a new cart when GetByIdAsync is called
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => 
            {
                var cart = Domain.Entities.Cart.Create(_userId);
                cart.AddItem(_productId, 1, product.Price);
                return cart;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Item added to cart successfully", result.Message);

        _cartRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Cart>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 1);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowException()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 1);
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInactiveProduct_ShouldThrowException()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 1);
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(10.50m, "BHD"), 10, _categoryId, "SKU001");
        product.Deactivate();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var command = new AddToCartCommand(_userId, _productId, 10);
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(10.50m, "BHD"), 5, _categoryId, "SKU001"); // Only 5 in stock

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}
