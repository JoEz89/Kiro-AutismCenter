using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateProgress;

public record UpdateProgressCommand(
    Guid EnrollmentId,
    Guid ModuleId,
    int ProgressPercentage,
    int WatchTimeInSeconds = 0
) : IRequest<UpdateProgressResponse>;