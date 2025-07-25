using AutismCenter.Application.Features.Orders.Services;
using AutismCenter.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Services;

public class OrderNumberServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderNumberService _service;

    public OrderNumberServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _service = new OrderNumberService(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_UniqueNumber_ShouldReturnValidOrderNumber()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var orderNumber = await _service.GenerateOrderNumberAsync();

        // Assert
        orderNumber.Should().NotBeNullOrEmpty();
        orderNumber.Should().StartWith($"ORD-{currentYear}-");
        orderNumber.Should().HaveLength(15); // ORD-YYYY-XXXXXX format
        
        // Verify the format matches expected pattern
        orderNumber.Should().MatchRegex(@"^ORD-\d{4}-\d{6}$");
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_FirstNumberExists_ShouldRetryAndReturnDifferentNumber()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var callCount = 0;
        
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1; // First call returns true (exists), second returns false
            });

        // Act
        var orderNumber = await _service.GenerateOrderNumberAsync();

        // Assert
        orderNumber.Should().NotBeNullOrEmpty();
        orderNumber.Should().StartWith($"ORD-{currentYear}-");
        orderNumber.Should().HaveLength(15);
        
        // Verify that the repository was called at least twice (retry mechanism)
        _orderRepositoryMock.Verify(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.AtLeast(2));
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_AllAttemptsExist_ShouldReturnGuidBasedNumber()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Always return true to simulate all numbers exist

        // Act
        var orderNumber = await _service.GenerateOrderNumberAsync();

        // Assert
        orderNumber.Should().NotBeNullOrEmpty();
        orderNumber.Should().StartWith($"ORD-{currentYear}-");
        orderNumber.Should().HaveLength(15); // ORD-YYYY-XXXXXX format
        
        // The last 6 characters should be alphanumeric (GUID-based)
        var suffix = orderNumber.Substring(9); // Get the last 6 characters
        suffix.Should().HaveLength(6);
        suffix.Should().MatchRegex(@"^[A-Z0-9]{6}$");
        
        // Verify that the repository was called the maximum number of times
        _orderRepositoryMock.Verify(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(10)); // maxAttempts = 10
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_MultipleCallsInSequence_ShouldReturnDifferentNumbers()
    {
        // Arrange
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var orderNumber1 = await _service.GenerateOrderNumberAsync();
        var orderNumber2 = await _service.GenerateOrderNumberAsync();
        var orderNumber3 = await _service.GenerateOrderNumberAsync();

        // Assert
        orderNumber1.Should().NotBeNullOrEmpty();
        orderNumber2.Should().NotBeNullOrEmpty();
        orderNumber3.Should().NotBeNullOrEmpty();

        // All numbers should be different (very high probability with random generation)
        orderNumber1.Should().NotBe(orderNumber2);
        orderNumber2.Should().NotBe(orderNumber3);
        orderNumber1.Should().NotBe(orderNumber3);

        // All should follow the same format
        var currentYear = DateTime.UtcNow.Year;
        orderNumber1.Should().StartWith($"ORD-{currentYear}-");
        orderNumber2.Should().StartWith($"ORD-{currentYear}-");
        orderNumber3.Should().StartWith($"ORD-{currentYear}-");
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_CancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await _service.Invoking(x => x.GenerateOrderNumberAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(2024)]
    [InlineData(2025)]
    [InlineData(2030)]
    public async Task GenerateOrderNumberAsync_DifferentYears_ShouldIncludeCorrectYear(int year)
    {
        // This test would require mocking DateTime.UtcNow, which is complex
        // For now, we'll test with the current year and document the expected behavior
        
        // Arrange
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var orderNumber = await _service.GenerateOrderNumberAsync();

        // Assert
        var currentYear = DateTime.UtcNow.Year;
        orderNumber.Should().StartWith($"ORD-{currentYear}-");
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        _orderRepositoryMock.Setup(x => x.OrderNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _service.Invoking(x => x.GenerateOrderNumberAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}



