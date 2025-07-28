using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;

public class UpdateProductAdminHandler : IRequestHandler<UpdateProductAdminCommand, UpdateProductAdminResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateProductAdminHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<UpdateProductAdminResponse> Handle(UpdateProductAdminCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.ProductId} not found");
        }

        // Verify category exists if it's being changed
        if (product.CategoryId != request.CategoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {request.CategoryId} not found");
            }
        }

        // Create money value object
        var price = Money.Create(request.Price, request.Currency);

        // Update product details
        product.UpdateDetails(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            price);

        // Update stock
        product.UpdateStock(request.StockQuantity);

        // Update images if provided
        if (request.ImageUrls != null)
        {
            // Clear existing images and add new ones
            var currentImages = product.ImageUrls.ToList();
            foreach (var imageUrl in currentImages)
            {
                product.RemoveImage(imageUrl);
            }

            foreach (var imageUrl in request.ImageUrls)
            {
                product.AddImage(imageUrl);
            }
        }

        // Update active status
        if (request.IsActive && !product.IsActive)
        {
            product.Activate();
        }
        else if (!request.IsActive && product.IsActive)
        {
            product.Deactivate();
        }

        await _productRepository.UpdateAsync(product, cancellationToken);

        return new UpdateProductAdminResponse(
            product.Id,
            product.NameEn,
            product.NameAr,
            product.ProductSku,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            product.UpdatedAt);
    }
}