using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;

public class GetCategoriesAdminHandler : IRequestHandler<GetCategoriesAdminQuery, GetCategoriesAdminResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public GetCategoriesAdminHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<GetCategoriesAdminResponse> Handle(GetCategoriesAdminQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        // Apply filters
        if (request.IsActive.HasValue)
        {
            categories = categories.Where(c => c.IsActive == request.IsActive.Value);
        }

        var categoryDtos = new List<CategoryAdminDto>();

        foreach (var category in categories)
        {
            int productCount = 0;
            int activeProductCount = 0;

            if (request.IncludeProductCount)
            {
                var products = await _productRepository.GetByCategoryIdAsync(category.Id, cancellationToken);
                productCount = products.Count();
                activeProductCount = products.Count(p => p.IsActive);
            }

            categoryDtos.Add(new CategoryAdminDto(
                category.Id,
                category.NameEn,
                category.NameAr,
                category.DescriptionEn,
                category.DescriptionAr,
                category.IsActive,
                productCount,
                activeProductCount,
                category.CreatedAt,
                category.UpdatedAt));
        }

        return new GetCategoriesAdminResponse(categoryDtos.OrderBy(c => c.NameEn));
    }
}