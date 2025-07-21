using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Cart.Common;

public record CartDto(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemDto> Items,
    decimal TotalAmount,
    string Currency,
    int TotalItemCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ExpiresAt
)
{
    public static CartDto FromEntity(Domain.Entities.Cart cart)
    {
        var totalAmount = cart.GetTotalAmount();
        
        return new CartDto(
            cart.Id,
            cart.UserId,
            cart.Items.Select(CartItemDto.FromEntity),
            totalAmount.Amount,
            totalAmount.Currency,
            cart.GetTotalItemCount(),
            cart.CreatedAt,
            cart.UpdatedAt,
            cart.ExpiresAt
        );
    }
}