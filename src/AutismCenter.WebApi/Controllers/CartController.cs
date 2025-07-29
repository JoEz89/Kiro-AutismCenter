using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutismCenter.Application.Features.Cart.Commands.AddToCart;
using AutismCenter.Application.Features.Cart.Commands.UpdateCartItem;
using AutismCenter.Application.Features.Cart.Commands.RemoveFromCart;
using AutismCenter.Application.Features.Cart.Commands.ClearCart;
using AutismCenter.Application.Features.Cart.Queries.GetCart;
using AutismCenter.Application.Features.Cart.Queries.GetCartItemCount;
using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : BaseController
{
    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetCartResponse>> GetCart()
    {
        var userId = GetCurrentUserId();
        var query = new GetCartQuery(userId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get cart item count for current user
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<GetCartItemCountResponse>> GetCartItemCount()
    {
        var userId = GetCurrentUserId();
        var query = new GetCartItemCountQuery(userId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<AddToCartResponse>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new AddToCartCommand(userId, request.ProductId, request.Quantity);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<UpdateCartItemResponse>> UpdateCartItem(
        Guid productId, 
        [FromBody] UpdateCartItemRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateCartItemCommand(userId, productId, request.Quantity);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<RemoveFromCartResponse>> RemoveFromCart(Guid productId)
    {
        var userId = GetCurrentUserId();
        var command = new RemoveFromCartCommand(userId, productId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ClearCartResponse>> ClearCart()
    {
        var userId = GetCurrentUserId();
        var command = new ClearCartCommand(userId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}

public record AddToCartRequest(Guid ProductId, int Quantity);

public record UpdateCartItemRequest(int Quantity);