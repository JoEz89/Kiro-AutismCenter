using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid Id) : IRequest<GetCourseByIdResponse>;