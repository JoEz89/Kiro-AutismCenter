using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;

public record ExtendEnrollmentCommand(
    Guid EnrollmentId,
    int AdditionalDays
) : IRequest<ExtendEnrollmentResponse>;