using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;

public class GetUserEnrollmentsHandler : IRequestHandler<GetUserEnrollmentsQuery, GetUserEnrollmentsResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserRepository _userRepository;

    public GetUserEnrollmentsHandler(
        IEnrollmentRepository enrollmentRepository,
        IUserRepository userRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _userRepository = userRepository;
    }

    public async Task<GetUserEnrollmentsResponse> Handle(GetUserEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user exists
            var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return new GetUserEnrollmentsResponse(
                    false,
                    "User not found",
                    Enumerable.Empty<EnrollmentDto>(),
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.UserId), new[] { "User with the specified ID does not exist" } }
                    }
                );
            }

            // Get user enrollments
            var enrollments = await _enrollmentRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            // Apply filters
            if (request.ActiveOnly)
            {
                enrollments = enrollments.Where(e => e.IsActive);
            }

            if (!request.IncludeExpired)
            {
                enrollments = enrollments.Where(e => !e.IsExpired());
            }

            var enrollmentDtos = enrollments.Select(EnrollmentDto.FromEntity).ToList();

            return new GetUserEnrollmentsResponse(
                true,
                "User enrollments retrieved successfully",
                enrollmentDtos
            );
        }
        catch (Exception ex)
        {
            return new GetUserEnrollmentsResponse(
                false,
                "An error occurred while retrieving user enrollments",
                Enumerable.Empty<EnrollmentDto>(),
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}