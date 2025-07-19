using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnAddress()
    {
        // Arrange
        var street = "123 Main St";
        var city = "Manama";
        var state = "Capital";
        var postalCode = "12345";
        var country = "Bahrain";

        // Act
        var address = Address.Create(street, city, state, postalCode, country);

        // Assert
        Assert.Equal(street, address.Street);
        Assert.Equal(city, address.City);
        Assert.Equal(state, address.State);
        Assert.Equal(postalCode, address.PostalCode);
        Assert.Equal(country, address.Country);
    }

    [Theory]
    [InlineData("", "City", "State", "12345", "Country")]
    [InlineData(" ", "City", "State", "12345", "Country")]
    [InlineData(null, "City", "State", "12345", "Country")]
    public void Create_WithEmptyStreet_ShouldThrowArgumentException(string street, string city, string state, string postalCode, string country)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create(street, city, state, postalCode, country));
    }

    [Theory]
    [InlineData("Street", "", "State", "12345", "Country")]
    [InlineData("Street", " ", "State", "12345", "Country")]
    [InlineData("Street", null, "State", "12345", "Country")]
    public void Create_WithEmptyCity_ShouldThrowArgumentException(string street, string city, string state, string postalCode, string country)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create(street, city, state, postalCode, country));
    }

    [Theory]
    [InlineData("Street", "City", "State", "12345", "")]
    [InlineData("Street", "City", "State", "12345", " ")]
    [InlineData("Street", "City", "State", "12345", null)]
    public void Create_WithEmptyCountry_ShouldThrowArgumentException(string street, string city, string state, string postalCode, string country)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create(street, city, state, postalCode, country));
    }

    [Fact]
    public void Create_WithNullStateAndPostalCode_ShouldUseEmptyString()
    {
        // Arrange
        var street = "123 Main St";
        var city = "Manama";
        var country = "Bahrain";

        // Act
        var address = Address.Create(street, city, null, null, country);

        // Assert
        Assert.Equal(string.Empty, address.State);
        Assert.Equal(string.Empty, address.PostalCode);
    }

    [Fact]
    public void Equals_WithSameData_ShouldReturnTrue()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var address2 = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");

        // Act & Assert
        Assert.Equal(address1, address2);
    }

    [Fact]
    public void ToString_WithAllFields_ShouldReturnFormattedString()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");

        // Act
        var result = address.ToString();

        // Assert
        Assert.Equal("123 Main St, Manama, Capital, 12345, Bahrain", result);
    }

    [Fact]
    public void ToString_WithoutStateAndPostalCode_ShouldReturnFormattedString()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Manama", "", "", "Bahrain");

        // Act
        var result = address.ToString();

        // Assert
        Assert.Equal("123 Main St, Manama, Bahrain", result);
    }
}