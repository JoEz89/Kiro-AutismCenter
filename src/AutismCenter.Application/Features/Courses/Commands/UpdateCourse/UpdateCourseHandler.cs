using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, UpdateCourseResponse>
{
    private readonly ICourseRepository _courseRepository;

    public UpdateCourseHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<UpdateCourseResponse> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing course
            var course = await _courseRepository.GetByIdAsync(request.Id, cancellationToken);
            if (course == null)
            {
                return new UpdateCourseResponse(
                    false,
                    "Course not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.Id), new[] { "Course with the specified ID does not exist" } }
                    }
                );
            }

            // Create money value object
            var price = Money.Create(request.Price, request.Currency);

            // Update course details
            course.UpdateDetails(
                request.TitleEn,
                request.TitleAr,
                request.DescriptionEn,
                request.DescriptionAr,
                request.DurationInMinutes,
                price
            );

            // Update thumbnail if provided
            if (!string.IsNullOrWhiteSpace(request.ThumbnailUrl))
            {
                course.SetThumbnail(request.ThumbnailUrl);
            }

            // Save changes
            await _courseRepository.UpdateAsync(course, cancellationToken);

            var courseDto = CourseDto.FromEntity(course);

            return new UpdateCourseResponse(
                true,
                "Course updated successfully",
                courseDto
            );
        }
        catch (ArgumentException ex)
        {
            return new UpdateCourseResponse(
                false,
                "Course update failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new UpdateCourseResponse(
                false,
                "An unexpected error occurred while updating the course",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}