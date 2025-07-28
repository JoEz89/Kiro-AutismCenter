using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.CategoryId} not found");
        }

        category.UpdateDetails(
            request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr);

        if (request.IsActive && !category.IsActive)
        {
            category.Activate();
        }
        else if (!request.IsActive && category.IsActive)
        {
            category.Deactivate();
        }

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return new UpdateCategoryResponse(
            category.Id,
            category.NameEn,
            category.NameAr,
            category.DescriptionEn,
            category.DescriptionAr,
            category.IsActive,
            category.UpdatedAt);
    }
}