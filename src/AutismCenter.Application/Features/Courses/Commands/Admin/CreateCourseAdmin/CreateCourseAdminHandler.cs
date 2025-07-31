using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Interfaces;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.CreateCourseAdmin;

public class CreateCourseAdminHandler : IRequestHandler<CreateCourseAdminCommand, CreateCourseAdminResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseAdminHandler(ICourseRepository courseRepository, IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCourseAdminResponse> Handle(CreateCourseAdminCommand request, CancellationToken cancellationToken)
    {
        // Create price value object
        var price = Money.Create(request.Price, request.Currency);

        // Create course entity
        var course = Course.Create(
            request.TitleEn,
            request.TitleAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.Duration,
            price,
            request.CourseCode
        );

        // Set thumbnail if provided
        if (!string.IsNullOrEmpty(request.ThumbnailUrl))
        {
            course.SetThumbnail(request.ThumbnailUrl);
        }

        // Set active status
        if (!request.IsActive)
        {
            course.Deactivate();
        }

        // Add to repository
        await _courseRepository.AddAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCourseAdminResponse(
            course.Id,
            course.TitleEn,
            course.TitleAr,
            course.CourseCode,
            course.IsActive,
            course.CreatedAt
        );
    }
}