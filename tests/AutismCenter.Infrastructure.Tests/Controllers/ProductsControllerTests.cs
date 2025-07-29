using AutismCenter.Application.Features.Products.Queries.GetProducts;
using AutismCenter.Application.Features.Products.Queries.GetProductById;
using AutismCenter.Application.Features.Products.Queries.SearchProducts;
using AutismCenter.Application.Features.Products.Commands.CreateProduct;
using AutismCenter.Application.Features.Products.Commands.UpdateProduct;
using AutismCenter.Application.Features.Products.Commands.DeleteProduct;
using AutismCenter.Application.Features.Products.Commands.UpdateStock;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Application.Common.Models;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController();

        // Setup controller context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task GetProducts_ValidRequest_ShouldReturnOkWithProducts()
    {
        // Arrange
        var products = new List<ProductSummaryDto>
        {
            new ProductSummaryDto(
                Guid.NewGuid(),
                "Product 1 EN",
                "Product 1 AR",
                29.99m,
                "USD",
                10,
                "Category 1 EN",
                "Category 1 AR",
                true,
                "PRD-001",
                "image1.jpg")
        };

        var pagedResult = new PagedResult<ProductSummaryDto>(products, 1, 1, 10);
        var expectedResponse = new GetProductsResponse(pagedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetProductsResponse>().Subject;
        
        returnValue.Products.Items.Should().HaveCount(1);
        returnValue.Products.Items.First().NameEn.Should().Be("Product 1 EN");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<GetProductsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProducts_WithFilters_ShouldPassFiltersToQuery()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var minPrice = 10.0m;
        var maxPrice = 100.0m;
        var sortBy = "price";
        var sortDescending = true;

        var pagedResult = new PagedResult<ProductSummaryDto>(new List<ProductSummaryDto>(), 0, 1, 10);
        var expectedResponse = new GetProductsResponse(pagedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetProducts(
            pageNumber: 2,
            pageSize: 20,
            categoryId: categoryId,
            isActive: true,
            inStockOnly: true,
            minPrice: minPrice,
            maxPrice: maxPrice,
            sortBy: sortBy,
            sortDescending: sortDescending);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetProductsQuery>(q => 
                q.PageNumber == 2 &&
                q.PageSize == 20 &&
                q.CategoryId == categoryId &&
                q.IsActive == true &&
                q.InStockOnly == true &&
                q.MinPrice == minPrice &&
                q.MaxPrice == maxPrice &&
                q.SortBy == sortBy &&
                q.SortDescending == sortDescending),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ShouldReturnOkWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto(
            productId,
            "Product EN",
            "Product AR",
            "Description EN",
            "Description AR",
            29.99m,
            "USD",
            10,
            Guid.NewGuid(),
            "Category EN",
            "Category AR",
            true,
            "PRD-001",
            new List<string> { "image1.jpg" },
            DateTime.UtcNow,
            DateTime.UtcNow);

        var expectedResponse = new GetProductByIdResponse(product);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetProductByIdResponse>().Subject;
        
        returnValue.Product.Should().NotBeNull();
        returnValue.Product!.Id.Should().Be(productId);
        returnValue.Product.NameEn.Should().Be("Product EN");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.Id == productId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductById_NonExistingProduct_ShouldReturnNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expectedResponse = new GetProductByIdResponse(null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Product with ID {productId} not found");
    }

    [Fact]
    public async Task SearchProducts_ValidSearchTerm_ShouldReturnOkWithResults()
    {
        // Arrange
        var searchTerm = "autism";
        var products = new List<ProductSummaryDto>
        {
            new ProductSummaryDto(
                Guid.NewGuid(),
                "Autism Guide EN",
                "Autism Guide AR",
                19.99m,
                "USD",
                5,
                "Books EN",
                "Books AR",
                true,
                "PRD-002",
                "book1.jpg")
        };

        var pagedResult = new PagedResult<ProductSummaryDto>(products, 1, 1, 10);
        var expectedResponse = new SearchProductsResponse(pagedResult, searchTerm);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<SearchProductsResponse>().Subject;
        
        returnValue.Products.Items.Should().HaveCount(1);
        returnValue.SearchTerm.Should().Be(searchTerm);

        _mediatorMock.Verify(x => x.Send(
            It.Is<SearchProductsQuery>(q => q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchProducts_EmptySearchTerm_ShouldReturnBadRequest()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Search term is required");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<SearchProductsQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ShouldReturnCreatedWithProduct()
    {
        // Arrange
        SetupAdminUser();

        var request = new CreateProductRequest(
            "New Product EN",
            "New Product AR",
            "Description EN",
            "Description AR",
            39.99m,
            "USD",
            15,
            Guid.NewGuid(),
            "PRD-003",
            new List<string> { "newproduct.jpg" });

        var productId = Guid.NewGuid();
        var product = new ProductDto(
            productId,
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Price,
            request.Currency,
            request.StockQuantity,
            request.CategoryId,
            "Category EN",
            "Category AR",
            true,
            request.ProductSku,
            request.ImageUrls ?? new List<string>(),
            DateTime.UtcNow,
            DateTime.UtcNow);

        var expectedResponse = new CreateProductResponse(product, "Product created successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnValue = createdResult.Value.Should().BeOfType<CreateProductResponse>().Subject;
        
        returnValue.Product.NameEn.Should().Be("New Product EN");
        returnValue.Message.Should().Be("Product created successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<CreateProductCommand>(c => 
                c.NameEn == request.NameEn &&
                c.NameAr == request.NameAr &&
                c.Price == request.Price &&
                c.ProductSku == request.ProductSku),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ValidRequest_ShouldReturnOkWithUpdatedProduct()
    {
        // Arrange
        SetupAdminUser();

        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest(
            "Updated Product EN",
            "Updated Product AR",
            "Updated Description EN",
            "Updated Description AR",
            49.99m,
            "USD",
            new List<string> { "updated.jpg" });

        var product = new ProductDto(
            productId,
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Price,
            request.Currency,
            10,
            Guid.NewGuid(),
            "Category EN",
            "Category AR",
            true,
            "PRD-001",
            request.ImageUrls ?? new List<string>(),
            DateTime.UtcNow,
            DateTime.UtcNow);

        var expectedResponse = new UpdateProductResponse(product, "Product updated successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UpdateProductResponse>().Subject;
        
        returnValue.Product.NameEn.Should().Be("Updated Product EN");
        returnValue.Message.Should().Be("Product updated successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdateProductCommand>(c => 
                c.Id == productId &&
                c.NameEn == request.NameEn &&
                c.Price == request.Price),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStock_ValidRequest_ShouldReturnOkWithStockUpdate()
    {
        // Arrange
        SetupAdminUser();

        var productId = Guid.NewGuid();
        var request = new UpdateStockRequest(25);

        var expectedResponse = new UpdateStockResponse(
            productId,
            10,
            25,
            "Stock updated successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateStock(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UpdateStockResponse>().Subject;
        
        returnValue.ProductId.Should().Be(productId);
        returnValue.NewQuantity.Should().Be(25);
        returnValue.Message.Should().Be("Stock updated successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdateStockCommand>(c => 
                c.ProductId == productId &&
                c.NewQuantity == 25),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_ValidRequest_ShouldReturnOkWithDeleteResponse()
    {
        // Arrange
        SetupAdminUser();

        var productId = Guid.NewGuid();
        var expectedResponse = new DeleteProductResponse(true, "Product deleted successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<DeleteProductResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Product deleted successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<DeleteProductCommand>(c => c.Id == productId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupAdminUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "test"));

        _controller.ControllerContext.HttpContext.User = user;
    }
}