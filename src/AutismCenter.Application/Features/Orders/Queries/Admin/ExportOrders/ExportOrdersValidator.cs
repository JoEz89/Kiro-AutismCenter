using FluentValidation;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.ExportOrders;

public class ExportOrdersValidator : AbstractValidator<ExportOrdersQuery>
{
    public ExportOrdersValidator()
    {
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

        RuleFor(x => x.Format)
            .NotEmpty().WithMessage("Export format is required")
            .Must(BeValidFormat).WithMessage("Format must be CSV");
    }

    private static bool BeValidFormat(string format)
    {
        return format.ToUpperInvariant() == "CSV";
    }
}