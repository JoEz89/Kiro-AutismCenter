using MediatR;
using AutismCenter.Domain.Interfaces;
using System.Text;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.ExportCourses;

public class ExportCoursesHandler : IRequestHandler<ExportCoursesQuery, ExportCoursesResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ExportCoursesHandler(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ExportCoursesResponse> Handle(ExportCoursesQuery request, CancellationToken cancellationToken)
    {
        // Get courses based on filters
        var courses = await _courseRepository.GetCoursesForExportAsync(
            request.IsActive,
            null, // CategoryId not supported yet
            cancellationToken);

        var courseList = courses.ToList();
        var courseIds = courseList.Select(c => c.Id).ToList();

        // Get enrollment statistics
        var enrollmentStats = await _enrollmentRepository.GetEnrollmentStatsByCourseIdsAsync(courseIds, cancellationToken);

        // Generate CSV content
        var csvContent = GenerateCsvContent(courseList, enrollmentStats);
        var fileContent = Encoding.UTF8.GetBytes(csvContent);

        var fileName = $"courses_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var contentType = "text/csv";

        return new ExportCoursesResponse(fileContent, contentType, fileName);
    }

    private string GenerateCsvContent(
        IList<Domain.Entities.Course> courses,
        IEnumerable<Domain.ValueObjects.EnrollmentStats> enrollmentStats)
    {
        var csv = new StringBuilder();
        
        // Add header
        csv.AppendLine("Course ID,Course Code,Title (EN),Title (AR),Description (EN),Description (AR),Duration (Minutes),Price,Currency,Category,Thumbnail URL,Is Active,Enrollment Count,Completion Count,Revenue,Created At,Updated At");

        // Add data rows
        foreach (var course in courses)
        {
            var stats = enrollmentStats.FirstOrDefault(s => s.CourseId == course.Id);
            
            csv.AppendLine($"{course.Id}," +
                          $"\"{course.CourseCode}\"," +
                          $"\"{EscapeCsvValue(course.TitleEn)}\"," +
                          $"\"{EscapeCsvValue(course.TitleAr)}\"," +
                          $"\"{EscapeCsvValue(course.DescriptionEn ?? string.Empty)}\"," +
                          $"\"{EscapeCsvValue(course.DescriptionAr ?? string.Empty)}\"," +
                          $"{course.DurationInMinutes}," +
                          $"{course.Price?.Amount ?? 0}," +
                          $"{course.Price?.Currency ?? "BHD"}," +
                          $"\"\"," + // Category not supported yet
                          $"\"{course.ThumbnailUrl ?? string.Empty}\"," +
                          $"{course.IsActive}," +
                          $"{stats?.EnrollmentCount ?? 0}," +
                          $"{stats?.CompletionCount ?? 0}," +
                          $"{stats?.Revenue ?? 0}," +
                          $"{course.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{course.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return csv.ToString();
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}