using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateCategory;

public record CreateCategoryCommand(
    string NameEn,
    string NameAr,
    string? DescriptionEn = null,
    string? DescriptionAr = null,
    bool IsActive = true
) : IRequest<CreateCategoryResponse>;