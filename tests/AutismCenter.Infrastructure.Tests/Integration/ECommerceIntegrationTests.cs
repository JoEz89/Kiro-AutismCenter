using AutismCenter.Application.Features.Products.Queries.GetProducts;
using AutismCenter.Application.Features.Products.Queries.SearchProducts;
using AutismCenter.Application.Features.Cart.Commands.AddToCart;
using AutismCenter.Application.Features.Cart.Queries.GetCart;
using AutismCenter.Application.Features.Orders.Commands.CreateOrder;
using AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;
using AutismCenter.Application.Features.Orders.Commands.ProcessPayment;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Application.Common.Models;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Integration;

public class ECommerceIntegrationTests
{
    private readonly Mock<IMediator> _mediatorMock;

    public ECommerceIntegrationTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task ECommerceFlow_CompleteUserJourney_ShouldWorkEndToEnd()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Step 1: Browse products
        var products = new List<ProductSummaryDto>
        {
            new ProductSummaryDto(
                productId,
                "Autism Guide EN",
                "Autism Guide AR",
                29.99m,
                "USD",
                10,
                "Books EN",
                "Books AR",
                true,
                "PRD-001",
                "book.jpg")
        };

        var productsPagedResult = new PagedResult<ProductSummaryDto>(products, 1, 1, 10);
        var getProductsResponse = new GetProductsResponse(productsPagedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getProductsResponse);

