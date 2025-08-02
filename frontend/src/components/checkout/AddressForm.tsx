import { useState } from 'react';
import { useLocalization } from '@/hooks';
import type { Address } from '@/types';

interface AddressFormProps {
  address: Address;
  onAddressChange: (address: Address) => void;
  title: string;
  errors?: Record<string, string>;
  className?: string;
}

export const AddressForm = ({ address, onAddressChange, title, errors = {}, className = '' }: AddressFormProps) => {
  const { t } = useLocalization();
  const [isValidating, setIsValidating] = useState(false);

  const handleFieldChange = (field: keyof Address, value: string) => {
    onAddressChange({
      ...address,
      [field]: value,
    });
  };

  const countries = [
    { code: 'BH', name: t('countries.bahrain', 'Bahrain') },
    { code: 'US', name: t('countries.unitedStates', 'United States') },
    { code: 'GB', name: t('countries.unitedKingdom', 'United Kingdom') },
    { code: 'CA', name: t('countries.canada', 'Canada') },
    { code: 'AU', name: t('countries.australia', 'Australia') },
  ];

  return (
    <div className={`space-y-6 ${className}`}>
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {title}
      </h3>

      <div className="grid grid-cols-1 gap-6">
        {/* Street Address */}
        <div>
          <label htmlFor={`${title.toLowerCase()}-street`} className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('checkout.streetAddress')} *
          </label>
          <input
            type="text"
            id={`${title.toLowerCase()}-street`}
            value={address.street}
            onChange={(e) => handleFieldChange('street', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
              errors.street ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
            }`}
            placeholder={t('checkout.streetAddressPlaceholder', 'Enter your street address')}
            required
          />
          {errors.street && (
            <p className="mt-1 text-sm text-red-600 dark:text-red-400">
              {errors.street}
            </p>
          )}
        </div>

        {/* City and State */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor={`${title.toLowerCase()}-city`} className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.city')} *
            </label>
            <input
              type="text"
              id={`${title.toLowerCase()}-city`}
              value={address.city}
              onChange={(e) => handleFieldChange('city', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.city ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder={t('checkout.cityPlaceholder', 'Enter city')}
              required
            />
            {errors.city && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.city}
              </p>
            )}
          </div>

          <div>
            <label htmlFor={`${title.toLowerCase()}-state`} className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.state')} *
            </label>
            <input
              type="text"
              id={`${title.toLowerCase()}-state`}
              value={address.state}
              onChange={(e) => handleFieldChange('state', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.state ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder={t('checkout.statePlaceholder', 'Enter state/province')}
              required
            />
            {errors.state && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.state}
              </p>
            )}
          </div>
        </div>

        {/* Postal Code and Country */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor={`${title.toLowerCase()}-postal`} className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.postalCode')} *
            </label>
            <input
              type="text"
              id={`${title.toLowerCase()}-postal`}
              value={address.postalCode}
              onChange={(e) => handleFieldChange('postalCode', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.postalCode ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder={t('checkout.postalCodePlaceholder', 'Enter postal code')}
              required
            />
            {errors.postalCode && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.postalCode}
              </p>
            )}
          </div>

          <div>
            <label htmlFor={`${title.toLowerCase()}-country`} className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.country')} *
            </label>
            <select
              id={`${title.toLowerCase()}-country`}
              value={address.country}
              onChange={(e) => handleFieldChange('country', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.country ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              required
            >
              <option value="">{t('checkout.selectCountry', 'Select a country')}</option>
              {countries.map((country) => (
                <option key={country.code} value={country.code}>
                  {country.name}
                </option>
              ))}
            </select>
            {errors.country && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.country}
              </p>
            )}
          </div>
        </div>
      </div>

      {isValidating && (
        <div className="flex items-center text-sm text-blue-600 dark:text-blue-400">
          <svg className="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {t('checkout.validatingAddress', 'Validating address...')}
        </div>
      )}
    </div>
  );
};