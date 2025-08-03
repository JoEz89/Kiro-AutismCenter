import React, { useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAppointments } from '../../hooks/useAppointments';
import type { Appointment, AppointmentStatus } from '../../types';

interface AppointmentListProps {
  filter?: 'upcoming' | 'past' | 'all';
  onReschedule?: (appointment: Appointment) => void;
  onCancel?: (appointment: Appointment) => void;
}

const AppointmentList: React.FC<AppointmentListProps> = ({
  filter = 'all',
  onReschedule,
  onCancel
}) => {
  const { t, language } = useLocalization();
  const { appointments, loading, error, cancelAppointment } = useAppointments();
  const [cancellingId, setCancellingId] = useState<string | null>(null);

  const filterAppointments = (appointments: Appointment[]) => {
    const now = new Date();
    
    switch (filter) {
      case 'upcoming':
        return appointments.filter(apt => 
          new Date(apt.appointmentDate) > now && 
          apt.status !== 'cancelled' && 
          apt.status !== 'completed'
        );
      case 'past':
        return appointments.filter(apt => 
          new Date(apt.appointmentDate) <= now || 
          apt.status === 'completed' || 
          apt.status === 'cancelled'
        );
      default:
        return appointments;
    }
  };

  const handleCancel = async (appointment: Appointment) => {
    if (!window.confirm(t('appointments.confirmCancel'))) {
      return;
    }

    try {
      setCancellingId(appointment.id);
      await cancelAppointment(appointment.id);
      onCancel?.(appointment);
    } catch (error) {
      console.error('Failed to cancel appointment:', error);
    } finally {
      setCancellingId(null);
    }
  };

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    const formattedDate = date.toLocaleDateString(language === 'ar' ? 'ar-BH' : 'en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });

    const formattedTime = date.toLocaleTimeString(language === 'ar' ? 'ar-BH' : 'en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });

    return { date: formattedDate, time: formattedTime };
  };

  const getStatusColor = (status: AppointmentStatus) => {
    switch (status) {
      case 'scheduled':
        return 'bg-blue-100 text-blue-800';
      case 'confirmed':
        return 'bg-green-100 text-green-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      case 'completed':
        return 'bg-gray-100 text-gray-800';
      case 'no_show':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status: AppointmentStatus) => {
    return t(`appointments.status.${status}`);
  };

  const canCancel = (appointment: Appointment) => {
    const appointmentDate = new Date(appointment.appointmentDate);
    const now = new Date();
    const hoursUntilAppointment = (appointmentDate.getTime() - now.getTime()) / (1000 * 60 * 60);
    
    return hoursUntilAppointment > 24 && 
           appointment.status !== 'cancelled' && 
           appointment.status !== 'completed';
  };

  const canReschedule = (appointment: Appointment) => {
    const appointmentDate = new Date(appointment.appointmentDate);
    const now = new Date();
    const hoursUntilAppointment = (appointmentDate.getTime() - now.getTime()) / (1000 * 60 * 60);
    
    return hoursUntilAppointment > 24 && 
           appointment.status !== 'cancelled' && 
           appointment.status !== 'completed';
  };

  const canJoinMeeting = (appointment: Appointment) => {
    const appointmentDate = new Date(appointment.appointmentDate);
    const now = new Date();
    const minutesUntilAppointment = (appointmentDate.getTime() - now.getTime()) / (1000 * 60);
    
    return minutesUntilAppointment <= 15 && 
           minutesUntilAppointment >= -60 && 
           appointment.status === 'confirmed' &&
           appointment.zoomLink;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2">{t('common.loading')}</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center p-8">
        <p className="text-red-600 mb-4">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          {t('common.retry')}
        </button>
      </div>
    );
  }

  const filteredAppointments = filterAppointments(appointments);

  if (filteredAppointments.length === 0) {
    return (
      <div className="text-center p-8">
        <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg className="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          {t('appointments.noAppointments')}
        </h3>
        <p className="text-gray-600">
          {filter === 'upcoming' 
            ? t('appointments.noUpcomingAppointments')
            : filter === 'past'
            ? t('appointments.noPastAppointments')
            : t('appointments.noAppointmentsDescription')
          }
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {filteredAppointments.map((appointment) => {
        const { date, time } = formatDateTime(appointment.appointmentDate);
        
        return (
          <div
            key={appointment.id}
            className="bg-white rounded-lg shadow-md p-6 border border-gray-200"
          >
            <div className="flex items-start justify-between">
              <div className="flex-1">
                {/* Date and Time */}
                <div className="flex items-center mb-2">
                  <svg className="w-5 h-5 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span className="text-lg font-medium text-gray-900">
                    {date}
                  </span>
                </div>
                
                <div className="flex items-center mb-3">
                  <svg className="w-5 h-5 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span className="text-gray-700">{time}</span>
                </div>

                {/* Patient Info */}
                <div className="mb-3">
                  <p className="text-gray-900 font-medium">
                    {appointment.patientInfo.firstName} {appointment.patientInfo.lastName}
                  </p>
                  <p className="text-sm text-gray-600">
                    {t('appointments.age')}: {appointment.patientInfo.age} | {t('appointments.phone')}: {appointment.patientInfo.phone}
                  </p>
                </div>

                {/* Notes */}
                {appointment.patientInfo.notes && (
                  <div className="mb-3">
                    <p className="text-sm text-gray-600">
                      <span className="font-medium">{t('appointments.notes')}:</span> {appointment.patientInfo.notes}
                    </p>
                  </div>
                )}

                {/* Doctor Notes */}
                {appointment.notes && (
                  <div className="mb-3">
                    <p className="text-sm text-gray-600">
                      <span className="font-medium">{t('appointments.doctorNotes')}:</span> {appointment.notes}
                    </p>
                  </div>
                )}
              </div>

              {/* Status Badge */}
              <div className="ml-4">
                <span className={`
                  inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                  ${getStatusColor(appointment.status)}
                `}>
                  {getStatusText(appointment.status)}
                </span>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="mt-4 flex flex-wrap gap-2">
              {canJoinMeeting(appointment) && (
                <a
                  href={appointment.zoomLink}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                  {t('appointments.joinMeeting')}
                </a>
              )}

              {canReschedule(appointment) && onReschedule && (
                <button
                  onClick={() => onReschedule(appointment)}
                  className="inline-flex items-center px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {t('appointments.reschedule')}
                </button>
              )}

              {canCancel(appointment) && (
                <button
                  onClick={() => handleCancel(appointment)}
                  disabled={cancellingId === appointment.id}
                  className="inline-flex items-center px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700 disabled:opacity-50"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                  {cancellingId === appointment.id ? t('common.loading') : t('appointments.cancel')}
                </button>
              )}

              {appointment.zoomLink && !canJoinMeeting(appointment) && (
                <button
                  onClick={() => navigator.clipboard.writeText(appointment.zoomLink)}
                  className="inline-flex items-center px-4 py-2 bg-gray-600 text-white text-sm font-medium rounded-md hover:bg-gray-700"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                  {t('appointments.copyZoomLink')}
                </button>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
};

export default AppointmentList;