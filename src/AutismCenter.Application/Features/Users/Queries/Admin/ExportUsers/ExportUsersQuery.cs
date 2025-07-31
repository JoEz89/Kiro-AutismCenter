using MediatR;

namespace AutismCenter.Application.Features.Users.Queries.Admin.ExportUsers;

public record ExportUsersQuery(
    string? Role,
    bool? IsActive,
    DateTime? StartDate,
    DateTime? EndDate,
    string Format
) : IRequest<ExportUsersResponse>;