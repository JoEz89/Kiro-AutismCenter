using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.DeleteCourseAdmin;

public record DeleteCourseAdminCommand(
    Guid CourseId
) : IRequest<DeleteCourseAdminResponse>;