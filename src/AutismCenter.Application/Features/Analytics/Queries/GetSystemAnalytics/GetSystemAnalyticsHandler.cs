using MediatR;
using AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;
using AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;
using AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;
using AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Analytics.Queries.GetSystemAnalytics;

public class GetSystemAnalyticsHandler : IRequestHandler<GetSystemAnalyticsQuery, GetSystemAnalyticsResponse>
{
    private readonly IMediator _mediator;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetSystemAnalyticsHandler(
        IMediator mediator,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _mediator = mediator;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetSystemAnalyticsResponse> Handle(GetSystemAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddMonths(-1);

        // Get analytics from individual modules
        var userAnalyticsTask = request.IncludeUsers ? 
            _mediator.Send(new GetUserAnalyticsQuery(startDate, endDate, null), cancellationToken) : 
            Task.FromResult<GetUserAnalyticsResponse?>(null);

        var productAnalyticsTask = request.IncludeProducts ? 
            _mediator.Send(new GetProductAnalyticsQuery(startDate, endDate, null), cancellationToken) : 
            Task.FromResult<GetProductAnalyticsResponse?>(null);

        var orderAnalyticsTask = request.IncludeOrders ? 
            _mediator.Send(new GetOrderAnalyticsQuery(startDate, endDate, null, null), cancellationToken) : 
            Task.FromResult<GetOrderAnalyticsResponse?>(null);

        var appointmentAnalyticsTask = request.IncludeAppointments ? 
            _mediator.Send(new GetAppointmentAnalyticsQuery(startDate, endDate, "day", null), cancellationToken) : 
            Task.FromResult<GetAppointmentAnalyticsResponse?>(null);

        var courseAnalyticsTask = request.IncludeCourses ? 
            GetCourseAnalytics(startDate, endDate, cancellationToken) : 
            Task.FromResult<GetCourseAnalyticsResponse?>(null);

        await Task.WhenAll(userAnalyticsTask, productAnalyticsTask, orderAnalyticsTask, appointmentAnalyticsTask, courseAnalyticsTask);

        var userAnalytics = await userAnalyticsTask;
        var productAnalytics = await productAnalyticsTask;
        var orderAnalytics = await orderAnalyticsTask;
        var appointmentAnalytics = await appointmentAnalyticsTask;
        var courseAnalytics = await courseAnalyticsTask;

        // Calculate system overview
        var systemOverview = await CalculateSystemOverview(startDate, endDate, userAnalytics, productAnalytics, orderAnalytics, appointmentAnalytics, courseAnalytics, cancellationToken);

        // Calculate cross-module insights
        var crossModuleInsights = CalculateCrossModuleInsights(userAnalytics, productAnalytics, orderAnalytics, appointmentAnalytics, courseAnalytics);

        return new GetSystemAnalyticsResponse(
            systemOverview,
            userAnalytics,
            productAnalytics,
            orderAnalytics,
            appointmentAnalytics,
            courseAnalytics,
            crossModuleInsights
        );
    }

    private async Task<GetCourseAnalyticsResponse> GetCourseAnalytics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var courses = await _courseRepository.GetAllAsync(cancellationToken);
        var enrollments = await _enrollmentRepository.GetEnrollmentsByDateRangeAsync(startDate, endDate, cancellationToken);

        var courseList = courses.ToList();
        var enrollmentList = enrollments.ToList();

        // Calculate overview
        var totalCourses = courseList.Count;
        var activeCourses = courseList.Count(c => c.IsActive);
        var totalEnrollments = enrollmentList.Count;
        var completedEnrollments = enrollmentList.Count(e => e.CompletionDate.HasValue);
        var totalCourseRevenue = enrollmentList.Sum(e => e.Course?.Price?.Amount ?? 0);
        var averageCompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;
        var averageEnrollmentDuration = enrollmentList.Where(e => e.CompletionDate.HasValue)
                                                     .Average(e => (e.CompletionDate!.Value - e.EnrollmentDate).TotalDays);

        var overview = new CourseOverviewAnalytics(
            totalCourses,
            activeCourses,
            totalEnrollments,
            completedEnrollments,
            totalCourseRevenue,
            averageCompletionRate,
            averageEnrollmentDuration
        );

        // Calculate top courses
        var topCourses = courseList.Select(c =>
        {
            var courseEnrollments = enrollmentList.Where(e => e.CourseId == c.Id).ToList();
            var enrollmentCount = courseEnrollments.Count;
            var completionCount = courseEnrollments.Count(e => e.CompletionDate.HasValue);
            var revenue = courseEnrollments.Sum(e => c.Price?.Amount ?? 0);
            var completionRate = enrollmentCount > 0 ? (double)completionCount / enrollmentCount * 100 : 0;

            return new CoursePerformanceAnalytics(
                c.Id,
                c.TitleEn,
                c.TitleAr,
                enrollmentCount,
                completionCount,
                revenue,
                completionRate,
                0 // Average rating - would need rating system
            );
        }).OrderByDescending(c => c.EnrollmentCount).Take(10);

        // Calculate enrollment trends
        var enrollmentTrends = enrollmentList.GroupBy(e => e.EnrollmentDate.Date)
                                           .Select(g => new CourseEnrollmentAnalytics(
                                               g.Key,
                                               g.Count(),
                                               g.Sum(e => e.Course?.Price?.Amount ?? 0)
                                           ))
                                           .OrderBy(t => t.Date);

        // Calculate completion rates
        var completionRates = courseList.Select(c =>
        {
            var courseEnrollments = enrollmentList.Where(e => e.CourseId == c.Id).ToList();
            var totalEnrollments = courseEnrollments.Count;
            var completedEnrollments = courseEnrollments.Count(e => e.CompletionDate.HasValue);
            var completionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;
            var averageCompletionTime = courseEnrollments.Where(e => e.CompletionDate.HasValue)
                                                        .Average(e => (e.CompletionDate!.Value - e.EnrollmentDate).TotalDays);

            return new CourseCompletionAnalytics(
                c.Id,
                c.TitleEn,
                totalEnrollments,
                completedEnrollments,
                completionRate,
                averageCompletionTime
            );
        }).Where(c => c.TotalEnrollments > 0);

        return new GetCourseAnalyticsResponse(
            overview,
            topCourses,
            enrollmentTrends,
            completionRates
        );
    }

