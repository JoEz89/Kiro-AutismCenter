using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.ValidateCompletion;

public record ValidateCompletionResponse(
    bool IsSuccess,
    string Message,
    bool IsCompleted = false,
    EnrollmentDto? Enrollment = null,
    Dictionary<string, string[]>? ValidationErrors = null
);