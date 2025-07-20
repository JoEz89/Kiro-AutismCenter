using FluentValidation;

namespace AutismCenter.Application.Features.Users.Commands.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeValidRole).WithMessage("Invalid role. Valid roles are: Patient, Doctor, Admin");

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty().WithMessage("Preferred language is required")
            .Must(BeValidLanguage).WithMessage("Invalid language. Valid languages are: en, ar");

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }

    private static bool BeValidRole(string role)
    {
        var validRoles = new[] { "Patient", "Doctor", "Admin" };
        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidLanguage(string language)
    {
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