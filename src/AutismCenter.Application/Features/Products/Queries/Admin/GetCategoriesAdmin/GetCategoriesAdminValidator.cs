using FluentValidation;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;

public class GetCategoriesAdminValidator : AbstractValidator<GetCategoriesAdminQuery>
{
    public GetCategoriesAdminValidator()
    {
        // No specific validation rules needed for this query
        // All parameters are optional and have sensible defaults
    }
}