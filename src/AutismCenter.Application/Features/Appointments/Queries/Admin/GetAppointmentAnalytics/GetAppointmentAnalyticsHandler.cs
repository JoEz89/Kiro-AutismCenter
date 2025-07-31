using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;

public class GetAppointmentAnalyticsHandler : IRequestHandler<GetAppointmentAnalyticsQuery, GetAppointmentAnalyticsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;

    public GetAppointmentAnalyticsHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<GetAppointmentAnalyticsResponse> Handle(GetAppointmentAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get all appointments in the date range
        var appointments = await _appointmentRepository.GetAppointmentsByDateRangeAsync(
            startDate, endDate, request.DoctorId, cancellationToken);

        // Calculate overall statistics
        var totalAppointments = appointments.Count();
        var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var upcomingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentDate > DateTime.UtcNow);

        var completionRate = totalAppointments > 0 ? (decimal)completedAppointments / totalAppointments * 100 : 0;
        var cancellationRate = totalAppointments > 0 ? (decimal)cancelledAppointments / totalAppointments * 100 : 0;

        // Group data points by the specified period
        var dataPoints = GroupAppointmentsByPeriod(appointments, request.GroupBy, startDate, endDate);

        // Get doctor statistics
        var doctorStats = await GetDoctorStatistics(appointments, cancellationToken);

        return new GetAppointmentAnalyticsResponse(
            totalAppointments,
            completedAppointments,
            cancelledAppointments,
            upcomingAppointments,
            completionRate,
            cancellationRate,
            dataPoints,
            doctorStats
        );
    }

    private IEnumerable<AppointmentAnalyticsDataPoint> GroupAppointmentsByPeriod(
        IEnumerable<Domain.Entities.Appointment> appointments, 
        string groupBy, 
        DateTime startDate, 
        DateTime endDate)
    {
        var dataPoints = new List<AppointmentAnalyticsDataPoint>();

        switch (groupBy.ToLower())
        {
            case "day":
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dayAppointments = appointments.Where(a => a.AppointmentDate.Date == date).ToList();
                    dataPoints.Add(new AppointmentAnalyticsDataPoint(
                        date,
                        dayAppointments.Count,
                        dayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                        dayAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                        date.ToString("yyyy-MM-dd")
                    ));
                }
                break;

            case "week":
                var startOfWeek = startDate.Date.AddDays(-(int)startDate.DayOfWeek);
                for (var date = startOfWeek; date <= endDate; date = date.AddDays(7))
                {
                    var weekEnd = date.AddDays(6);
                    var weekAppointments = appointments.Where(a => 
                        a.AppointmentDate.Date >= date && a.AppointmentDate.Date <= weekEnd).ToList();
                    
                    dataPoints.Add(new AppointmentAnalyticsDataPoint(
                        date,
                        weekAppointments.Count,
                        weekAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                        weekAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                        $"Week of {date:yyyy-MM-dd}"
                    ));
                }
                break;

            case "month":
                var startOfMonth = new DateTime(startDate.Year, startDate.Month, 1);
                for (var date = startOfMonth; date <= endDate; date = date.AddMonths(1))
                {
                    var monthEnd = date.AddMonths(1).AddDays(-1);
                    var monthAppointments = appointments.Where(a => 
                        a.AppointmentDate.Date >= date && a.AppointmentDate.Date <= monthEnd).ToList();
                    
                    dataPoints.Add(new AppointmentAnalyticsDataPoint(
                        date,
                        monthAppointments.Count,
                        monthAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                        monthAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                        date.ToString("yyyy-MM")
                    ));
                }
                break;
        }

        return dataPoints.OrderBy(dp => dp.Date);
    }

    private async Task<IEnumerable<DoctorAppointmentStats>> GetDoctorStatistics(
        IEnumerable<Domain.Entities.Appointment> appointments, 
        CancellationToken cancellationToken)
    {
        var doctorGroups = appointments.GroupBy(a => a.DoctorId);
        var doctorStats = new List<DoctorAppointmentStats>();

        foreach (var group in doctorGroups)
        {
            var doctor = await _doctorRepository.GetByIdAsync(group.Key, cancellationToken);
            if (doctor != null)
            {
                var doctorAppointments = group.ToList();
                var totalAppointments = doctorAppointments.Count;
                var completedAppointments = doctorAppointments.Count(a => a.Status == AppointmentStatus.Completed);
                var cancelledAppointments = doctorAppointments.Count(a => a.Status == AppointmentStatus.Cancelled);
                var completionRate = totalAppointments > 0 ? (decimal)completedAppointments / totalAppointments * 100 : 0;

                doctorStats.Add(new DoctorAppointmentStats(
                    doctor.Id,
                    doctor.NameEn,
                    totalAppointments,
                    completedAppointments,
                    cancelledAppointments,
                    completionRate
                ));
            }
        }

        return doctorStats.OrderByDescending(ds => ds.TotalAppointments);
    }
}