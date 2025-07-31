using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCoursesAdmin;

public record GetCoursesAdminQuery(
    int PageNumber,
    int PageSize,
    bool? IsActive,
    Guid? CategoryId,
    string? SearchTerm,
    string SortBy,
    bool SortDescending
) : IRequest<GetCoursesAdminResponse>;