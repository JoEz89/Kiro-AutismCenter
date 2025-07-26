using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesHandler : IRequestHandler<GetCoursesQuery, GetCoursesResponse>
{
    private readonly ICourseRepository _courseRepository;

    public GetCoursesHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<GetCoursesResponse> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.Course> courses;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                // Search courses by term
                courses = await _courseRepository.SearchAsync(request.SearchTerm, cancellationToken);
                
                // Filter by active status if requested
                if (request.ActiveOnly)
                {
                    courses = courses.Where(c => c.IsActive);
                }
            }
            else if (request.ActiveOnly)
            {
                // Get only active courses
                courses = await _courseRepository.GetActiveAsync(cancellationToken);
            }
            else
            {
                // Get all courses
                courses = await _courseRepository.GetAllAsync(cancellationToken);
            }

            var courseDtos = courses.Select(CourseSummaryDto.FromEntity).ToList();

            return new GetCoursesResponse(
                true,
                "Courses retrieved successfully",
                courseDtos
            );
        }
        catch (Exception ex)
        {
            return new GetCoursesResponse(
                false,
                "An error occurred while retrieving courses",
                Enumerable.Empty<CourseSummaryDto>(),
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}