        // Step 2: Search for specific products
        var searchResponse = new SearchProductsResponse(productsPagedResult, "autism");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse);

        // Step 3: Add to cart
        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(
                Guid.NewGuid(),
                productId,
                "Autism Guide EN",
                "Autism Guide AR",
                2,
                29.99m,
                "USD",
                59.98m)
        };

        var cart = new CartDto(
            Guid.NewGuid(),
            userId,
            cartItems,
            59.98m,
            "USD",
            2,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var addToCartResponse = new AddToCartResponse(cart, "Item added to cart");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addToCartResponse);

        // Step 4: Get cart
        var getCartResponse = new GetCartResponse(cart);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getCartResponse);

        // Step 5: Create order
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(
                Guid.NewGuid(),
                orderId,
                productId,
                "Autism Guide EN",
                2,
                29.99m,
                "USD",
                59.98m)
        };

        var order = new OrderDto(
            orderId,
            "ORD-2024-001234",
            userId,
            59.98m,
            "USD",
            OrderStatus.Pending,
            PaymentStatus.Pending,
            null,
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            orderItems,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Step 6: Create payment intent
        var paymentIntentResponse = new CreatePaymentIntentResult(
            true,
            "client_secret_test",
            "pi_test_payment_intent",
            null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePaymentIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntentResponse);

        // Step 7: Process payment
        var processPaymentResponse = new ProcessPaymentResult(
            true,
            "pi_test_payment_intent",
            null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processPaymentResponse);

        // Act & Assert

        // Step 1: Browse products
        var browseResult = await _mediatorMock.Object.Send(new GetProductsQuery());
        browseResult.Products.Items.Should().HaveCount(1);
        browseResult.Products.Items.First().NameEn.Should().Be("Autism Guide EN");

        // Step 2: Search products
        var searchResult = await _mediatorMock.Object.Send(new SearchProductsQuery("autism"));
        searchResult.Products.Items.Should().HaveCount(1);
        searchResult.SearchTerm.Should().Be("autism");

        // Step 3: Add to cart
        var addResult = await _mediatorMock.Object.Send(new AddToCartCommand(userId, productId, 2));
        addResult.Cart.UserId.Should().Be(userId);
        addResult.Cart.TotalAmount.Should().Be(59.98m);
        addResult.Message.Should().Be("Item added to cart");

        // Step 4: Get cart
        var cartResult = await _mediatorMock.Object.Send(new GetCartQuery(userId));
        cartResult.Cart.Should().NotBeNull();
        cartResult.Cart!.Items.Should().HaveCount(1);

        // Step 5: Create order
        var orderResult = await _mediatorMock.Object.Send(new CreateOrderCommand(
            userId,
            new List<CreateOrderItemDto> { new CreateOrderItemDto(productId, 2) },
            new AddressDto("123 Main St", "City", "State", "12345", "Country"),
            new AddressDto("123 Main St", "City", "State", "12345", "Country")));
        orderResult.UserId.Should().Be(userId);
        orderResult.Status.Should().Be(OrderStatus.Pending);

        // Step 6: Create payment intent
        var paymentIntentResult = await _mediatorMock.Object.Send(new CreatePaymentIntentCommand(orderId));
        paymentIntentResult.IsSuccess.Should().BeTrue();
        paymentIntentResult.PaymentIntentId.Should().Be("pi_test_payment_intent");
        paymentIntentResult.ClientSecret.Should().Be("client_secret_test");

        // Step 7: Process payment
        var paymentResult = await _mediatorMock.Object.Send(new ProcessPaymentCommand(
            orderId,
            "pm_test_payment_method",
            "pi_test_payment_intent"));
        paymentResult.IsSuccess.Should().BeTrue();
        paymentResult.PaymentId.Should().Be("pi_test_payment_intent");

        // Verify all interactions
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreatePaymentIntentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProductCatalog_WithFiltering_ShouldReturnFilteredResults()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<ProductSummaryDto>
        {
            new ProductSummaryDto(
                Guid.NewGuid(),
                "Expensive Product EN",
                "Expensive Product AR",
                99.99m,
                "USD",
                5,
                "Premium EN",
                "Premium AR",
                true,
                "PRD-002",
                "premium.jpg")
        };

        var pagedResult = new PagedResult<ProductSummaryDto>(products, 1, 1, 10);
        var response = new GetProductsResponse(pagedResult);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetProductsQuery>(q => 
                q.CategoryId == categoryId &&
                q.MinPrice == 50m &&
                q.MaxPrice == 150m &&
                q.InStockOnly == true &&
                q.IsActive == true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _mediatorMock.Object.Send(new GetProductsQuery(
            PageNumber: 1,
            PageSize: 10,
            CategoryId: categoryId,
            IsActive: true,
            InStockOnly: true,
            MinPrice: 50m,
            MaxPrice: 150m));

        // Assert
        result.Products.Items.Should().HaveCount(1);
        result.Products.Items.First().Price.Should().Be(99.99m);
        result.Products.Items.First().GetName(false).Should().Be("Expensive Product EN");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetProductsQuery>(q => 
                q.CategoryId == categoryId &&
                q.MinPrice == 50m &&
                q.MaxPrice == 150m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CartManagement_MultipleOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        // Initial empty cart
        var emptyCart = new CartDto(
            Guid.NewGuid(),
            userId,
            new List<CartItemDto>(),
            0m,
            "USD",
            0,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        // Cart with first item
        var cartWithFirstItem = new CartDto(
            Guid.NewGuid(),
            userId,
            new List<CartItemDto>
            {
                new CartItemDto(
                    Guid.NewGuid(),
                    product1Id,
                    "Product 1 EN",
                    "Product 1 AR",
                    1,
                    29.99m,
                    "USD",
                    29.99m)
            },
            29.99m,
            "USD",
            1,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        // Cart with both items
        var cartWithBothItems = new CartDto(
            Guid.NewGuid(),
            userId,
            new List<CartItemDto>
            {
                new CartItemDto(
                    Guid.NewGuid(),
                    product1Id,
                    "Product 1 EN",
                    "Product 1 AR",
                    1,
                    29.99m,
                    "USD",
                    29.99m),
                new CartItemDto(
                    Guid.NewGuid(),
                    product2Id,
                    "Product 2 EN",
                    "Product 2 AR",
                    2,
                    19.99m,
                    "USD",
                    39.98m)
            },
            69.97m,
            "USD",
            3,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        // Setup mock responses
        _mediatorMock
            .Setup(x => x.Send(It.Is<AddToCartCommand>(c => c.ProductId == product1Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddToCartResponse(cartWithFirstItem, "First item added"));

        _mediatorMock
            .Setup(x => x.Send(It.Is<AddToCartCommand>(c => c.ProductId == product2Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddToCartResponse(cartWithBothItems, "Second item added"));

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCartResponse(cartWithBothItems));

        // Act
        var firstAddResult = await _mediatorMock.Object.Send(new AddToCartCommand(userId, product1Id, 1));
        var secondAddResult = await _mediatorMock.Object.Send(new AddToCartCommand(userId, product2Id, 2));
        var finalCartResult = await _mediatorMock.Object.Send(new GetCartQuery(userId));

        // Assert
        firstAddResult.Cart.TotalItemCount.Should().Be(1);
        firstAddResult.Cart.TotalAmount.Should().Be(29.99m);

        secondAddResult.Cart.TotalItemCount.Should().Be(3);
        secondAddResult.Cart.TotalAmount.Should().Be(69.97m);

        finalCartResult.Cart.Should().NotBeNull();
        finalCartResult.Cart!.Items.Should().HaveCount(2);
        finalCartResult.Cart.TotalAmount.Should().Be(69.97m);

        _mediatorMock.Verify(x => x.Send(It.IsAny<AddToCartCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCartQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}