import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useLocalization } from '@/hooks/useLocalization';
import { cn } from '@/lib/utils';
import type { RegisterData } from '@/types';

interface RegisterFormProps {
  onSuccess?: () => void;
  className?: string;
}

interface RegisterFormData extends RegisterData {
  confirmPassword: string;
  agreeToTerms: boolean;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onSuccess, className }) => {
  const { register, loginWithGoogle, isLoading } = useAuth();
  const { t, direction, language } = useLocalization();
  
  const [formData, setFormData] = useState<RegisterFormData>({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    preferredLanguage: language,
    agreeToTerms: false,
  });
  
  const [errors, setErrors] = useState<Partial<Record<keyof RegisterFormData, string>>>({});
  const [apiError, setApiError] = useState<string>('');
  const [successMessage, setSuccessMessage] = useState<string>('');

  const validatePassword = (password: string): boolean => {
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasMinLength = password.length >= 8;
    
    return hasUpperCase && hasLowerCase && hasNumbers && hasMinLength;
  };

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof RegisterFormData, string>> = {};

    // Email validation
    if (!formData.email) {
      newErrors.email = t('validation.required');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = t('validation.invalidEmail');
    }

    // First name validation
    if (!formData.firstName.trim()) {
      newErrors.firstName = t('validation.required');
    }

    // Last name validation
    if (!formData.lastName.trim()) {
      newErrors.lastName = t('validation.required');
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = t('validation.required');
    } else if (formData.password.length < 8) {
      newErrors.password = t('validation.passwordTooShort');
    } else if (!validatePassword(formData.password)) {
      newErrors.password = t('validation.passwordRequirements');
    }

    // Confirm password validation
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = t('validation.required');
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = t('validation.passwordMismatch');
    }

    // Terms agreement validation
    if (!formData.agreeToTerms) {
      newErrors.agreeToTerms = t('validation.required');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError('');
    setSuccessMessage('');

    if (!validateForm()) {
      return;
    }

    try {
      const { confirmPassword, agreeToTerms, ...registerData } = formData;
      await register(registerData);
      setSuccessMessage(t('auth.emailVerificationSent'));
      onSuccess?.();
    } catch (error: any) {
      setApiError(error.response?.data?.message || t('errors.generic'));
    }
  };

  const handleGoogleRegister = async () => {
    setApiError('');
    try {
      await loginWithGoogle();
      onSuccess?.();
    } catch (error: any) {
      setApiError(error.response?.data?.message || t('errors.generic'));
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    
    setFormData(prev => ({ 
      ...prev, 
      [name]: type === 'checkbox' ? checked : value 
    }));
    
    // Clear error when user starts typing
    if (errors[name as keyof RegisterFormData]) {
      setErrors(prev => ({ ...prev, [name]: undefined }));
    }
  };

  const getPasswordStrength = (password: string): { strength: string; color: string } => {
    if (password.length === 0) return { strength: '', color: '' };
    
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasMinLength = password.length >= 8;
    
    const score = [hasUpperCase, hasLowerCase, hasNumbers, hasMinLength].filter(Boolean).length;
    
    if (score <= 1) return { strength: t('auth.weak'), color: 'text-red-600' };
    if (score <= 2) return { strength: t('auth.medium'), color: 'text-yellow-600' };
    return { strength: t('auth.strong'), color: 'text-green-600' };
  };

  const passwordStrength = getPasswordStrength(formData.password);

  return (
    <div className={cn('w-full max-w-md mx-auto', className)}>
      <div className="bg-white dark:bg-gray-800 shadow-lg rounded-lg p-6">
        <div className="text-center mb-6">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t('auth.register')}
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            {t('auth.createAccount')}
          </p>
        </div>

        {apiError && (
          <div 
            className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded"
            role="alert"
            aria-live="polite"
          >
            {apiError}
          </div>
        )}

        {successMessage && (
          <div 
            className="mb-4 p-3 bg-green-100 border border-green-400 text-green-700 rounded"
            role="alert"
            aria-live="polite"
          >
            {successMessage}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label 
                htmlFor="firstName" 
                className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
              >
                {t('auth.firstName')}
              </label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleInputChange}
                className={cn(
                  'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                  'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                  direction === 'rtl' && 'text-right',
                  errors.firstName && 'border-red-500 focus:ring-red-500 focus:border-red-500'
                )}
                disabled={isLoading}
                aria-invalid={!!errors.firstName}
                aria-describedby={errors.firstName ? 'firstName-error' : undefined}
              />
              {errors.firstName && (
                <p id="firstName-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.firstName}
                </p>
              )}
            </div>

            <div>
              <label 
                htmlFor="lastName" 
                className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
              >
                {t('auth.lastName')}
              </label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleInputChange}
                className={cn(
                  'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                  'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                  direction === 'rtl' && 'text-right',
                  errors.lastName && 'border-red-500 focus:ring-red-500 focus:border-red-500'
                )}
                disabled={isLoading}
                aria-invalid={!!errors.lastName}
                aria-describedby={errors.lastName ? 'lastName-error' : undefined}
              />
              {errors.lastName && (
                <p id="lastName-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.lastName}
                </p>
              )}
            </div>
          </div>

          <div>
            <label 
              htmlFor="email" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.email')}
            </label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right',
                errors.email && 'border-red-500 focus:ring-red-500 focus:border-red-500'
              )}
              placeholder={t('auth.emailPlaceholder')}
              disabled={isLoading}
              aria-invalid={!!errors.email}
              aria-describedby={errors.email ? 'email-error' : undefined}
            />
            {errors.email && (
              <p id="email-error" className="mt-1 text-sm text-red-600" role="alert">
                {errors.email}
              </p>
            )}
          </div>

          <div>
            <label 
              htmlFor="preferredLanguage" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.preferredLanguage')}
            </label>
            <select
              id="preferredLanguage"
              name="preferredLanguage"
              value={formData.preferredLanguage}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right'
              )}
              disabled={isLoading}
            >
              <option value="en">English</option>
              <option value="ar">العربية</option>
            </select>
          </div>

          <div>
            <label 
              htmlFor="password" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.password')}
            </label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right',
                errors.password && 'border-red-500 focus:ring-red-500 focus:border-red-500'
              )}
              placeholder={t('auth.passwordPlaceholder')}
              disabled={isLoading}
              aria-invalid={!!errors.password}
              aria-describedby={errors.password ? 'password-error' : 'password-help'}
            />
            {formData.password && (
              <div className="mt-1 text-sm">
                <span className="text-gray-600 dark:text-gray-400">
                  {t('auth.passwordStrength')}: 
                </span>
                <span className={cn('ml-1', passwordStrength.color)}>
                  {passwordStrength.strength}
                </span>
              </div>
            )}
            {errors.password && (
              <p id="password-error" className="mt-1 text-sm text-red-600" role="alert">
                {errors.password}
              </p>
            )}
            <p id="password-help" className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {t('auth.passwordRequirements')}
            </p>
          </div>

          <div>
            <label 
              htmlFor="confirmPassword" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.confirmPassword')}
            </label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right',
                errors.confirmPassword && 'border-red-500 focus:ring-red-500 focus:border-red-500'
              )}
              disabled={isLoading}
              aria-invalid={!!errors.confirmPassword}
              aria-describedby={errors.confirmPassword ? 'confirmPassword-error' : undefined}
            />
            {errors.confirmPassword && (
              <p id="confirmPassword-error" className="mt-1 text-sm text-red-600" role="alert">
                {errors.confirmPassword}
              </p>
            )}
          </div>

          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                id="agreeToTerms"
                name="agreeToTerms"
                type="checkbox"
                checked={formData.agreeToTerms}
                onChange={handleInputChange}
                className={cn(
                  'h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded',
                  errors.agreeToTerms && 'border-red-500'
                )}
                disabled={isLoading}
                aria-invalid={!!errors.agreeToTerms}
                aria-describedby={errors.agreeToTerms ? 'terms-error' : undefined}
              />
            </div>
            <div className="ml-3 text-sm">
              <label htmlFor="agreeToTerms" className="text-gray-700 dark:text-gray-300">
                {t('auth.agreeToTerms').split(' ').slice(0, -4).join(' ')}{' '}
                <Link
                  to="/terms"
                  className="text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300"
                >
                  {t('auth.termsOfService')}
                </Link>
                {' '}{t('common.and')}{' '}
                <Link
                  to="/privacy"
                  className="text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300"
                >
                  {t('auth.privacyPolicy')}
                </Link>
              </label>
              {errors.agreeToTerms && (
                <p id="terms-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.agreeToTerms}
                </p>
              )}
            </div>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className={cn(
              'w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white',
              'bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500',
              'disabled:opacity-50 disabled:cursor-not-allowed',
              'dark:bg-blue-500 dark:hover:bg-blue-600'
            )}
          >
            {isLoading ? (
              <div className="flex items-center">
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                {t('common.loading')}
              </div>
            ) : (
              t('auth.register')
            )}
          </button>
        </form>

        <div className="mt-6">
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300 dark:border-gray-600" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white dark:bg-gray-800 text-gray-500 dark:text-gray-400">
                {t('auth.orContinueWith')}
              </span>
            </div>
          </div>

          <div className="mt-6">
            <button
              type="button"
              onClick={handleGoogleRegister}
              disabled={isLoading}
              className={cn(
                'w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm',
                'bg-white text-sm font-medium text-gray-500 hover:bg-gray-50',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-600',
                'disabled:opacity-50 disabled:cursor-not-allowed'
              )}
            >
              <svg className="w-5 h-5 mr-2" viewBox="0 0 24 24">
                <path
                  fill="currentColor"
                  d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                />
                <path
                  fill="currentColor"
                  d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                />
                <path
                  fill="currentColor"
                  d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                />
                <path
                  fill="currentColor"
                  d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                />
              </svg>
              {t('auth.registerWithGoogle')}
            </button>
          </div>
        </div>

        <div className="mt-6 text-center">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            {t('auth.alreadyHaveAccount')}{' '}
            <Link
              to="/login"
              className="font-medium text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300"
            >
              {t('auth.login')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default RegisterForm;