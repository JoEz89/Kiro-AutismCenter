using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Interfaces;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.DeleteCourseAdmin;

public class DeleteCourseAdminHandler : IRequestHandler<DeleteCourseAdminCommand, DeleteCourseAdminResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourseAdminHandler(
        ICourseRepository courseRepository, 
        IEnrollmentRepository enrollmentRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteCourseAdminResponse> Handle(DeleteCourseAdminCommand request, CancellationToken cancellationToken)
    {
        // Get existing course
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            throw new InvalidOperationException($"Course with ID {request.CourseId} not found");
        }

        // Check if course has active enrollments
        var hasActiveEnrollments = await _enrollmentRepository.HasActiveEnrollmentsAsync(request.CourseId, cancellationToken);
        if (hasActiveEnrollments)
        {
            throw new InvalidOperationException("Cannot delete course with active enrollments. Please deactivate the course instead.");
        }

        // Soft delete the course (set IsActive to false)
        course.Deactivate();
        
        await _courseRepository.UpdateAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteCourseAdminResponse(
            course.Id,
            !course.IsActive,
            DateTime.UtcNow
        );
    }
}