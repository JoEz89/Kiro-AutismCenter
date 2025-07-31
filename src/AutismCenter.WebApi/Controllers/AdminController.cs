using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;
using AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;
using AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;
using AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;
using AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;
using AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;
using AutismCenter.Application.Features.Products.Queries.Admin.GetInventoryReport;
using AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;
using AutismCenter.Application.Features.Products.Commands.Admin.CreateProductAdmin;
using AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;
using AutismCenter.Application.Features.Products.Commands.Admin.DeleteProductAdmin;
using AutismCenter.Application.Features.Products.Commands.Admin.CreateCategory;
using AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;
using AutismCenter.Application.Features.Products.Commands.Admin.DeleteCategory;
using AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;
using AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;
using AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;
using AutismCenter.Application.Features.Orders.Queries.Admin.ExportOrders;
using AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;
using AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;
using AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContentList;
using AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;
using AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;
using AutismCenter.Application.Features.ContentManagement.Commands.DeleteLocalizedContent;
using AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;
using AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentsAdmin;
using AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;
using AutismCenter.Application.Features.Appointments.Commands.Admin.UpdateAppointmentStatusAdmin;
using AutismCenter.WebApi.Models.Admin;

namespace AutismCenter.WebApi.Controllers;

/// <summary>
/// Admin dashboard API endpoints for managing all aspects of the system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    #region User Management

    /// <summary>
    /// Get all users with filtering and pagination for admin management
    /// </summary>
    /// <param name="request">User filtering and pagination parameters</param>
    /// <returns>Paginated list of users with admin details</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(GetUsersAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetUsersAdminResponse>> GetUsers([FromQuery] GetUsersAdminRequest request)
    {
        try
        {
            var query = new GetUsersAdminQuery(
                request.PageNumber,
                request.PageSize,
                request.Role,
                request.IsActive,
                null, // IsEmailVerified
                request.SearchTerm,
                request.SortBy,
                request.SortDirection.ToLower() == "desc"
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get user analytics and statistics
    /// </summary>
    /// <param name="request">Analytics parameters</param>
    /// <returns>User analytics data</returns>
    [HttpGet("users/analytics")]
    [ProducesResponseType(typeof(GetUserAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetUserAnalyticsResponse>> GetUserAnalytics([FromQuery] GetUserAnalyticsRequest request)
    {
        try
        {
            var query = new GetUserAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                null // Role filter
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update user role
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Role update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("users/{userId:guid}/role")]
    [ProducesResponseType(typeof(UpdateUserRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateUserRoleResponse>> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var command = new UpdateUserRoleCommand(userId, request.Role);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Activate user account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Activation result</returns>
    [HttpPut("users/{userId:guid}/activate")]
    [ProducesResponseType(typeof(ActivateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivateUserResponse>> ActivateUser(Guid userId)
    {
        try
        {
            var command = new ActivateUserCommand(userId);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Deactivation result</returns>
    [HttpPut("users/{userId:guid}/deactivate")]
    [ProducesResponseType(typeof(DeactivateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeactivateUserResponse>> DeactivateUser(Guid userId)
    {
        try
        {
            var command = new DeactivateUserCommand(userId);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Product and Inventory Management

    /// <summary>
    /// Get product analytics and statistics
    /// </summary>
    /// <param name="request">Analytics parameters</param>
    /// <returns>Product analytics data</returns>
    [HttpGet("products/analytics")]
    [ProducesResponseType(typeof(GetProductAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetProductAnalyticsResponse>> GetProductAnalytics([FromQuery] GetProductAnalyticsRequest request)
    {
        try
        {
            var query = new GetProductAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                request.CategoryId
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get inventory report
    /// </summary>
    /// <param name="request">Inventory report parameters</param>
    /// <returns>Inventory report data</returns>
    [HttpGet("inventory/report")]
    [ProducesResponseType(typeof(GetInventoryReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetInventoryReportResponse>> GetInventoryReport([FromQuery] GetInventoryReportRequest request)
    {
        try
        {
            var query = new GetInventoryReportQuery(
                request.CategoryId,
                null, // IsActive
                !request.IncludeOutOfStock, // LowStockOnly (inverse of IncludeOutOfStock)
                request.LowStockThreshold ?? 10
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get categories for admin management
    /// </summary>
    /// <param name="request">Category filtering parameters</param>
    /// <returns>List of categories with admin details</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(GetCategoriesAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetCategoriesAdminResponse>> GetCategories([FromQuery] GetCategoriesAdminRequest request)
    {
        try
        {
            var query = new GetCategoriesAdminQuery(
                request.IsActive,
                request.IncludeProductCount
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product information</returns>
    [HttpPost("products")]
    [ProducesResponseType(typeof(CreateProductAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateProductAdminResponse>> CreateProduct([FromBody] CreateProductAdminRequest request)
    {
        try
        {
            var command = new CreateProductAdminCommand(
                request.NameEn,
                request.NameAr,
                request.DescriptionEn ?? string.Empty,
                request.DescriptionAr ?? string.Empty,
                request.Price,
                "BHD", // Currency
                request.StockQuantity,
                request.CategoryId,
                $"PRD-{Guid.NewGuid().ToString()[..8]}", // ProductSku
                request.ImageUrls,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(CreateProduct), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product information</returns>
    [HttpPut("products/{productId:guid}")]
    [ProducesResponseType(typeof(UpdateProductAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateProductAdminResponse>> UpdateProduct(Guid productId, [FromBody] UpdateProductAdminRequest request)
    {
        try
        {
            var command = new UpdateProductAdminCommand(
                productId,
                request.NameEn,
                request.NameAr,
                request.DescriptionEn ?? string.Empty,
                request.DescriptionAr ?? string.Empty,
                request.Price,
                "BHD", // Currency
                request.StockQuantity,
                request.CategoryId,
                request.ImageUrls,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("products/{productId:guid}")]
    [ProducesResponseType(typeof(DeleteProductAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteProductAdminResponse>> DeleteProduct(Guid productId)
    {
        try
        {
            var command = new DeleteProductAdminCommand(productId);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bulk update product stock quantities
    /// </summary>
    /// <param name="request">Bulk stock update request</param>
    /// <returns>Bulk update result</returns>
    [HttpPut("products/stock/bulk")]
    [ProducesResponseType(typeof(BulkUpdateStockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BulkUpdateStockResponse>> BulkUpdateStock([FromBody] BulkUpdateStockRequest request)
    {
        try
        {
            var stockUpdates = request.StockUpdates.Select(su => 
                new AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock.StockUpdateItem(
                    su.ProductId, 
                    su.StockQuantity
                ));
            var command = new BulkUpdateStockCommand(stockUpdates);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new category
    /// </summary>
    /// <param name="request">Category creation request</param>
    /// <returns>Created category information</returns>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(CreateCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateCategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var command = new CreateCategoryCommand(
                request.NameEn,
                request.NameAr,
                request.DescriptionEn,
                request.DescriptionAr,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(CreateCategory), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="request">Category update request</param>
    /// <returns>Updated category information</returns>
    [HttpPut("categories/{categoryId:guid}")]
    [ProducesResponseType(typeof(UpdateCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateCategoryResponse>> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var command = new UpdateCategoryCommand(
                categoryId,
                request.NameEn,
                request.NameAr,
                request.DescriptionEn,
                request.DescriptionAr,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("categories/{categoryId:guid}")]
    [ProducesResponseType(typeof(DeleteCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteCategoryResponse>> DeleteCategory(Guid categoryId)
    {
        try
        {
            var command = new DeleteCategoryCommand(categoryId);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Order Management

    /// <summary>
    /// Get all orders with filtering and pagination for admin management
    /// </summary>
    /// <param name="request">Order filtering and pagination parameters</param>
    /// <returns>Paginated list of orders with admin details</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(GetOrdersAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrdersAdminResponse>> GetOrders([FromQuery] GetOrdersAdminRequest request)
    {
        try
        {
            var query = new GetOrdersAdminQuery(
                request.PageNumber,
                request.PageSize,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.PaymentStatus,
                request.SearchTerm,
                request.SortBy,
                request.SortDirection.ToLower() == "desc"
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get order analytics and statistics
    /// </summary>
    /// <param name="request">Analytics parameters</param>
    /// <returns>Order analytics data</returns>
    [HttpGet("orders/analytics")]
    [ProducesResponseType(typeof(GetOrderAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrderAnalyticsResponse>> GetOrderAnalytics([FromQuery] GetOrderAnalyticsRequest request)
    {
        try
        {
            var query = new GetOrderAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                request.Status,
                null // UserId
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export orders to CSV format
    /// </summary>
    /// <param name="request">Export parameters</param>
    /// <returns>CSV file download</returns>
    [HttpGet("orders/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportOrders([FromQuery] ExportOrdersRequest request)
    {
        try
        {
            var query = new ExportOrdersQuery(
                request.StartDate,
                request.EndDate,
                request.Status,
                request.Format.ToUpper()
            );

            var result = await Mediator.Send(query);
            
            return File(
                result.FileContent,
                result.ContentType,
                result.FileName
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated order information</returns>
    [HttpPut("orders/{orderId:guid}/status")]
    [ProducesResponseType(typeof(UpdateOrderStatusAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateOrderStatusAdminResponse>> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusAdminRequest request)
    {
        try
        {
            var command = new UpdateOrderStatusAdminCommand(
                orderId,
                request.Status,
                request.Notes
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Process refund for order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Refund request</param>
    /// <returns>Refund processing result</returns>
    [HttpPost("orders/{orderId:guid}/refund")]
    [ProducesResponseType(typeof(ProcessRefundAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessRefundAdminResponse>> ProcessRefund(Guid orderId, [FromBody] ProcessRefundAdminRequest request)
    {
        try
        {
            var command = new ProcessRefundAdminCommand(
                orderId,
                request.Amount,
                request.Reason,
                request.RefundType
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Appointment Management

    /// <summary>
    /// Get all appointments with filtering and pagination for admin management
    /// </summary>
    /// <param name="request">Appointment filtering and pagination parameters</param>
    /// <returns>Paginated list of appointments with admin details</returns>
    [HttpGet("appointments")]
    [ProducesResponseType(typeof(GetAppointmentsAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAppointmentsAdminResponse>> GetAppointments([FromQuery] GetAppointmentsAdminRequest request)
    {
        try
        {
            var query = new GetAppointmentsAdminQuery(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.DoctorId,
                request.StartDate,
                request.EndDate,
                request.SearchTerm,
                request.SortBy,
                request.SortDirection
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get appointment analytics and statistics
    /// </summary>
    /// <param name="request">Analytics parameters</param>
    /// <returns>Appointment analytics data</returns>
    [HttpGet("appointments/analytics")]
    [ProducesResponseType(typeof(GetAppointmentAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAppointmentAnalyticsResponse>> GetAppointmentAnalytics([FromQuery] GetAppointmentAnalyticsRequest request)
    {
        try
        {
            var query = new GetAppointmentAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                request.GroupBy,
                request.DoctorId
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update appointment status
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated appointment information</returns>
    [HttpPut("appointments/{appointmentId:guid}/status")]
    [ProducesResponseType(typeof(UpdateAppointmentStatusAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateAppointmentStatusAdminResponse>> UpdateAppointmentStatus(Guid appointmentId, [FromBody] UpdateAppointmentStatusAdminRequest request)
    {
        try
        {
            var command = new UpdateAppointmentStatusAdminCommand(
                appointmentId,
                request.Status,
                request.Notes
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Content Management

    /// <summary>
    /// Get localized content list for admin management
    /// </summary>
    /// <param name="request">Content filtering parameters</param>
    /// <returns>List of localized content</returns>
    [HttpGet("content")]
    [ProducesResponseType(typeof(GetLocalizedContentListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetLocalizedContentListResponse>> GetLocalizedContent([FromQuery] GetLocalizedContentListRequest request)
    {
        try
        {
            var query = new GetLocalizedContentListQuery(
                request.Category,
                request.Language,
                request.IsActive,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new localized content
    /// </summary>
    /// <param name="request">Content creation request</param>
    /// <returns>Created content information</returns>
    [HttpPost("content")]
    [ProducesResponseType(typeof(CreateLocalizedContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateLocalizedContentResponse>> CreateLocalizedContent([FromBody] CreateLocalizedContentRequest request)
    {
        try
        {
            var command = new CreateLocalizedContentCommand(
                request.Key,
                request.Category,
                request.EnglishContent,
                request.ArabicContent,
                request.ContentType,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(CreateLocalizedContent), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing localized content
    /// </summary>
    /// <param name="contentId">Content ID</param>
    /// <param name="request">Content update request</param>
    /// <returns>Updated content information</returns>
    [HttpPut("content/{contentId:guid}")]
    [ProducesResponseType(typeof(UpdateLocalizedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateLocalizedContentResponse>> UpdateLocalizedContent(Guid contentId, [FromBody] UpdateLocalizedContentRequest request)
    {
        try
        {
            var command = new UpdateLocalizedContentCommand(
                contentId,
                request.Key,
                request.Category,
                request.EnglishContent,
                request.ArabicContent,
                request.ContentType,
                request.IsActive
            );

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete localized content
    /// </summary>
    /// <param name="contentId">Content ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("content/{contentId:guid}")]
    [ProducesResponseType(typeof(DeleteLocalizedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteLocalizedContentResponse>> DeleteLocalizedContent(Guid contentId)
    {
        try
        {
            var command = new DeleteLocalizedContentCommand(contentId);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bulk update localized content
    /// </summary>
    /// <param name="request">Bulk update request</param>
    /// <returns>Bulk update result</returns>
    [HttpPut("content/bulk")]
    [ProducesResponseType(typeof(BulkUpdateLocalizedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BulkUpdateLocalizedContentResponse>> BulkUpdateLocalizedContent([FromBody] BulkUpdateLocalizedContentRequest request)
    {
        try
        {
            var command = new BulkUpdateLocalizedContentCommand(request.ContentUpdates);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}