using MediatR;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, CreateCourseResponse>
{
    private readonly ICourseRepository _courseRepository;

    public CreateCourseHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<CreateCourseResponse> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if course code already exists
            if (await _courseRepository.CodeExistsAsync(request.CourseCode, cancellationToken))
            {
                return new CreateCourseResponse(
                    false,
                    "Course creation failed",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.CourseCode), new[] { "Course code already exists" } }
                    }
                );
            }

            // Create money value object
            var price = Money.Create(request.Price, request.Currency);

            // Create course entity
            var course = Course.Create(
                request.TitleEn,
                request.TitleAr,
                request.DescriptionEn,
                request.DescriptionAr,
                request.DurationInMinutes,
                price,
                request.CourseCode
            );

            // Set thumbnail if provided
            if (!string.IsNullOrWhiteSpace(request.ThumbnailUrl))
            {
                course.SetThumbnail(request.ThumbnailUrl);
            }

            // Save to repository
            await _courseRepository.AddAsync(course, cancellationToken);

            var courseDto = CourseDto.FromEntity(course);

            return new CreateCourseResponse(
                true,
                "Course created successfully",
                courseDto
            );
        }
        catch (ArgumentException ex)
        {
            return new CreateCourseResponse(
                false,
                "Course creation failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new CreateCourseResponse(
                false,
                "An unexpected error occurred while creating the course",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}