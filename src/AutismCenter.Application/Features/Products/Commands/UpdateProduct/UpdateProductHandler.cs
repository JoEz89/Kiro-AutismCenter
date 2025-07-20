using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Create money value object
        var price = Money.Create(request.Price, request.Currency);

        // Update product details
        product.UpdateDetails(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            price
        );

        // Update images if provided
        if (request.ImageUrls != null)
        {
            // Remove all existing images
            var existingImages = product.ImageUrls.ToList();
            foreach (var existingImage in existingImages)
            {
                product.RemoveImage(existingImage);
            }

            // Add new images
            foreach (var imageUrl in request.ImageUrls)
            {
                product.AddImage(imageUrl);
            }
        }

        // Save changes
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load updated product with category for response
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);

        return new UpdateProductResponse(
            ProductDto.FromEntity(updatedProduct!),
            "Product updated successfully"
        );
    }
}