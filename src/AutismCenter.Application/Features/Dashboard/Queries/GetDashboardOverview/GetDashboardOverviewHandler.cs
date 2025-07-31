using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Dashboard.Queries.GetDashboardOverview;

public class GetDashboardOverviewHandler : IRequestHandler<GetDashboardOverviewQuery, GetDashboardOverviewResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetDashboardOverviewHandler(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IAppointmentRepository appointmentRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _appointmentRepository = appointmentRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetDashboardOverviewResponse> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddMonths(-1);

        // Get all data in parallel
        var usersTask = _userRepository.GetAllAsync(cancellationToken);
        var productsTask = _productRepository.GetAllAsync(cancellationToken);
        var ordersTask = _orderRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        var appointmentsTask = _appointmentRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        var coursesTask = _courseRepository.GetAllAsync(cancellationToken);
        var enrollmentsTask = _enrollmentRepository.GetEnrollmentsByDateRangeAsync(startDate, endDate, cancellationToken);

        await Task.WhenAll(usersTask, productsTask, ordersTask, appointmentsTask, coursesTask, enrollmentsTask);

        var users = usersTask.Result.ToList();
        var products = productsTask.Result.ToList();
        var orders = ordersTask.Result.ToList();
        var appointments = appointmentsTask.Result.ToList();
        var courses = coursesTask.Result.ToList();
        var enrollments = enrollmentsTask.Result.ToList();

        // Calculate metrics
        var userMetrics = CalculateUserMetrics(users, startDate, endDate);
        var productMetrics = CalculateProductMetrics(products);
        var orderMetrics = CalculateOrderMetrics(orders);
        var appointmentMetrics = CalculateAppointmentMetrics(appointments);
        var courseMetrics = CalculateCourseMetrics(courses, enrollments);

        // Calculate overview metrics
        var totalRevenue = orders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                                .Sum(o => o.TotalAmount.Amount);
        var totalUsers = users.Count;
        var totalOrders = orders.Count;
        var totalAppointments = appointments.Count;
        var totalCourses = courses.Count;
        var totalProducts = products.Count;

        // Calculate growth rate (simplified - comparing with previous period)
        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
        var previousOrders = await _orderRepository.GetByDateRangeAsync(previousPeriodStart, startDate, cancellationToken);
        var previousRevenue = previousOrders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                                          .Sum(o => o.TotalAmount.Amount);
        var growthRate = previousRevenue > 0 ? (double)((totalRevenue - previousRevenue) / previousRevenue * 100) : 0;

        var overview = new DashboardOverviewMetrics(
            totalRevenue,
            totalUsers,
            totalOrders,
            totalAppointments,
            totalCourses,
            totalProducts,
            growthRate
        );

        // Get recent activity
        var recentActivity = await GetRecentActivity(cancellationToken);

        return new GetDashboardOverviewResponse(
            overview,
            userMetrics,
            productMetrics,
            orderMetrics,
            appointmentMetrics,
            courseMetrics,
            recentActivity
        );
    }

    private UserMetrics CalculateUserMetrics(IList<Domain.Entities.User> users, DateTime startDate, DateTime endDate)
    {
        var totalUsers = users.Count;
        var activeUsers = users.Count(u => u.IsActive);
        var newUsersThisMonth = users.Count(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
        
        // Calculate growth rate
        var previousMonthStart = startDate.AddMonths(-1);
        var previousMonthUsers = users.Count(u => u.CreatedAt >= previousMonthStart && u.CreatedAt < startDate);
        var userGrowthRate = previousMonthUsers > 0 ? (double)(newUsersThisMonth - previousMonthUsers) / previousMonthUsers * 100 : 0;

        return new UserMetrics(totalUsers, activeUsers, newUsersThisMonth, userGrowthRate);
    }

    private ProductMetrics CalculateProductMetrics(IList<Domain.Entities.Product> products)
    {
        var totalProducts = products.Count;
        var activeProducts = products.Count(p => p.IsActive);
        var lowStockProducts = products.Count(p => p.StockQuantity <= 10 && p.StockQuantity > 0);
        var outOfStockProducts = products.Count(p => p.StockQuantity == 0);

        return new ProductMetrics(totalProducts, activeProducts, lowStockProducts, outOfStockProducts);
    }

    private OrderMetrics CalculateOrderMetrics(IList<Domain.Entities.Order> orders)
    {
        var totalOrders = orders.Count;
        var totalRevenue = orders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                                .Sum(o => o.TotalAmount.Amount);
        var pendingOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Pending);
        var completedOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
        var averageOrderValue = completedOrders > 0 ? (double)(totalRevenue / completedOrders) : 0;

        return new OrderMetrics(totalOrders, totalRevenue, pendingOrders, completedOrders, averageOrderValue);
    }

    private AppointmentMetrics CalculateAppointmentMetrics(IList<Domain.Entities.Appointment> appointments)
    {
        var totalAppointments = appointments.Count;
        var upcomingAppointments = appointments.Count(a => a.AppointmentDate > DateTime.UtcNow && 
                                                          a.Status == Domain.Enums.AppointmentStatus.Confirmed);
        var completedAppointments = appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Cancelled);
        var bookingRate = totalAppointments > 0 ? (double)completedAppointments / totalAppointments * 100 : 0;

        return new AppointmentMetrics(totalAppointments, upcomingAppointments, completedAppointments, cancelledAppointments, bookingRate);
    }

    private CourseMetrics CalculateCourseMetrics(IList<Domain.Entities.Course> courses, IList<Domain.Entities.Enrollment> enrollments)
    {
        var totalCourses = courses.Count;
        var activeCourses = courses.Count(c => c.IsActive);
        var totalEnrollments = enrollments.Count;
        var completedCourses = enrollments.Count(e => e.CompletionDate.HasValue);
        var completionRate = totalEnrollments > 0 ? (double)completedCourses / totalEnrollments * 100 : 0;

        return new CourseMetrics(totalCourses, activeCourses, totalEnrollments, completedCourses, completionRate);
    }

    private async Task<RecentActivity> GetRecentActivity(CancellationToken cancellationToken)
    {
        // Get recent orders, appointments, and enrollments
        var recentOrders = await _orderRepository.GetRecentOrdersAsync(10, cancellationToken);
        var recentAppointments = await _appointmentRepository.GetRecentAppointmentsAsync(10, cancellationToken);
        var recentEnrollments = await _enrollmentRepository.GetRecentEnrollmentsAsync(10, cancellationToken);

        var activities = new List<ActivityItem>();

        // Add order activities
        activities.AddRange(recentOrders.Select(o => new ActivityItem(
            "Order",
            $"New order #{o.OrderNumber} placed",
            o.CreatedAt,
            o.UserId.ToString(),
            $"{o.User?.FirstName} {o.User?.LastName}"
        )));

        // Add appointment activities
        activities.AddRange(recentAppointments.Select(a => new ActivityItem(
            "Appointment",
            $"Appointment booked for {a.AppointmentDate:MMM dd, yyyy}",
            a.CreatedAt,
            a.UserId.ToString(),
            $"{a.User?.FirstName} {a.User?.LastName}"
        )));

        // Add enrollment activities
        activities.AddRange(recentEnrollments.Select(e => new ActivityItem(
            "Enrollment",
            $"User enrolled in course: {e.Course?.TitleEn}",
            e.EnrollmentDate,
            e.UserId.ToString(),
            $"{e.User?.FirstName} {e.User?.LastName}"
        )));

        // Sort by timestamp and take top 20
        var sortedActivities = activities.OrderByDescending(a => a.Timestamp).Take(20);

        return new RecentActivity(sortedActivities);
    }
}