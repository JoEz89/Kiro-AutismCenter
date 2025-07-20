using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        // Check if SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(request.ProductSku, cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException("A product with this SKU already exists");
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
            request.ProductSku
        );

        // Add images if provided
        if (request.ImageUrls != null)
        {
            foreach (var imageUrl in request.ImageUrls)
            {
                product.AddImage(imageUrl);
            }
        }

        // Save product
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load the product with category for response
        var savedProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        
        return new CreateProductResponse(
            ProductDto.FromEntity(savedProduct!),
            "Product created successfully"
        );
    }
}