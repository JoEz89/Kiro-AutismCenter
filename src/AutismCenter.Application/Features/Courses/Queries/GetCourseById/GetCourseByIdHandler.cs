using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourseById;

public class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, GetCourseByIdResponse>
{
    private readonly ICourseRepository _courseRepository;

    public GetCourseByIdHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<GetCourseByIdResponse> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(request.Id, cancellationToken);

            if (course == null)
            {
                return new GetCourseByIdResponse(
                    false,
                    "Course not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.Id), new[] { "Course with the specified ID does not exist" } }
                    }
                );
            }

            var courseDto = CourseDto.FromEntity(course);

            return new GetCourseByIdResponse(
                true,
                "Course retrieved successfully",
                courseDto
            );
        }
        catch (Exception ex)
        {
            return new GetCourseByIdResponse(
                false,
                "An error occurred while retrieving the course",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}