import React, { useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAppointments } from '../../hooks/useAppointments';
import DoctorSelection from './DoctorSelection';
import AppointmentCalendar from './AppointmentCalendar';
import PatientInfoForm from './PatientInfoForm';
import type { Doctor, PatientInfo } from '../../types';
import type { AppointmentSlot } from '../../services/appointmentService';

interface AppointmentBookingFormProps {
  onSuccess?: () => void;
  onCancel?: () => void;
}

const AppointmentBookingForm: React.FC<AppointmentBookingFormProps> = ({
  onSuccess,
  onCancel
}) => {
  const { t, language } = useLocalization();
  const { bookAppointment, loading } = useAppointments();
  
  const [step, setStep] = useState(1);
  const [selectedDoctor, setSelectedDoctor] = useState<Doctor | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<AppointmentSlot | null>(null);
  const [patientInfo, setPatientInfo] = useState<PatientInfo>({
    firstName: '',
    lastName: '',
    age: 0,
    phone: '',
    notes: ''
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validatePatientInfo = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!patientInfo.firstName.trim()) {
      newErrors.firstName = t('validation.required');
    }

    if (!patientInfo.lastName.trim()) {
      newErrors.lastName = t('validation.required');
    }

    if (!patientInfo.age || patientInfo.age < 1 || patientInfo.age > 120) {
      newErrors.age = t('appointments.invalidAge');
    }

    if (!patientInfo.phone.trim()) {
      newErrors.phone = t('validation.required');
    } else if (!/^\+?[\d\s\-\(\)]+$/.test(patientInfo.phone)) {
      newErrors.phone = t('validation.invalidPhone');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (step === 1 && !selectedDoctor) {
      return;
    }
    if (step === 2 && !selectedSlot) {
      return;
    }
    if (step === 3 && !validatePatientInfo()) {
      return;
    }
    setStep(step + 1);
  };

  const handleBack = () => {
    setStep(step - 1);
  };

  const handleSubmit = async () => {
    if (!selectedDoctor || !selectedSlot || !validatePatientInfo()) {
      return;
    }

    try {
      setSubmitError(null);
      const appointmentDate = `${selectedSlot.date}T${selectedSlot.time}:00`;
      
      await bookAppointment({
        doctorId: selectedDoctor.id,
        appointmentDate,
        patientInfo
      });

      onSuccess?.();
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : t('appointments.bookingFailed'));
    }
  };

  const formatDateTime = (slot: AppointmentSlot) => {
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

  const renderStepContent = () => {
    switch (step) {
      case 1:
        return (
          <DoctorSelection
            selectedDoctorId={selectedDoctor?.id}
            onDoctorSelect={setSelectedDoctor}
          />
        );
      case 2:
        return (
          <AppointmentCalendar
            selectedDoctorId={selectedDoctor?.id}
            onSlotSelect={setSelectedSlot}
            selectedSlot={selectedSlot || undefined}
          />
        );
      case 3:
        return (
          <PatientInfoForm
            patientInfo={patientInfo}
            onPatientInfoChange={setPatientInfo}
            errors={errors}
          />
        );
      case 4:
        return (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold mb-4">
              {t('appointments.confirmBooking')}
            </h3>
            
            <div className="space-y-4">
              {/* Doctor Info */}
              <div className="border-b pb-4">
                <h4 className="font-medium text-gray-900 mb-2">
                  {t('appointments.selectedDoctor')}
                </h4>
                <p className="text-gray-700">
                  {language === 'ar' ? selectedDoctor?.nameAr : selectedDoctor?.nameEn}
                </p>
                <p className="text-sm text-gray-600">
                  {language === 'ar' ? selectedDoctor?.specialtyAr : selectedDoctor?.specialtyEn}
                </p>
              </div>

              {/* Appointment Time */}
              <div className="border-b pb-4">
                <h4 className="font-medium text-gray-900 mb-2">
                  {t('appointments.appointmentTime')}
                </h4>
                <p className="text-gray-700">
                  {selectedSlot && formatDateTime(selectedSlot)}
                </p>
              </div>

              {/* Patient Info */}
              <div>
                <h4 className="font-medium text-gray-900 mb-2">
                  {t('appointments.patientInfo')}
                </h4>
                <div className="text-gray-700 space-y-1">
                  <p>{patientInfo.firstName} {patientInfo.lastName}</p>
                  <p>{t('appointments.age')}: {patientInfo.age}</p>
                  <p>{t('appointments.phone')}: {patientInfo.phone}</p>
                  {patientInfo.notes && (
                    <p>{t('appointments.notes')}: {patientInfo.notes}</p>
                  )}
                </div>
              </div>
            </div>

            {submitError && (
              <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-md">
                <p className="text-red-600 text-sm">{submitError}</p>
              </div>
            )}
          </div>
        );
      default:
        return null;
    }
  };

  const getStepTitle = () => {
    switch (step) {
      case 1:
        return t('appointments.selectDoctor');
      case 2:
        return t('appointments.selectDateTime');
      case 3:
        return t('appointments.patientInfo');
      case 4:
        return t('appointments.confirmBooking');
      default:
        return '';
    }
  };

  const canProceed = () => {
    switch (step) {
      case 1:
        return selectedDoctor !== null;
      case 2:
        return selectedSlot !== null;
      case 3:
        return validatePatientInfo();
      case 4:
        return true;
      default:
        return false;
    }
  };

  return (
    <div className="max-w-4xl mx-auto">
      {/* Progress Steps */}
      <div className="mb-8">
        <div className="flex items-center justify-between">
          {[1, 2, 3, 4].map((stepNumber) => (
            <div key={stepNumber} className="flex items-center">
              <div className={`
                w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium
                ${step >= stepNumber
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-200 text-gray-600'
                }
              `}>
                {stepNumber}
              </div>
              {stepNumber < 4 && (
                <div className={`
                  w-16 h-1 mx-2
                  ${step > stepNumber ? 'bg-blue-600' : 'bg-gray-200'}
                `} />
              )}
            </div>
          ))}
        </div>
        <div className="mt-2">
          <h2 className="text-xl font-semibold">{getStepTitle()}</h2>
        </div>
      </div>

      {/* Step Content */}
      <div className="mb-8">
        {renderStepContent()}
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between">
        <div>
          {step > 1 && (
            <button
              onClick={handleBack}
              className="px-4 py-2 text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50"
            >
              {t('common.back')}
            </button>
          )}
          {onCancel && (
            <button
              onClick={onCancel}
              className="ml-2 px-4 py-2 text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50"
            >
              {t('common.cancel')}
            </button>
          )}
        </div>

        <div>
          {step < 4 ? (
            <button
              onClick={handleNext}
              disabled={!canProceed()}
              className={`
                px-6 py-2 rounded-md font-medium
                ${canProceed()
                  ? 'bg-blue-600 text-white hover:bg-blue-700'
                  : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }
              `}
            >
              {t('common.next')}
            </button>
          ) : (
            <button
              onClick={handleSubmit}
              disabled={loading}
              className={`
                px-6 py-2 rounded-md font-medium
                ${loading
                  ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  : 'bg-green-600 text-white hover:bg-green-700'
                }
              `}
            >
              {loading ? t('common.loading') : t('appointments.confirmBooking')}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default AppointmentBookingForm;