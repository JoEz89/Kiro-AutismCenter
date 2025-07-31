namespace AutismCenter.Application.Features.Courses.Queries.Admin.ExportCourses;

public record ExportCoursesResponse(
    byte[] FileContent,
    string ContentType,
    string FileName
);