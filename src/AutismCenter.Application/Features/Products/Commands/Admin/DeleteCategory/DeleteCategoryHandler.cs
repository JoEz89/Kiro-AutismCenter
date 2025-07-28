using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteCategory;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public DeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.CategoryId} not found");
        }

        // Check if category has products
        var hasProducts = await _productRepository.HasProductsInCategoryAsync(request.CategoryId, cancellationToken);
        if (hasProducts)
        {
            throw new ValidationException("Cannot delete category that contains products. Please move or delete all products first.");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);

        return new DeleteCategoryResponse(
            true,
            $"Category '{category.NameEn}' has been successfully deleted");
    }
}