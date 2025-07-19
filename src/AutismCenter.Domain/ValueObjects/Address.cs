using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(
            street.Trim(),
            city.Trim(),
            state?.Trim() ?? string.Empty,
            postalCode?.Trim() ?? string.Empty,
            country.Trim()
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString()
    {
        var parts = new List<string> { Street, City };
        
        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);
            
        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);
            
        parts.Add(Country);
        
        return string.Join(", ", parts);
    }
}