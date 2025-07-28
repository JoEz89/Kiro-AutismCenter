namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentCategories;

public record GetContentCategoriesResponse(
    IEnumerable<string> Categories
);