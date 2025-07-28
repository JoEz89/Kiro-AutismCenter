using FluentValidation;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;

public class GetUsersAdminValidator : AbstractValidator<GetUsersAdminQuery>
{
    public GetUsersAdminValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role")
            .When(x => x.Role.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private static bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy))
            return true;

        var validFields = new[] { "CreatedAt", "Email", "FirstName", "LastName", "Role" };
        return validFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}