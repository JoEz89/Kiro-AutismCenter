using MediatR;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateProductAdmin;

public class CreateProductAdminHandler : IRequestHandler<CreateProductAdminCommand, CreateProductAdminResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductAdminHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CreateProductAdminResponse> Handle(CreateProductAdminCommand request, CancellationToken cancellationToken)
    {
        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.CategoryId} not found");
        }

        // Check if product SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(request.ProductSku, cancellationToken);
        if (existingProduct != null)
        {
            throw new ValidationException($"Product with SKU '{request.ProductSku}' already exists");
        }

        // Create money value object
        var price = Money.Create(request.Price, request.Currency);

        // Create product
        var product = Product.Create(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            price,
            request.StockQuantity,
            request.CategoryId,
            request.ProductSku);

        // Add images if provided
        if (request.ImageUrls != null)
        {
            foreach (var imageUrl in request.ImageUrls)
            {
                product.AddImage(imageUrl);
            }
        }

        // Set active status
        if (!request.IsActive)
        {
            product.Deactivate();
        }

        // Save product
        await _productRepository.AddAsync(product, cancellationToken);

        return new CreateProductAdminResponse(
            product.Id,
            product.NameEn,
            product.NameAr,
            product.ProductSku,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAt);
    }
}