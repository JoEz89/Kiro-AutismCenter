using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Interfaces;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.UpdateCourseAdmin;

public class UpdateCourseAdminHandler : IRequestHandler<UpdateCourseAdminCommand, UpdateCourseAdminResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseAdminHandler(ICourseRepository courseRepository, IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateCourseAdminResponse> Handle(UpdateCourseAdminCommand request, CancellationToken cancellationToken)
    {
        // Get existing course
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            throw new InvalidOperationException($"Course with ID {request.CourseId} not found");
        }

        // Create price value object
        var price = Money.Create(request.Price, request.Currency);

        // Update course properties
        course.UpdateDetails(
            request.TitleEn,
            request.TitleAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Duration,
            price
        );

        // Set thumbnail if provided
        if (!string.IsNullOrEmpty(request.ThumbnailUrl))
        {
            course.SetThumbnail(request.ThumbnailUrl);
        }

        // Update active status
        if (request.IsActive && !course.IsActive)
        {
            course.Activate();
        }
        else if (!request.IsActive && course.IsActive)
        {
            course.Deactivate();
        }

        // Update in repository
        await _courseRepository.UpdateAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCourseAdminResponse(
            course.Id,
            course.TitleEn,
            course.TitleAr,
            course.IsActive,
            course.UpdatedAt
        );
    }
}