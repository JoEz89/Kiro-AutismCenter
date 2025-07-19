using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Domain.Services;

public class EnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository, IUserRepository userRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
    }

    public async Task<Enrollment> EnrollUserInCourseAsync(Guid userId, Guid courseId, int validityDays = 30, 
        CancellationToken cancellationToken = default)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Validate course exists and is active
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course == null)
            throw new InvalidOperationException("Course not found");

        if (!course.IsActive)
            throw new InvalidOperationException("Course is not active");

        // Check if user is already enrolled
        var existingEnrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId, cancellationToken);
        if (existingEnrollment != null && existingEnrollment.CanAccess())
            throw new InvalidOperationException("User is already enrolled in this course");

        // Create new enrollment
        var enrollment = Enrollment.CreateEnrollment(userId, courseId, validityDays);

        return enrollment;
    }

    public async Task<bool> CanUserAccessCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId, cancellationToken);
        return enrollment?.CanAccess() ?? false;
    }

    public async Task<bool> ExtendEnrollmentAsync(Guid enrollmentId, int additionalDays, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment == null)
            return false;

        enrollment.ExtendExpiry(additionalDays);
        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Enrollment>> GetExpiringEnrollmentsAsync(int daysBeforeExpiry = 7, 
        CancellationToken cancellationToken = default)
    {
        var allEnrollments = await _enrollmentRepository.GetActiveEnrollmentsAsync(cancellationToken);
        var expiryThreshold = DateTime.UtcNow.AddDays(daysBeforeExpiry);

        return allEnrollments.Where(e => e.ExpiryDate <= expiryThreshold && !e.IsExpired());
    }

    public async Task DeactivateExpiredEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        var expiredEnrollments = await _enrollmentRepository.GetExpiredEnrollmentsAsync(cancellationToken);
        
        foreach (var enrollment in expiredEnrollments.Where(e => e.IsActive))
        {
            enrollment.Deactivate();
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        }
    }

    public double CalculateCompletionRate(Guid courseId, IEnumerable<Enrollment> enrollments)
    {
        var courseEnrollments = enrollments.Where(e => e.CourseId == courseId).ToList();
        if (!courseEnrollments.Any())
            return 0;

        var completedCount = courseEnrollments.Count(e => e.IsCompleted());
        return (double)completedCount / courseEnrollments.Count * 100;
    }

    public async Task<bool> GenerateCertificateAsync(Guid enrollmentId, string certificateUrl, 
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment == null || !enrollment.IsCompleted())
            return false;

        enrollment.GenerateCertificate(certificateUrl);
        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        return true;
    }
}