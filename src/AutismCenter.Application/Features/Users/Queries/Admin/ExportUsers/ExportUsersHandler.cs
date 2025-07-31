using MediatR;
using AutismCenter.Domain.Interfaces;
using System.Text;

namespace AutismCenter.Application.Features.Users.Queries.Admin.ExportUsers;

public class ExportUsersHandler : IRequestHandler<ExportUsersQuery, ExportUsersResponse>
{
    private readonly IUserRepository _userRepository;

    public ExportUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ExportUsersResponse> Handle(ExportUsersQuery request, CancellationToken cancellationToken)
    {
        // Get users based on filters
        var users = await _userRepository.GetUsersForExportAsync(
            request.Role,
            request.IsActive,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        // Generate CSV content
        var csvContent = GenerateCsvContent(users);
        var fileContent = Encoding.UTF8.GetBytes(csvContent);

        var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var contentType = "text/csv";

        return new ExportUsersResponse(fileContent, contentType, fileName);
    }

    private string GenerateCsvContent(IEnumerable<Domain.Entities.User> users)
    {
        var csv = new StringBuilder();
        
        // Add header
        csv.AppendLine("User ID,Email,First Name,Last Name,Role,Preferred Language,Is Active,Is Email Verified,Google ID,Created At");

        // Add data rows
        foreach (var user in users)
        {
            csv.AppendLine($"{user.Id}," +
                          $"\"{user.Email.Value}\"," +
                          $"\"{EscapeCsvValue(user.FirstName)}\"," +
                          $"\"{EscapeCsvValue(user.LastName)}\"," +
                          $"{user.Role}," +
                          $"{user.PreferredLanguage}," +
                          $"{user.IsActive}," +
                          $"{user.IsEmailVerified}," +
                          $"\"{user.GoogleId ?? string.Empty}\"," +
                          $"{user.CreatedAt:yyyy-MM-dd HH:mm:ss}");
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