using FluentValidation;

namespace AutismCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.PreferredLanguage)
            .Must(BeValidLanguage).WithMessage("Invalid language. Valid languages are: en, ar")
            .When(x => !string.IsNullOrEmpty(x.PreferredLanguage));

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private static bool BeValidLanguage(string? language)
    {
        if (string.IsNullOrEmpty(language))
            return true;

        var validLanguages = new[] { "en", "ar" };
        return validLanguages.Contains(language, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return true;

        // Basic phone number validation - can be enhanced based on requirements
        return phoneNumber.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')') 
               && phoneNumber.Length >= 8 && phoneNumber.Length <= 20;
    }
}