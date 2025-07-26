using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseResponse(
    bool Success,
    string Message,
    CourseDto? Course = null,
    Dictionary<string, string[]>? Errors = null
);