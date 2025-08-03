import React, { useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAppointments } from '../../hooks/useAppointments';
import AppointmentCalendar from './AppointmentCalendar';
import type { Appointment } from '../../types';
import type { AppointmentSlot } from '../../services/appointmentService';

interface RescheduleModalProps {
  appointment: Appointment;
  onClose: () => void;
  onSuccess: () => void;
}

const RescheduleModal: React.FC<RescheduleModalProps> = ({
  appointment,
  onClose,
  onSuccess
}) => {
  const { t, language } = useLocalization();
  const { rescheduleAppointment, loading } = useAppointments();
  const [selectedSlot, setSelectedSlot] = useState<AppointmentSlot | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleReschedule = async () => {
    if (!selectedSlot) return;

    try {
      setError(null);
      const newDateTime = `${selectedSlot.date}T${selectedSlot.time}:00`;
      await rescheduleAppointment(appointment.id, newDateTime);
      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : t('appointments.rescheduleFailed'));
    }
  };

  const formatCurrentDateTime = () => {
    const date = new Date(appointment.appointmentDate);
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

    return `${formattedDate} ${t('appointments.at')} ${formattedTime}`;
  };

  const formatNewDateTime = (slot: AppointmentSlot) => {
    const date = new Date(slot.date);
    const formattedDate = date.toLocaleDateString(language === 'ar' ? 'ar-BH' : 'en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });

    const [hours, minutes] = slot.time.split(':');
    const timeDate = new Date();
    timeDate.setHours(parseInt(hours), parseInt(minutes));
    const formattedTime = timeDate.toLocaleTimeString(language === 'ar' ? 'ar-BH' : 'en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });

    return `${formattedDate} ${t('appointments.at')} ${formattedTime}`;
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-900">
            {t('appointments.rescheduleAppointment')}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
            aria-label={t('common.close')}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {/* Current Appointment Info */}
          <div className="mb-6 p-4 bg-gray-50 rounded-lg">
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              {t('appointments.currentAppointment')}
            </h3>
            <div className="space-y-2 text-sm text-gray-600">
              <p>
                <span className="font-medium">{t('appointments.patient')}:</span> {appointment.patientInfo.firstName} {appointment.patientInfo.lastName}
              </p>
              <p>
                <span className="font-medium">{t('appointments.currentTime')}:</span> {formatCurrentDateTime()}
              </p>
            </div>
          </div>

          {/* New Time Selection */}
          <div className="mb-6">
            <h3 className="text-lg font-medium text-gray-900 mb-4">
              {t('appointments.selectNewTime')}
            </h3>
            <AppointmentCalendar
              selectedDoctorId={appointment.doctorId}
              onSlotSelect={setSelectedSlot}
              selectedSlot={selectedSlot || undefined}
            />
          </div>

          {/* Selected New Time */}
          {selectedSlot && (
            <div className="mb-6 p-4 bg-blue-50 rounded-lg">
              <h4 className="font-medium text-blue-900 mb-2">
                {t('appointments.newAppointmentTime')}
              </h4>
              <p className="text-blue-800">
                {formatNewDateTime(selectedSlot)}
              </p>
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
              <p className="text-red-600 text-sm">{error}</p>
            </div>
          )}

          {/* Important Notice */}
          <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-md">
            <div className="flex items-start">
              <svg className="w-5 h-5 text-yellow-400 mr-2 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
              <div>
                <p className="text-yellow-800 text-sm font-medium">
                  {t('appointments.rescheduleNotice')}
                </p>
                <p className="text-yellow-700 text-sm mt-1">
                  {t('appointments.rescheduleNoticeDescription')}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end space-x-3 p-6 border-t border-gray-200">
          <button
            onClick={onClose}
            className="px-4 py-2 text-gray-700 border border-gray-300 rounded-md hover:bg-gray-50"
          >
            {t('common.cancel')}
          </button>
          <button
            onClick={handleReschedule}
            disabled={!selectedSlot || loading}
            className={`
              px-6 py-2 rounded-md font-medium
              ${!selectedSlot || loading
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700'
              }
            `}
          >
            {loading ? t('common.loading') : t('appointments.confirmReschedule')}
          </button>
        </div>
      </div>
    </div>
  );
};

export default RescheduleModal;