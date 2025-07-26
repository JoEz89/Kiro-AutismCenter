using MediatR;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr);

        if (!request.IsActive)
        {
            category.Deactivate();
        }

        await _categoryRepository.AddAsync(category, cancellationToken);

        return new CreateCategoryResponse(
            category.Id,
            category.NameEn,
            category.NameAr,
            category.DescriptionEn,
            category.DescriptionAr,
            category.IsActive,
            category.CreatedAt);
    }
}