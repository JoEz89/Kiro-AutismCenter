using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;

public record UpdateCategoryCommand(
    Guid CategoryId,
    string NameEn,
    string NameAr,
    string? DescriptionEn = null,
    string? DescriptionAr = null,
    bool IsActive = true
) : IRequest<UpdateCategoryResponse>;