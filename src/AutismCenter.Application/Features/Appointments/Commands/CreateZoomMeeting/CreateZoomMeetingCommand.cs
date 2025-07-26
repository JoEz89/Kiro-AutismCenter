using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CreateZoomMeeting;

public record CreateZoomMeetingCommand(Guid AppointmentId) : IRequest<CreateZoomMeetingResponse>;