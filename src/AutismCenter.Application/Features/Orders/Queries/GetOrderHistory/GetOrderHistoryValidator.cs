using FluentValidation;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrderHistory;

public class GetOrderHistoryValidator : AbstractValidator<GetOrderHistoryQuery>
{
    public GetOrderHistoryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");
    }
}