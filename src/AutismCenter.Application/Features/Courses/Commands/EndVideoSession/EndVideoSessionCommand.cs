using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.EndVideoSession;

public record EndVideoSessionCommand(string SessionId) : IRequest;