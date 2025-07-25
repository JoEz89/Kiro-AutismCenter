using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Commands.UpdateProduct;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateProductHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            "Old Name",
            "اسم قديم",
            "Old Description",
            "وصف قديم",
            Money.Create(100, "USD"),
            5,
            categoryId,
            "PRD-001"
        );

        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "اسم جديد",
            "New Description",
            "وصف جديد",
            200.50m,
            "USD",
            new[] { "https://example.com/new-image.jpg" }
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Setup the second call after update
        _productRepositoryMock.Setup(x => x.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.NameEn.Should().Be(command.NameEn);
        result.Product.NameAr.Should().Be(command.NameAr);
        result.Product.DescriptionEn.Should().Be(command.DescriptionEn);
        result.Product.DescriptionAr.Should().Be(command.DescriptionAr);
        result.Product.Price.Should().Be(command.Price);
        result.Product.Currency.Should().Be(command.Currency);
        result.Message.Should().Be("Product updated successfully");

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "اسم جديد",
            "New Description",
            "وصف جديد",
            200.50m,
            "USD"
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Product not found");
        
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutImages_ShouldUpdateProductWithoutChangingImages()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            "Old Name",
            "اسم قديم",
            "Old Description",
            "وصف قديم",
            Money.Create(100, "USD"),
            5,
            categoryId,
            "PRD-001"
        );
        existingProduct.AddImage("https://example.com/existing-image.jpg");

        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "اسم جديد",
            "New Description",
            "وصف جديد",
            200.50m,
            "USD"
            // No images provided
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Setup the second call after update
        _productRepositoryMock.Setup(x => x.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.ImageUrls.Should().Contain("https://example.com/existing-image.jpg");
        result.Message.Should().Be("Product updated successfully");
    }

    [Fact]
    public async Task Handle_WithNewImages_ShouldReplaceAllImages()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            "Old Name",
            "اسم قديم",
            "Old Description",
            "وصف قديم",
            Money.Create(100, "USD"),
            5,
            categoryId,
            "PRD-001"
        );
        existingProduct.AddImage("https://example.com/old-image1.jpg");
        existingProduct.AddImage("https://example.com/old-image2.jpg");

        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "اسم جديد",
            "New Description",
            "وصف جديد",
            200.50m,
            "USD",
            new[] { "https://example.com/new-image1.jpg", "https://example.com/new-image2.jpg" }
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Setup the second call after update
        _productRepositoryMock.Setup(x => x.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.ImageUrls.Should().NotContain("https://example.com/old-image1.jpg");
        result.Product.ImageUrls.Should().NotContain("https://example.com/old-image2.jpg");
        result.Product.ImageUrls.Should().Contain("https://example.com/new-image1.jpg");
        result.Product.ImageUrls.Should().Contain("https://example.com/new-image2.jpg");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("BHD")]
    public async Task Handle_DifferentCurrencies_ShouldUpdateProductSuccessfully(string currency)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            "Old Name",
            "اسم قديم",
            "Old Description",
            "وصف قديم",
            Money.Create(100, "USD"),
            5,
            categoryId,
            "PRD-001"
        );

        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "اسم جديد",
            "New Description",
            "وصف جديد",
            200.50m,
            currency
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Setup the second call after update
        _productRepositoryMock.Setup(x => x.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Product.Currency.Should().Be(currency);
    }
}
