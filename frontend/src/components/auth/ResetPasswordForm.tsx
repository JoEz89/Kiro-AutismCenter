import React, { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useLocalization } from '@/hooks/useLocalization';
import { authService } from '@/services/authService';
import { cn } from '@/lib/utils';

interface ResetPasswordFormProps {
  onSuccess?: () => void;
  className?: string;
}

interface ResetPasswordData {
  newPassword: string;
  confirmPassword: string;
}

export const ResetPasswordForm: React.FC<ResetPasswordFormProps> = ({ onSuccess, className }) => {
  const { t, direction } = useLocalization();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  
  const [formData, setFormData] = useState<ResetPasswordData>({
    newPassword: '',
    confirmPassword: '',
  });
  
  const [errors, setErrors] = useState<Partial<ResetPasswordData>>({});
  const [isLoading, setIsLoading] = useState(false);
  const [apiError, setApiError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const validatePassword = (password: string): boolean => {
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasMinLength = password.length >= 8;
    
    return hasUpperCase && hasLowerCase && hasNumbers && hasMinLength;
  };

  const validateForm = (): boolean => {
    const newErrors: Partial<ResetPasswordData> = {};

    // Password validation
    if (!formData.newPassword) {
      newErrors.newPassword = t('validation.required');
    } else if (formData.newPassword.length < 8) {
      newErrors.newPassword = t('validation.passwordTooShort');
    } else if (!validatePassword(formData.newPassword)) {
      newErrors.newPassword = t('validation.passwordRequirements');
    }

    // Confirm password validation
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = t('validation.required');
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = t('validation.passwordMismatch');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError('');
    setSuccessMessage('');

    if (!token) {
      setApiError(t('auth.invalidResetToken'));
      return;
    }

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);

    try {
      await authService.resetPassword(token, formData.newPassword);
      setSuccessMessage(t('auth.passwordResetSuccess'));
      onSuccess?.();
    } catch (error: any) {
      setApiError(error.response?.data?.message || t('errors.generic'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when user starts typing
    if (errors[name as keyof ResetPasswordData]) {
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

  const passwordStrength = getPasswordStrength(formData.newPassword);

  if (!token) {
    return (
      <div className={cn('w-full max-w-md mx-auto', className)}>
        <div className="bg-white dark:bg-gray-800 shadow-lg rounded-lg p-6">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              {t('auth.invalidResetToken')}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              {t('auth.invalidResetTokenMessage')}
            </p>
            <Link
              to="/forgot-password"
              className="inline-flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              {t('auth.requestNewReset')}
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('w-full max-w-md mx-auto', className)}>
      <div className="bg-white dark:bg-gray-800 shadow-lg rounded-lg p-6">
        <div className="text-center mb-6">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t('auth.resetPassword')}
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            {t('auth.enterNewPassword')}
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
          <div>
            <label 
              htmlFor="newPassword" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.newPassword')}
            </label>
            <input
              type="password"
              id="newPassword"
              name="newPassword"
              value={formData.newPassword}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right',
                errors.newPassword && 'border-red-500 focus:ring-red-500 focus:border-red-500'
              )}
              placeholder={t('auth.passwordPlaceholder')}
              disabled={isLoading}
              aria-invalid={!!errors.newPassword}
              aria-describedby={errors.newPassword ? 'newPassword-error' : 'newPassword-help'}
              autoComplete="new-password"
            />
            {formData.newPassword && (
              <div className="mt-1 text-sm">
                <span className="text-gray-600 dark:text-gray-400">
                  {t('auth.passwordStrength')}: 
                </span>
                <span className={cn('ml-1', passwordStrength.color)}>
                  {passwordStrength.strength}
                </span>
              </div>
            )}
            {errors.newPassword && (
              <p id="newPassword-error" className="mt-1 text-sm text-red-600" role="alert">
                {errors.newPassword}
              </p>
            )}
            <p id="newPassword-help" className="mt-1 text-xs text-gray-500 dark:text-gray-400">
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
              autoComplete="new-password"
            />
            {errors.confirmPassword && (
              <p id="confirmPassword-error" className="mt-1 text-sm text-red-600" role="alert">
                {errors.confirmPassword}
              </p>
            )}
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
              t('auth.resetPassword')
            )}
          </button>
        </form>

        <div className="mt-6 text-center">
          <Link
            to="/login"
            className="text-sm font-medium text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300"
          >
            {t('auth.backToLogin')}
          </Link>
        </div>
      </div>
    </div>
  );
};

export default ResetPasswordForm;