using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetDoctorAvailability;

public record GetDoctorAvailabilityQuery(Guid DoctorId) : IRequest<GetDoctorAvailabilityResponse>;