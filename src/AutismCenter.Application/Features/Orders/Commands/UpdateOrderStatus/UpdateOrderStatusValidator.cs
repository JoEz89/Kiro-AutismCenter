using AutismCenter.Domain.Enums;
using FluentValidation;

namespace AutismCenter.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid order status")
            .NotEqual(OrderStatus.Pending)
            .WithMessage("Cannot manually set status to Pending");
    }
}