    private async Task<SystemOverviewAnalytics> CalculateSystemOverview(
        DateTime startDate, 
        DateTime endDate,
        GetUserAnalyticsResponse? userAnalytics,
        GetProductAnalyticsResponse? productAnalytics,
        GetOrderAnalyticsResponse? orderAnalytics,
        GetAppointmentAnalyticsResponse? appointmentAnalytics,
        GetCourseAnalyticsResponse? courseAnalytics,
        CancellationToken cancellationToken)
    {
        // Calculate total system revenue
        var orderRevenue = orderAnalytics?.Overview.TotalRevenue ?? 0;
        var courseRevenue = courseAnalytics?.Overview.TotalCourseRevenue ?? 0;
        var totalSystemRevenue = orderRevenue + courseRevenue;

        // Calculate total system users
        var totalSystemUsers = userAnalytics?.Overview.TotalUsers ?? 0;

        // Calculate total system transactions
        var totalOrders = orderAnalytics?.Overview.TotalOrders ?? 0;
        var totalEnrollments = courseAnalytics?.Overview.TotalEnrollments ?? 0;
        var totalAppointments = appointmentAnalytics?.TotalAppointments ?? 0;
        var totalSystemTransactions = totalOrders + totalEnrollments + totalAppointments;

        // Calculate system growth rate (simplified)
        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
        var previousOrders = await _orderRepository.GetByDateRangeAsync(previousPeriodStart, startDate, cancellationToken);
        var previousRevenue = previousOrders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                                          .Sum(o => o.TotalAmount.Amount);
        var systemGrowthRate = previousRevenue > 0 ? (double)((totalSystemRevenue - previousRevenue) / previousRevenue * 100) : 0;

        // Determine top performing module
        var modulePerformances = new List<ModulePerformance>
        {
            new("E-commerce", orderRevenue, totalOrders, 0, "Active"),
            new("Courses", courseRevenue, totalEnrollments, 0, "Active"),
            new("Appointments", 0, totalAppointments, 0, "Active")
        };

        var topPerformingModule = modulePerformances.OrderByDescending(m => m.Revenue).First().ModuleName;

        return new SystemOverviewAnalytics(
            startDate,
            endDate,
            totalSystemRevenue,
            totalSystemUsers,
            totalSystemTransactions,
            systemGrowthRate,
            topPerformingModule,
            modulePerformances
        );
    }

    private IEnumerable<CrossModuleAnalytics> CalculateCrossModuleInsights(
        GetUserAnalyticsResponse? userAnalytics,
        GetProductAnalyticsResponse? productAnalytics,
        GetOrderAnalyticsResponse? orderAnalytics,
        GetAppointmentAnalyticsResponse? appointmentAnalytics,
        GetCourseAnalyticsResponse? courseAnalytics)
    {
        var insights = new List<CrossModuleAnalytics>();

        // User engagement across modules
        if (userAnalytics != null && orderAnalytics != null)
        {
            var userOrderRatio = userAnalytics.Overview.TotalUsers > 0 ? 
                (decimal)orderAnalytics.Overview.TotalOrders / userAnalytics.Overview.TotalUsers : 0;
            
            insights.Add(new CrossModuleAnalytics(
                "User Engagement",
                "Average orders per user",
                userOrderRatio,
                userOrderRatio > 1 ? "High" : "Low",
                new[] { "Users", "Orders" }
            ));
        }

        // Revenue distribution
        if (orderAnalytics != null && courseAnalytics != null)
        {
            var totalRevenue = orderAnalytics.Overview.TotalRevenue + courseAnalytics.Overview.TotalCourseRevenue;
            if (totalRevenue > 0)
            {
                var ecommerceShare = orderAnalytics.Overview.TotalRevenue / totalRevenue * 100;
                insights.Add(new CrossModuleAnalytics(
                    "Revenue Distribution",
                    "E-commerce revenue share",
                    ecommerceShare,
                    ecommerceShare > 50 ? "Dominant" : "Balanced",
                    new[] { "Orders", "Courses" }
                ));
            }
        }

        return insights;
    }
}