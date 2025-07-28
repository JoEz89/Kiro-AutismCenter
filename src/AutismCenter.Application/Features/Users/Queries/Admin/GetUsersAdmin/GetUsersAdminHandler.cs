using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;

public class GetUsersAdminHandler : IRequestHandler<GetUsersAdminQuery, GetUsersAdminResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUsersAdminHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUsersAdminResponse> Handle(GetUsersAdminQuery request, CancellationToken cancellationToken)
    {
        // Get all users (in a real implementation, this should be optimized with database-level filtering)
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredUsers = allUsers.AsQueryable();

        if (request.Role.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.Role == request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.IsEmailVerified.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.IsEmailVerified == request.IsEmailVerified.Value);
        }

        // Apply search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLowerInvariant();
            filteredUsers = filteredUsers.Where(u => 
                u.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                u.LastName.ToLowerInvariant().Contains(searchTerm) ||
                u.Email.Value.ToLowerInvariant().Contains(searchTerm));
        }

        // Apply sorting
        filteredUsers = request.SortBy?.ToLowerInvariant() switch
        {
            "email" => request.SortDescending 
                ? filteredUsers.OrderByDescending(u => u.Email.Value)
                : filteredUsers.OrderBy(u => u.Email.Value),
            "firstname" => request.SortDescending 
                ? filteredUsers.OrderByDescending(u => u.FirstName)
                : filteredUsers.OrderBy(u => u.FirstName),
            "lastname" => request.SortDescending 
                ? filteredUsers.OrderByDescending(u => u.LastName)
                : filteredUsers.OrderBy(u => u.LastName),
            "role" => request.SortDescending 
                ? filteredUsers.OrderByDescending(u => u.Role)
                : filteredUsers.OrderBy(u => u.Role),
            _ => request.SortDescending 
                ? filteredUsers.OrderByDescending(u => u.CreatedAt)
                : filteredUsers.OrderBy(u => u.CreatedAt)
        };

        var totalCount = filteredUsers.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Apply pagination
        var pagedUsers = filteredUsers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Convert to DTOs
        var userDtos = pagedUsers.Select(user => new UserAdminDto(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.GetFullName(),
            user.Role,
            user.PreferredLanguage,
            user.IsActive,
            user.IsEmailVerified,
            user.HasGoogleAccount(),
            user.PhoneNumber?.Value,
            user.CreatedAt,
            user.UpdatedAt,
            user.Orders.Count,
            user.Appointments.Count,
            user.Enrollments.Count));

        return new GetUsersAdminResponse(
            userDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}