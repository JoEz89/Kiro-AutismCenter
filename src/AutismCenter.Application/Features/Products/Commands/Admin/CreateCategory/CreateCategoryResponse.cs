namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateCategory;

public record CreateCategoryResponse(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive,
    DateTime CreatedAt
);