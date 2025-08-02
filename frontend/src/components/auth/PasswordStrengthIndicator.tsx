import React from 'react';
import { useLocalization } from '@/hooks/useLocalization';
import { cn } from '@/lib/utils';

interface PasswordStrengthIndicatorProps {
  password: string;
  className?: string;
  showRequirements?: boolean;
}

interface PasswordRequirement {
  key: string;
  label: string;
  test: (password: string) => boolean;
}

export const PasswordStrengthIndicator: React.FC<PasswordStrengthIndicatorProps> = ({
  password,
  className,
  showRequirements = true,
}) => {
  const { t } = useLocalization();

  const requirements: PasswordRequirement[] = [
    {
      key: 'length',
      label: t('auth.passwordReq.minLength'),
      test: (pwd) => pwd.length >= 8,
    },
    {
      key: 'uppercase',
      label: t('auth.passwordReq.uppercase'),
      test: (pwd) => /[A-Z]/.test(pwd),
    },
    {
      key: 'lowercase',
      label: t('auth.passwordReq.lowercase'),
      test: (pwd) => /[a-z]/.test(pwd),
    },
    {
      key: 'number',
      label: t('auth.passwordReq.number'),
      test: (pwd) => /\d/.test(pwd),
    },
  ];

  const getPasswordStrength = (): { 
    score: number; 
    strength: string; 
    color: string; 
    bgColor: string;
  } => {
    if (password.length === 0) {
      return { score: 0, strength: '', color: '', bgColor: '' };
    }

    const score = requirements.filter(req => req.test(password)).length;
    
    if (score <= 1) {
      return { 
        score, 
        strength: t('auth.weak'), 
        color: 'text-red-600', 
        bgColor: 'bg-red-500' 
      };
    }
    if (score <= 2) {
      return { 
        score, 
        strength: t('auth.medium'), 
        color: 'text-yellow-600', 
        bgColor: 'bg-yellow-500' 
      };
    }
    return { 
      score, 
      strength: t('auth.strong'), 
      color: 'text-green-600', 
      bgColor: 'bg-green-500' 
    };
  };

  const { score, strength, color, bgColor } = getPasswordStrength();
  const progressPercentage = password.length > 0 ? (score / requirements.length) * 100 : 0;

  return (
    <div className={cn('space-y-2', className)}>
      {password.length > 0 && (
        <>
          {/* Strength indicator bar */}
          <div className="space-y-1">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600 dark:text-gray-400">
                {t('auth.passwordStrength')}:
              </span>
              <span className={cn('text-sm font-medium', color)}>
                {strength}
              </span>
            </div>
            <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                className={cn('h-2 rounded-full transition-all duration-300', bgColor)}
                style={{ width: `${progressPercentage}%` }}
                role="progressbar"
                aria-valuenow={score}
                aria-valuemin={0}
                aria-valuemax={requirements.length}
                aria-label={`${t('auth.passwordStrength')}: ${strength}`}
              />
            </div>
          </div>

          {/* Requirements checklist */}
          {showRequirements && (
            <div className="space-y-1">
              <p className="text-xs text-gray-600 dark:text-gray-400">
                {t('auth.passwordMustContain')}:
              </p>
              <ul className="space-y-1" role="list">
                {requirements.map((requirement) => {
                  const isMet = requirement.test(password);
                  return (
                    <li
                      key={requirement.key}
                      className={cn(
                        'flex items-center text-xs',
                        isMet ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'
                      )}
                    >
                      <span className="mr-2" aria-hidden="true">
                        {isMet ? '✓' : '○'}
                      </span>
                      <span className={isMet ? 'line-through' : ''}>
                        {requirement.label}
                      </span>
                      <span className="sr-only">
                        {isMet ? t('common.completed') : t('common.pending')}
                      </span>
                    </li>
                  );
                })}
              </ul>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default PasswordStrengthIndicator;