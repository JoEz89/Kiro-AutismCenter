using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AutismCenter.Application.Features.Orders.Commands.CreateOrder;
using AutismCenter.Application.Features.Orders.Commands.UpdateOrderStatus;
using AutismCenter.Application.Features.Orders.Commands.CancelOrder;
using AutismCenter.Application.Features.Orders.Queries.GetOrders;
using AutismCenter.Application.Features.Orders.Queries.GetOrderById;
using AutismCenter.Application.Features.Orders.Queries.GetOrderHistory;
using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Enums;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : BaseController
{

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get orders with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetOrdersResponse>> GetOrders(
        [FromQuery] Guid? userId = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetOrdersQuery(userId, status, startDate, endDate, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, [FromQuery] Guid? userId = null)
    {
        var query = new GetOrderByIdQuery(id, userId);
        var result = await Mediator.Send(query);
        
        if (result == null)
            return NotFound($"Order with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Get order history for a specific user
    /// </summary>
    [HttpGet("history/{userId:guid}")]
    public async Task<ActionResult<GetOrderHistoryResponse>> GetOrderHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetOrderHistoryQuery(userId, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand(id, request.NewStatus);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromQuery] Guid? userId = null)
    {
        var command = new CancelOrderCommand(id, userId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}

public record UpdateOrderStatusRequest(OrderStatus NewStatus);