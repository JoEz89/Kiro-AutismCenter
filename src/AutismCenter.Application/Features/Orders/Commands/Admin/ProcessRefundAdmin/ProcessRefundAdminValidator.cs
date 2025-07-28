using FluentValidation;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;

public class ProcessRefundAdminValidator : AbstractValidator<ProcessRefundAdminCommand>
{
    public ProcessRefundAdminValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than 0")
            .When(x => x.RefundAmount.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Refund reason must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}