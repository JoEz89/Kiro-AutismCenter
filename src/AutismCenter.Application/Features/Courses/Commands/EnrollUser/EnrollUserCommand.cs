using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.EnrollUser;

public record EnrollUserCommand(
    Guid UserId,
    Guid CourseId,
    int ValidityDays = 30
) : IRequest<EnrollUserResponse>;