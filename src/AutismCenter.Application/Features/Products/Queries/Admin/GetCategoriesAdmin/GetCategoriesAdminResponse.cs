namespace AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;

public record GetCategoriesAdminResponse(
    IEnumerable<CategoryAdminDto> Categories
);

public record CategoryAdminDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive,
    int ProductCount,
    int ActiveProductCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);