using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldReturnMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "BHD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Create_WithDefaultCurrency_ShouldUseBHD()
    {
        // Arrange
        var amount = 100.50m;

        // Act
        var money = Money.Create(amount);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal("BHD", money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(-10, "BHD"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCurrency_ShouldThrowArgumentException(string currency)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(100, currency));
    }

    [Fact]
    public void Create_WithInvalidCurrency_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(100, "EUR"));
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100, "BHD");
        var money2 = Money.Create(50, "BHD");

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150, result.Amount);
        Assert.Equal("BHD", result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100, "BHD");
        var money2 = Money.Create(50, "USD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100, "BHD");
        var money2 = Money.Create(30, "BHD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(70, result.Amount);
        Assert.Equal("BHD", result.Currency);
    }

    [Fact]
    public void Multiply_WithFactor_ShouldReturnProduct()
    {
        // Arrange
        var money = Money.Create(100, "BHD");
        var factor = 2.5m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.Equal(250, result.Amount);
        Assert.Equal("BHD", result.Currency);
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100, "BHD");
        var money2 = Money.Create(100, "BHD");

        // Act & Assert
        Assert.Equal(money1, money2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.Create(100.50m, "BHD");

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal("100.50 BHD", result);
    }
}