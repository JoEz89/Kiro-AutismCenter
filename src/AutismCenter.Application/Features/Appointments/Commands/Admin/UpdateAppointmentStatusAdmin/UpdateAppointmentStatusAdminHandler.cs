using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;

namespace AutismCenter.Application.Features.Appointments.Commands.Admin.UpdateAppointmentStatusAdmin;

public class UpdateAppointmentStatusAdminHandler : IRequestHandler<UpdateAppointmentStatusAdminCommand, UpdateAppointmentStatusAdminResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppointmentStatusAdminHandler(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateAppointmentStatusAdminResponse> Handle(UpdateAppointmentStatusAdminCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        
        if (appointment == null)
        {
            throw new NotFoundException($"Appointment with ID {request.AppointmentId} not found");
        }

        // Update appointment status and notes
        appointment.UpdateStatus(request.Status);
        
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            appointment.AddNotes(request.Notes);
        }

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateAppointmentStatusAdminResponse(
            appointment.Id,
            appointment.Status,
            appointment.Notes,
            appointment.UpdatedAt
        );
    }
}