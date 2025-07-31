using MediatR;
using AutismCenter.Domain.Interfaces;
using System.Text;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.ExportAppointments;

public class ExportAppointmentsHandler : IRequestHandler<ExportAppointmentsQuery, ExportAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public ExportAppointmentsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ExportAppointmentsResponse> Handle(ExportAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Get appointments based on filters
        var appointments = await _appointmentRepository.GetAppointmentsForExportAsync(
            request.StartDate,
            request.EndDate,
            request.Status,
            request.DoctorId,
            cancellationToken);

        // Generate CSV content
        var csvContent = GenerateCsvContent(appointments);
        var fileContent = Encoding.UTF8.GetBytes(csvContent);

        var fileName = $"appointments_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var contentType = "text/csv";

        return new ExportAppointmentsResponse(fileContent, contentType, fileName);
    }

    private string GenerateCsvContent(IEnumerable<Domain.Entities.Appointment> appointments)
    {
        var csv = new StringBuilder();
        
        // Add header
        csv.AppendLine("Appointment ID,Appointment Number,User Email,User Name,Doctor Name,Appointment Date,Status,Zoom Link,Patient Info,Notes,Created At,Updated At");

        // Add data rows
        foreach (var appointment in appointments)
        {
            var userName = $"{appointment.User?.FirstName} {appointment.User?.LastName}".Trim();
            var doctorName = appointment.Doctor?.NameEn ?? string.Empty;
            var patientInfo = appointment.PatientInfo != null ? 
                $"Name: {appointment.PatientInfo.PatientName}, Age: {appointment.PatientInfo.PatientAge}, Concerns: {appointment.PatientInfo.CurrentConcerns ?? "None"}" : 
                string.Empty;
            
            csv.AppendLine($"{appointment.Id}," +
                          $"\"{appointment.AppointmentNumber}\"," +
                          $"\"{appointment.User?.Email.Value ?? string.Empty}\"," +
                          $"\"{EscapeCsvValue(userName)}\"," +
                          $"\"{EscapeCsvValue(doctorName)}\"," +
                          $"{appointment.AppointmentDate:yyyy-MM-dd HH:mm:ss}," +
                          $"{appointment.Status}," +
                          $"\"{appointment.ZoomJoinUrl ?? string.Empty}\"," +
                          $"\"{EscapeCsvValue(patientInfo)}\"," +
                          $"\"{EscapeCsvValue(appointment.Notes ?? string.Empty)}\"," +
                          $"{appointment.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{appointment.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
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