using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;

public record GetCategoriesAdminQuery(
    bool? IsActive = null,
    bool IncludeProductCount = true
) : IRequest<GetCategoriesAdminResponse>;