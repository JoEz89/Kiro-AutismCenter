using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Models;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCoursesAdmin;

public class GetCoursesAdminHandler : IRequestHandler<GetCoursesAdminQuery, GetCoursesAdminResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetCoursesAdminHandler(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetCoursesAdminResponse> Handle(GetCoursesAdminQuery request, CancellationToken cancellationToken)
    {
        var (courseItems, totalCount) = await _courseRepository.GetCoursesWithDetailsAsync(
            request.PageNumber,
            request.PageSize,
            request.IsActive,
            null, // CategoryId not supported yet
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var courseList = courseItems.ToList();
        var courseIds = courseList.Select(c => c.Id).ToList();

        // Get enrollment statistics for all courses
        var enrollmentStats = await _enrollmentRepository.GetEnrollmentStatsByCourseIdsAsync(courseIds, cancellationToken);

        var courseDtos = courseList.Select(course =>
        {
            var stats = enrollmentStats.FirstOrDefault(s => s.CourseId == course.Id);
            
            return new CourseAdminDto(
                course.Id,
                course.TitleEn,
                course.TitleAr,
                course.DescriptionEn ?? string.Empty,
                course.DescriptionAr ?? string.Empty,
                course.DurationInMinutes,
                course.Price?.Amount ?? 0,
                course.Price?.Currency ?? "BHD",
                course.CourseCode,
                course.ThumbnailUrl,
                course.IsActive,
                stats?.EnrollmentCount ?? 0,
                stats?.CompletionCount ?? 0,
                stats?.Revenue ?? 0,
                course.CreatedAt,
                course.UpdatedAt
            );
        }).ToList();

        var paginatedResult = new PagedResult<CourseAdminDto>(
            courseDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return new GetCoursesAdminResponse(paginatedResult);
    }
}