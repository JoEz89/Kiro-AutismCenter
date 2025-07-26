using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.ValidateCompletion;

public record ValidateCompletionCommand(Guid EnrollmentId) : IRequest<ValidateCompletionResponse>;