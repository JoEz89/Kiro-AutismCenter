using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutismCenter.Application.Features.Products.Queries.GetProducts;
using AutismCenter.Application.Features.Products.Queries.GetProductById;
using AutismCenter.Application.Features.Products.Queries.SearchProducts;
using AutismCenter.Application.Features.Products.Commands.CreateProduct;
using AutismCenter.Application.Features.Products.Commands.UpdateProduct;
using AutismCenter.Application.Features.Products.Commands.DeleteProduct;
using AutismCenter.Application.Features.Products.Commands.UpdateStock;
using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    /// <summary>
    /// Get products with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetProductsResponse>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? inStockOnly = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetProductsQuery(
            pageNumber, 
            pageSize, 
            categoryId, 
            isActive, 
            inStockOnly, 
            minPrice, 
            maxPrice, 
            sortBy, 
            sortDescending);
            
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetProductByIdResponse>> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query);
        
        if (result?.Product == null)
            return NotFound($"Product with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Search products by term
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<SearchProductsResponse>> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? inStockOnly = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var query = new SearchProductsQuery(
            searchTerm,
            pageNumber,
            pageSize,
            categoryId,
            isActive,
            inStockOnly,
            minPrice,
            maxPrice,
            sortBy,
            sortDescending);
            
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Price,
            request.Currency,
            request.StockQuantity,
            request.CategoryId,
            request.ProductSku,
            request.ImageUrls);
            
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Product.Id }, result);
    }

    /// <summary>
    /// Update an existing product (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UpdateProductResponse>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var command = new UpdateProductCommand(
            id,
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Price,
            request.Currency,
            request.ImageUrls);
            
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update product stock (Admin only)
    /// </summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UpdateStockResponse>> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var command = new UpdateStockCommand(id, request.StockQuantity);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DeleteProductResponse>> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}

public record CreateProductRequest(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    string ProductSku,
    IEnumerable<string>? ImageUrls);

public record UpdateProductRequest(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    IEnumerable<string>? ImageUrls);

public record UpdateStockRequest(int StockQuantity);