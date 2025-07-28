namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;

public record UpdateCategoryResponse(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive,
    DateTime UpdatedAt
);