using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;

public class GetUserAnalyticsHandler : IRequestHandler<GetUserAnalyticsQuery, GetUserAnalyticsResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserAnalyticsHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserAnalyticsResponse> Handle(GetUserAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get users within date range
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var filteredUsers = users.Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);

        // Apply role filter
        if (request.Role.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.Role == request.Role.Value);
        }

        var userList = filteredUsers.ToList();

        // Calculate overview analytics
        var overview = CalculateOverviewAnalytics(userList);

        // Calculate role breakdown
        var roleBreakdown = CalculateRoleBreakdown(userList);

        // Calculate monthly trends
        var monthlyTrends = CalculateMonthlyTrends(userList);

        // Calculate language breakdown
        var languageBreakdown = CalculateLanguageBreakdown(userList);

        // Calculate top active users
        var topActiveUsers = CalculateTopActiveUsers(userList);

        return new GetUserAnalyticsResponse(
            overview,
            roleBreakdown,
            monthlyTrends,
            languageBreakdown,
            topActiveUsers);
    }

    private static UserOverviewAnalytics CalculateOverviewAnalytics(IList<Domain.Entities.User> users)
    {
        var totalUsers = users.Count;
        var activeUsers = users.Count(u => u.IsActive);
        var inactiveUsers = totalUsers - activeUsers;
        var verifiedUsers = users.Count(u => u.IsEmailVerified);
        var unverifiedUsers = totalUsers - verifiedUsers;
        var googleUsers = users.Count(u => u.HasGoogleAccount());
        var regularUsers = totalUsers - googleUsers;
        
        var verificationRate = totalUsers > 0 ? (decimal)verifiedUsers / totalUsers : 0;
        var googleAccountRate = totalUsers > 0 ? (decimal)googleUsers / totalUsers : 0;

        return new UserOverviewAnalytics(
            totalUsers,
            activeUsers,
            inactiveUsers,
            verifiedUsers,
            unverifiedUsers,
            googleUsers,
            regularUsers,
            verificationRate,
            googleAccountRate);
    }

    private static IEnumerable<UserRoleAnalytics> CalculateRoleBreakdown(IList<Domain.Entities.User> users)
    {
        var totalUsers = users.Count;

        return users
            .GroupBy(u => u.Role)
            .Select(g => new UserRoleAnalytics(
                g.Key,
                g.Count(),
                totalUsers > 0 ? (double)g.Count() / totalUsers * 100 : 0))
            .OrderByDescending(r => r.Count);
    }

    private static IEnumerable<MonthlyUserAnalytics> CalculateMonthlyTrends(IList<Domain.Entities.User> users)
    {
        return users
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new MonthlyUserAnalytics(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Count())) // In this context, new users = total users for that month
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month);
    }

    private static IEnumerable<UserLanguageAnalytics> CalculateLanguageBreakdown(IList<Domain.Entities.User> users)
    {
        var totalUsers = users.Count;

        return users
            .GroupBy(u => u.PreferredLanguage)
            .Select(g => new UserLanguageAnalytics(
                g.Key,
                g.Count(),
                totalUsers > 0 ? (double)g.Count() / totalUsers * 100 : 0))
            .OrderByDescending(l => l.Count);
    }

    private static IEnumerable<UserActivityAnalytics> CalculateTopActiveUsers(IList<Domain.Entities.User> users)
    {
        return users
            .Select(u => new UserActivityAnalytics(
                u.Id,
                u.GetFullName(),
                u.Email.Value,
                u.Role,
                u.Orders.Count,
                u.Appointments.Count,
                u.Enrollments.Count,
                u.Orders.Sum(o => o.TotalAmount.Amount),
                u.UpdatedAt))
            .OrderByDescending(u => u.OrderCount + u.AppointmentCount + u.EnrollmentCount)
            .Take(10);
    }
}