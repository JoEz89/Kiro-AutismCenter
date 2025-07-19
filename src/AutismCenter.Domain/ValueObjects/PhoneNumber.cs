using AutismCenter.Domain.Common;
using System.Text.RegularExpressions;

namespace AutismCenter.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        // Remove all non-digit characters except + at the beginning
        var cleaned = Regex.Replace(phoneNumber.Trim(), @"[^\d+]", "");
        
        if (string.IsNullOrEmpty(cleaned))
            throw new ArgumentException("Phone number must contain digits", nameof(phoneNumber));

        // Basic validation - should start with + and have at least 7 digits
        if (!Regex.IsMatch(cleaned, @"^\+?\d{7,15}$"))
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}