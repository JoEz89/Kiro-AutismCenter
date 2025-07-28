using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;

public record GetUsersAdminQuery(
    int PageNumber = 1,
    int PageSize = 20,
    UserRole? Role = null,
    bool? IsActive = null,
    bool? IsEmailVerified = null,
    string? SearchTerm = null, // Search by name or email
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<GetUsersAdminResponse>;