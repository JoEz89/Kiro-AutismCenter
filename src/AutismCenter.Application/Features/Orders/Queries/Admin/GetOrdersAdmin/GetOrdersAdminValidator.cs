using FluentValidation;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;

public class GetOrdersAdminValidator : AbstractValidator<GetOrdersAdminQuery>
{
    public GetOrdersAdminValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before or equal to end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("End date cannot be in the future")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.PaymentStatus)
            .IsInEnum().WithMessage("Invalid payment status")
            .When(x => x.PaymentStatus.HasValue);

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

        var validFields = new[] { "CreatedAt", "OrderNumber", "Status", "TotalAmount" };
        return validFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}