using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseResponse(
    bool Success,
    string Message,
    CourseDto? Course = null,
    Dictionary<string, string[]>? Errors = null
);