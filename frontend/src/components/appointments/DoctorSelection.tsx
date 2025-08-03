import React from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useDoctors } from '../../hooks/useAppointments';
import type { Doctor } from '../../types';

interface DoctorSelectionProps {
  selectedDoctorId?: string;
  onDoctorSelect: (doctor: Doctor) => void;
}

const DoctorSelection: React.FC<DoctorSelectionProps> = ({
  selectedDoctorId,
  onDoctorSelect
}) => {
  const { t, language } = useLocalization();
  const { doctors, loading, error } = useDoctors();

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
        <p className="text-red-600">{error}</p>
      </div>
    );
  }

  if (doctors.length === 0) {
    return (
      <div className="text-center p-8">
        <p className="text-gray-600">{t('appointments.noDoctorsAvailable')}</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <h3 className="text-lg font-semibold mb-4">
        {t('appointments.selectDoctor')}
      </h3>
      
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {doctors.map((doctor) => (
          <div
            key={doctor.id}
            className={`
              p-4 border-2 rounded-lg cursor-pointer transition-all
              ${selectedDoctorId === doctor.id
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
              }
            `}
            onClick={() => onDoctorSelect(doctor)}
          >
            <div className="flex items-start space-x-3">
              {/* Doctor Avatar Placeholder */}
              <div className="w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              
              <div className="flex-1">
                <h4 className="font-medium text-gray-900">
                  {language === 'ar' ? doctor.nameAr : doctor.nameEn}
                </h4>
                <p className="text-sm text-gray-600 mt-1">
                  {language === 'ar' ? doctor.specialtyAr : doctor.specialtyEn}
                </p>
                
                {/* Availability indicator */}
                <div className="mt-2 flex items-center">
                  <div className="w-2 h-2 bg-green-400 rounded-full mr-2"></div>
                  <span className="text-xs text-gray-500">
                    {t('appointments.available')}
                  </span>
                </div>
              </div>
              
              {/* Selection indicator */}
              {selectedDoctorId === doctor.id && (
                <div className="text-blue-500">
                  <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DoctorSelection;