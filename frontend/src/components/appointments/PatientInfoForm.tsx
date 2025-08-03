import React, { useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import type { PatientInfo } from '../../types';

interface PatientInfoFormProps {
  patientInfo: PatientInfo;
  onPatientInfoChange: (info: PatientInfo) => void;
  errors?: Record<string, string>;
}

const PatientInfoForm: React.FC<PatientInfoFormProps> = ({
  patientInfo,
  onPatientInfoChange,
  errors = {}
}) => {
  const { t } = useLocalization();

  const handleInputChange = (field: keyof PatientInfo, value: string | number) => {
    onPatientInfoChange({
      ...patientInfo,
      [field]: value
    });
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <h3 className="text-lg font-semibold mb-4">
        {t('appointments.patientInfo')}
      </h3>
      
      <div className="grid gap-4 md:grid-cols-2">
        {/* First Name */}
        <div>
          <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-1">
            {t('auth.firstName')} *
          </label>
          <input
            type="text"
            id="firstName"
            value={patientInfo.firstName}
            onChange={(e) => handleInputChange('firstName', e.target.value)}
            className={`
              w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500
              ${errors.firstName ? 'border-red-300' : 'border-gray-300'}
            `}
            placeholder={t('auth.firstName')}
            required
          />
          {errors.firstName && (
            <p className="mt-1 text-sm text-red-600">{errors.firstName}</p>
          )}
        </div>

        {/* Last Name */}
        <div>
          <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-1">
            {t('auth.lastName')} *
          </label>
          <input
            type="text"
            id="lastName"
            value={patientInfo.lastName}
            onChange={(e) => handleInputChange('lastName', e.target.value)}
            className={`
              w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500
              ${errors.lastName ? 'border-red-300' : 'border-gray-300'}
            `}
            placeholder={t('auth.lastName')}
            required
          />
          {errors.lastName && (
            <p className="mt-1 text-sm text-red-600">{errors.lastName}</p>
          )}
        </div>

        {/* Age */}
        <div>
          <label htmlFor="age" className="block text-sm font-medium text-gray-700 mb-1">
            {t('appointments.age')} *
          </label>
          <input
            type="number"
            id="age"
            min="1"
            max="120"
            value={patientInfo.age}
            onChange={(e) => handleInputChange('age', parseInt(e.target.value) || 0)}
            className={`
              w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500
              ${errors.age ? 'border-red-300' : 'border-gray-300'}
            `}
            placeholder={t('appointments.age')}
            required
          />
          {errors.age && (
            <p className="mt-1 text-sm text-red-600">{errors.age}</p>
          )}
        </div>

        {/* Phone */}
        <div>
          <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-1">
            {t('appointments.phone')} *
          </label>
          <input
            type="tel"
            id="phone"
            value={patientInfo.phone}
            onChange={(e) => handleInputChange('phone', e.target.value)}
            className={`
              w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500
              ${errors.phone ? 'border-red-300' : 'border-gray-300'}
            `}
            placeholder={t('appointments.phonePlaceholder')}
            required
          />
          {errors.phone && (
            <p className="mt-1 text-sm text-red-600">{errors.phone}</p>
          )}
        </div>
      </div>

      {/* Notes */}
      <div className="mt-4">
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          {t('appointments.additionalNotes')} ({t('checkout.optional')})
        </label>
        <textarea
          id="notes"
          rows={4}
          value={patientInfo.notes || ''}
          onChange={(e) => handleInputChange('notes', e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder={t('appointments.notesPlaceholder')}
        />
      </div>
    </div>
  );
};

export default PatientInfoForm;