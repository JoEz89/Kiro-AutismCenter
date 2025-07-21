using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Cart.Common;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductNameEn,
    string ProductNameAr,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal TotalPrice,
    ProductDto? Product = null
)
{
    public static CartItemDto FromEntity(CartItem cartItem)
    {
        var totalPrice = cartItem.GetTotalPrice();
        
        return new CartItemDto(
            cartItem.Id,
            cartItem.ProductId,
            cartItem.Product?.NameEn ?? "",
            cartItem.Product?.NameAr ?? "",
            cartItem.Quantity,
            cartItem.UnitPrice.Amount,
            cartItem.UnitPrice.Currency,
            totalPrice.Amount,
            cartItem.Product != null ? ProductDto.FromEntity(cartItem.Product) : null
        );
    }
}