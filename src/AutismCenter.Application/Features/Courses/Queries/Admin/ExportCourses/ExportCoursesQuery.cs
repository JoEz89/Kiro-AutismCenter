using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.ExportCourses;

public record ExportCoursesQuery(
    bool? IsActive,
    Guid? CategoryId,
    string Format
) : IRequest<ExportCoursesResponse>;