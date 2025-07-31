using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCourseAnalytics;

public class GetCourseAnalyticsHandler : IRequestHandler<GetCourseAnalyticsQuery, GetCourseAnalyticsResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetCourseAnalyticsHandler(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetCourseAnalyticsResponse> Handle(GetCourseAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddMonths(-3);

        // Get courses and enrollments
        var courses = await _courseRepository.GetCoursesByCategoryAsync(null, cancellationToken); // CategoryId not supported yet
        var enrollments = await _enrollmentRepository.GetEnrollmentsByDateRangeAsync(startDate, endDate, cancellationToken);

        var courseList = courses.ToList();
        var enrollmentList = enrollments.ToList();

        // Filter enrollments by category if specified (not supported yet)
        // if (request.CategoryId.HasValue)
        // {
        //     enrollmentList = enrollmentList.Where(e => e.Course?.CategoryId == request.CategoryId.Value).ToList();
        // }

        // Calculate overview analytics
        var overview = CalculateOverviewAnalytics(courseList, enrollmentList);

        // Calculate top performing courses
        var topPerformingCourses = CalculateTopPerformingCourses(courseList, enrollmentList);

        // Calculate enrollment trends
        var enrollmentTrends = CalculateEnrollmentTrends(enrollmentList);

        // Calculate completion rates
        var completionRates = CalculateCompletionRates(courseList, enrollmentList);

        return new GetCourseAnalyticsResponse(
            overview,
            topPerformingCourses,
            enrollmentTrends,
            completionRates
        );
    }

    private CourseOverviewAnalytics CalculateOverviewAnalytics(
        IList<Domain.Entities.Course> courses, 
        IList<Domain.Entities.Enrollment> enrollments)
    {
        var totalCourses = courses.Count;
        var activeCourses = courses.Count(c => c.IsActive);
        var totalEnrollments = enrollments.Count;
        var completedEnrollments = enrollments.Count(e => e.CompletionDate.HasValue);
        var totalRevenue = enrollments.Sum(e => e.Course?.Price?.Amount ?? 0);
        var averageCompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;
        
        var completedEnrollmentsWithDuration = enrollments.Where(e => e.CompletionDate.HasValue).ToList();
        var averageEnrollmentDuration = completedEnrollmentsWithDuration.Any() 
            ? completedEnrollmentsWithDuration.Average(e => (e.CompletionDate!.Value - e.EnrollmentDate).TotalDays)
            : 0;

        return new CourseOverviewAnalytics(
            totalCourses,
            activeCourses,
            totalEnrollments,
            completedEnrollments,
            totalRevenue,
            averageCompletionRate,
            averageEnrollmentDuration
        );
    }

    private IEnumerable<CoursePerformanceAnalytics> CalculateTopPerformingCourses(
        IList<Domain.Entities.Course> courses, 
        IList<Domain.Entities.Enrollment> enrollments)
    {
        return courses.Select(course =>
        {
            var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();
            var enrollmentCount = courseEnrollments.Count;
            var completionCount = courseEnrollments.Count(e => e.CompletionDate.HasValue);
            var revenue = courseEnrollments.Sum(e => course.Price?.Amount ?? 0);
            var completionRate = enrollmentCount > 0 ? (double)completionCount / enrollmentCount * 100 : 0;

            return new CoursePerformanceAnalytics(
                course.Id,
                course.TitleEn,
                course.TitleAr,
                enrollmentCount,
                completionCount,
                revenue,
                completionRate
            );
        })
        .OrderByDescending(c => c.Revenue)
        .Take(10);
    }

    private IEnumerable<CourseEnrollmentTrend> CalculateEnrollmentTrends(IList<Domain.Entities.Enrollment> enrollments)
    {
        return enrollments
            .GroupBy(e => e.EnrollmentDate.Date)
            .Select(g => new CourseEnrollmentTrend(
                g.Key,
                g.Count(),
                g.Sum(e => e.Course?.Price?.Amount ?? 0)
            ))
            .OrderBy(t => t.Date);
    }

    private IEnumerable<CourseCompletionAnalytics> CalculateCompletionRates(
        IList<Domain.Entities.Course> courses, 
        IList<Domain.Entities.Enrollment> enrollments)
    {
        return courses
            .Select(course =>
            {
                var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();
                var totalEnrollments = courseEnrollments.Count;
                var completedEnrollments = courseEnrollments.Count(e => e.CompletionDate.HasValue);
                var completionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;
                
                var completedWithDuration = courseEnrollments.Where(e => e.CompletionDate.HasValue).ToList();
                var averageCompletionTime = completedWithDuration.Any()
                    ? completedWithDuration.Average(e => (e.CompletionDate!.Value - e.EnrollmentDate).TotalDays)
                    : 0;

                return new CourseCompletionAnalytics(
                    course.Id,
                    course.TitleEn,
                    totalEnrollments,
                    completedEnrollments,
                    completionRate,
                    averageCompletionTime
                );
            })
            .Where(c => c.TotalEnrollments > 0)
            .OrderByDescending(c => c.CompletionRate);
    }
}