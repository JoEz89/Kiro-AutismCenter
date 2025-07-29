using AutismCenter.Application.Features.Cart.Commands.AddToCart;
using AutismCenter.Application.Features.Cart.Commands.UpdateCartItem;
using AutismCenter.Application.Features.Cart.Commands.RemoveFromCart;
using AutismCenter.Application.Features.Cart.Commands.ClearCart;
using AutismCenter.Application.Features.Cart.Queries.GetCart;
using AutismCenter.Application.Features.Cart.Queries.GetCartItemCount;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CartController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public CartControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CartController();

        // Setup controller context with authenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        SetupAuthenticatedUser();

        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task GetCart_ValidRequest_ShouldReturnOkWithCart()
    {
        // Arrange
        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Product 1 EN",
                "Product 1 AR",
                2,
                29.99m,
                "USD",
                59.98m)
        };

        var cart = new CartDto(
            Guid.NewGuid(),
            _userId,
            cartItems,
            59.98m,
            "USD",
            2,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var expectedResponse = new GetCartResponse(cart);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCartResponse>().Subject;
        
        returnValue.Cart.Should().NotBeNull();
        returnValue.Cart!.UserId.Should().Be(_userId);
        returnValue.Cart.Items.Should().HaveCount(1);
        returnValue.Cart.TotalAmount.Should().Be(59.98m);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCartQuery>(q => q.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCart_EmptyCart_ShouldReturnOkWithNullCart()
    {
        // Arrange
        var expectedResponse = new GetCartResponse(null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCartResponse>().Subject;
        
        returnValue.Cart.Should().BeNull();

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCartQuery>(q => q.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCartItemCount_ValidRequest_ShouldReturnOkWithCount()
    {
        // Arrange
        var expectedResponse = new GetCartItemCountResponse(3);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCartItemCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCartItemCount();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCartItemCountResponse>().Subject;
        
        returnValue.ItemCount.Should().Be(3);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCartItemCountQuery>(q => q.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddToCart_ValidRequest_ShouldReturnOkWithUpdatedCart()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 2;
        var request = new AddToCartRequest(productId, quantity);

        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(
                Guid.NewGuid(),
                productId,
                "Product 1 EN",
                "Product 1 AR",
                quantity,
                29.99m,
                "USD",
                59.98m)
        };

        var cart = new CartDto(
            Guid.NewGuid(),
            _userId,
            cartItems,
            59.98m,
            "USD",
            2,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var expectedResponse = new AddToCartResponse(cart, "Item added to cart successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AddToCart(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<AddToCartResponse>().Subject;
        
        returnValue.Cart.UserId.Should().Be(_userId);
        returnValue.Cart.Items.Should().HaveCount(1);
        returnValue.Message.Should().Be("Item added to cart successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<AddToCartCommand>(c => 
                c.UserId == _userId &&
                c.ProductId == productId &&
                c.Quantity == quantity),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCartItem_ValidRequest_ShouldReturnOkWithUpdatedCart()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var newQuantity = 3;
        var request = new UpdateCartItemRequest(newQuantity);

        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(
                Guid.NewGuid(),
                productId,
                "Product 1 EN",
                "Product 1 AR",
                newQuantity,
                29.99m,
                "USD",
                89.97m)
        };

        var cart = new CartDto(
            Guid.NewGuid(),
            _userId,
            cartItems,
            89.97m,
            "USD",
            3,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var expectedResponse = new UpdateCartItemResponse(cart, "Cart item updated successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateCartItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateCartItem(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UpdateCartItemResponse>().Subject;
        
        returnValue.Cart.UserId.Should().Be(_userId);
        returnValue.Cart.TotalItemCount.Should().Be(3);
        returnValue.Message.Should().Be("Cart item updated successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdateCartItemCommand>(c => 
                c.UserId == _userId &&
                c.ProductId == productId &&
                c.Quantity == newQuantity),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromCart_ValidRequest_ShouldReturnOkWithUpdatedCart()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var cart = new CartDto(
            Guid.NewGuid(),
            _userId,
            new List<CartItemDto>(),
            0m,
            "USD",
            0,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var expectedResponse = new RemoveFromCartResponse(cart, "Item removed from cart successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RemoveFromCartCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RemoveFromCart(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<RemoveFromCartResponse>().Subject;
        
        returnValue.Cart.UserId.Should().Be(_userId);
        returnValue.Cart.Items.Should().BeEmpty();
        returnValue.Message.Should().Be("Item removed from cart successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<RemoveFromCartCommand>(c => 
                c.UserId == _userId &&
                c.ProductId == productId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearCart_ValidRequest_ShouldReturnOkWithEmptyCart()
    {
        // Arrange
        var cart = new CartDto(
            Guid.NewGuid(),
            _userId,
            new List<CartItemDto>(),
            0m,
            "USD",
            0,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var expectedResponse = new ClearCartResponse(cart, "Cart cleared successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ClearCartCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ClearCart();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ClearCartResponse>().Subject;
        
        returnValue.Cart.UserId.Should().Be(_userId);
        returnValue.Cart.Items.Should().BeEmpty();
        returnValue.Message.Should().Be("Cart cleared successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<ClearCartCommand>(c => c.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCart_UnauthenticatedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetCart());

        exception.Message.Should().Be("User ID not found in token");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<GetCartQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddToCart_UnauthenticatedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        var request = new AddToCartRequest(Guid.NewGuid(), 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.AddToCart(request));

        exception.Message.Should().Be("User ID not found in token");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<AddToCartCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupAuthenticatedUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        }, "test"));

        _controller.ControllerContext.HttpContext.User = user;
    }
}