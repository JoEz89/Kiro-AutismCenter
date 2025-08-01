import React from 'react';
import { useTranslation } from 'react-i18next';
import { useLanguage } from '@/context/LanguageContext';
import { Language } from '@/types';
import { cn } from '@/lib/utils';

interface LanguageSwitcherProps {
  className?: string;
  variant?: 'button' | 'dropdown' | 'toggle';
}

export const LanguageSwitcher: React.FC<LanguageSwitcherProps> = ({ 
  className,
  variant = 'button' 
}) => {
  const { t } = useTranslation();
  const { language, changeLanguage, isRTL } = useLanguage();

  const languages: { code: Language; name: string; nativeName: string }[] = [
    { code: 'en', name: 'English', nativeName: 'English' },
    { code: 'ar', name: 'Arabic', nativeName: 'العربية' },
  ];

  const currentLanguage = languages.find(lang => lang.code === language);
  const otherLanguage = languages.find(lang => lang.code !== language);

  if (variant === 'toggle') {
    return (
      <button
        onClick={() => changeLanguage(otherLanguage!.code)}
        className={cn(
          'flex items-center gap-2 px-3 py-2 rounded-md transition-colors',
          'hover:bg-gray-100 dark:hover:bg-gray-800',
          'focus:outline-none focus:ring-2 focus:ring-blue-500',
          className
        )}
        aria-label={t('language.switchTo', { language: otherLanguage?.name })}
      >
        <span className="text-sm font-medium">
          {currentLanguage?.nativeName}
        </span>
        <svg
          className={cn(
            'w-4 h-4 transition-transform',
            isRTL && 'rotate-180'
          )}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M8 9l4-4 4 4m0 6l-4 4-4-4"
          />
        </svg>
      </button>
    );
  }

  if (variant === 'dropdown') {
    return (
      <div className={cn('relative', className)}>
        <select
          value={language}
          onChange={(e) => changeLanguage(e.target.value as Language)}
          className={cn(
            'appearance-none bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600',
            'rounded-md px-3 py-2 pr-8 text-sm font-medium',
            'focus:outline-none focus:ring-2 focus:ring-blue-500',
            'cursor-pointer',
            isRTL && 'text-right'
          )}
          aria-label={t('language.switchTo', { language: '' })}
        >
          {languages.map((lang) => (
            <option key={lang.code} value={lang.code}>
              {lang.nativeName}
            </option>
          ))}
        </select>
        <div className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
          <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>
    );
  }

  // Default button variant
  return (
    <div className={cn('flex gap-1', className)}>
      {languages.map((lang) => (
        <button
          key={lang.code}
          onClick={() => changeLanguage(lang.code)}
          className={cn(
            'px-3 py-2 text-sm font-medium rounded-md transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-blue-500',
            language === lang.code
              ? 'bg-blue-600 text-white'
              : 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700'
          )}
          aria-label={t('language.switchTo', { language: lang.name })}
          aria-pressed={language === lang.code}
        >
          {lang.nativeName}
        </button>
      ))}
    </div>
  );
};

export default LanguageSwitcher;