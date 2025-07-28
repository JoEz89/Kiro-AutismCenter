using FluentValidation;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;

public class UpdateOrderStatusAdminValidator : AbstractValidator<UpdateOrderStatusAdminCommand>
{
    public UpdateOrderStatusAdminValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status")
            .NotEqual(OrderStatus.Pending).WithMessage("Cannot set status back to Pending");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000).WithMessage("Admin notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.AdminNotes));
    }
}