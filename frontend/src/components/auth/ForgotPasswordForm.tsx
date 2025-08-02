import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useLocalization } from '@/hooks/useLocalization';
import { authService } from '@/services/authService';
import { cn } from '@/lib/utils';

interface ForgotPasswordFormProps {
  onSuccess?: () => void;
  className?: string;
}

export const ForgotPasswordForm: React.FC<ForgotPasswordFormProps> = ({ onSuccess, className }) => {
  const { t, direction } = useLocalization();
  
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const validateEmail = (email: string): boolean => {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccessMessage('');

    if (!email) {
      setError(t('validation.required'));
      return;
    }

    if (!validateEmail(email)) {
      setError(t('validation.invalidEmail'));
      return;
    }

    setIsLoading(true);

    try {
      await authService.forgotPassword(email);
      setSuccessMessage(t('auth.resetEmailSent'));
      onSuccess?.();
    } catch (error: any) {
      setError(error.response?.data?.message || t('errors.generic'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value);
    if (error) {
      setError('');
    }
  };

  return (
    <div className={cn('w-full max-w-md mx-auto', className)}>
      <div className="bg-white dark:bg-gray-800 shadow-lg rounded-lg p-6">
        <div className="text-center mb-6">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t('auth.forgotPassword').replace('?', '')}
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            {t('auth.checkEmail')}
          </p>
        </div>

        {error && (
          <div 
            className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded"
            role="alert"
            aria-live="polite"
          >
            {error}
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
              htmlFor="email" 
              className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              {t('auth.email')}
            </label>
            <input
              type="email"
              id="email"
              name="email"
              value={email}
              onChange={handleInputChange}
              className={cn(
                'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
                'dark:bg-gray-700 dark:border-gray-600 dark:text-white',
                direction === 'rtl' && 'text-right',
                error && 'border-red-500 focus:ring-red-500 focus:border-red-500'
              )}
              placeholder={t('auth.emailPlaceholder')}
              disabled={isLoading}
              aria-invalid={!!error}
              aria-describedby={error ? 'email-error' : 'email-help'}
              autoComplete="email"
            />
            <p id="email-help" className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {t('auth.resetPasswordInstructions')}
            </p>
            {error && (
              <p id="email-error" className="mt-1 text-sm text-red-600" role="alert">
                {error}
              </p>
            )}
          </div>

          <button
            type="submit"
            disabled={isLoading || !email}
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
              t('auth.sendResetEmail')
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

export default ForgotPasswordForm;