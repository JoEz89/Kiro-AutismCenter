using MediatR;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.EnrollUser;

public class EnrollUserHandler : IRequestHandler<EnrollUserCommand, EnrollUserResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;

    public EnrollUserHandler(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository,
        IUserRepository userRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
    }

    public async Task<EnrollUserResponse> Handle(EnrollUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user exists
            var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return new EnrollUserResponse(
                    false,
                    "User not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.UserId), new[] { "User with the specified ID does not exist" } }
                    }
                );
            }

            // Validate course exists and is active
            var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
            if (course == null)
            {
                return new EnrollUserResponse(
                    false,
                    "Course not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.CourseId), new[] { "Course with the specified ID does not exist" } }
                    }
                );
            }

            if (!course.IsActive)
            {
                return new EnrollUserResponse(
                    false,
                    "Course is not available for enrollment",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.CourseId), new[] { "Course is currently inactive" } }
                    }
                );
            }

            // Check if user is already enrolled in an active enrollment
            var existingEnrollment = await _enrollmentRepository.GetByUserAndCourseAsync(
                request.UserId, request.CourseId, cancellationToken);

            if (existingEnrollment != null && existingEnrollment.CanAccess())
            {
                return new EnrollUserResponse(
                    false,
                    "User is already enrolled in this course",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "Enrollment", new[] { "User already has an active enrollment for this course" } }
                    }
                );
            }

            // Create new enrollment
            var enrollment = Enrollment.CreateEnrollment(request.UserId, request.CourseId, request.ValidityDays);

            // Save enrollment
            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

            // Load course information for DTO
            enrollment.GetType().GetProperty("Course")?.SetValue(enrollment, course);

            var enrollmentDto = EnrollmentDto.FromEntity(enrollment);

            return new EnrollUserResponse(
                true,
                "User enrolled successfully",
                enrollmentDto
            );
        }
        catch (ArgumentException ex)
        {
            return new EnrollUserResponse(
                false,
                "Enrollment failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new EnrollUserResponse(
                false,
                "An unexpected error occurred during enrollment",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}