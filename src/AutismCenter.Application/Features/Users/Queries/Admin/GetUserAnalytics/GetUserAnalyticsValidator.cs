using FluentValidation;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;

public class GetUserAnalyticsValidator : AbstractValidator<GetUserAnalyticsQuery>
{
    public GetUserAnalyticsValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before or equal to end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("End date cannot be in the future")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role")
            .When(x => x.Role.HasValue);
    }
}