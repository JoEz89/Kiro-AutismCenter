import { useTranslation } from 'react-i18next';
import { useLanguage } from '@/context/LanguageContext';

export const useLocalization = () => {
  const { t, i18n } = useTranslation();
  const { 
    language, 
    direction, 
    isRTL, 
    changeLanguage, 
    formatNumber, 
    formatCurrency, 
    formatDate 
  } = useLanguage();

  // Helper function to get localized content
  const getLocalizedContent = (content: { en: string; ar: string }) => {
    return content[language] || content.en;
  };

  // Helper function to format relative time
  const formatRelativeTime = (date: Date | string): string => {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - dateObj.getTime()) / 1000);
    
    const rtf = new Intl.RelativeTimeFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      numeric: 'auto',
    });

    if (diffInSeconds < 60) {
      return rtf.format(-diffInSeconds, 'second');
    } else if (diffInSeconds < 3600) {
      return rtf.format(-Math.floor(diffInSeconds / 60), 'minute');
    } else if (diffInSeconds < 86400) {
      return rtf.format(-Math.floor(diffInSeconds / 3600), 'hour');
    } else if (diffInSeconds < 2592000) {
      return rtf.format(-Math.floor(diffInSeconds / 86400), 'day');
    } else if (diffInSeconds < 31536000) {
      return rtf.format(-Math.floor(diffInSeconds / 2592000), 'month');
    } else {
      return rtf.format(-Math.floor(diffInSeconds / 31536000), 'year');
    }
  };

  // Helper function to format time
  const formatTime = (date: Date | string, options?: Intl.DateTimeFormatOptions): string => {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const locale = language === 'ar' ? 'ar-BH' : 'en-US';
    
    const defaultOptions: Intl.DateTimeFormatOptions = {
      hour: '2-digit',
      minute: '2-digit',
      ...options,
    };
    
    const formatted = new Intl.DateTimeFormat(locale, defaultOptions).format(dateObj);
    
    // For Arabic, convert numbers to Arabic-Indic numerals
    if (language === 'ar') {
      return formatted.replace(/\d/g, (digit) => {
        const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
        return arabicNumerals[parseInt(digit)];
      });
    }
    
    return formatted;
  };

  // Helper function to format percentage
  const formatPercentage = (value: number, options?: Intl.NumberFormatOptions): string => {
    const locale = language === 'ar' ? 'ar-BH' : 'en-US';
    
    const formatted = new Intl.NumberFormat(locale, {
      style: 'percent',
      minimumFractionDigits: 0,
      maximumFractionDigits: 1,
      ...options,
    }).format(value / 100);
    
    // For Arabic, convert numbers to Arabic-Indic numerals
    if (language === 'ar') {
      return formatted.replace(/\d/g, (digit) => {
        const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
        return arabicNumerals[parseInt(digit)];
      });
    }
    
    return formatted;
  };

  // Helper function to get direction-aware class names
  const getDirectionClass = (ltrClass: string, rtlClass?: string): string => {
    if (isRTL) {
      return rtlClass || ltrClass.replace(/left/g, 'temp').replace(/right/g, 'left').replace(/temp/g, 'right');
    }
    return ltrClass;
  };

  // Helper function to get margin/padding classes for RTL
  const getSpacingClass = (spacing: string): string => {
    if (!isRTL) return spacing;
    
    // Split by spaces and process each class individually
    return spacing.split(' ').map(cls => {
      // Handle margin left/right
      if (cls.startsWith('ml-')) {
        return cls.replace('ml-', 'mr-');
      }
      if (cls.startsWith('mr-')) {
        return cls.replace('mr-', 'ml-');
      }
      // Handle padding left/right
      if (cls.startsWith('pl-')) {
        return cls.replace('pl-', 'pr-');
      }
      if (cls.startsWith('pr-')) {
        return cls.replace('pr-', 'pl-');
      }
      // Handle left/right positioning
      if (cls.includes('left-')) {
        return cls.replace('left-', 'right-');
      }
      if (cls.includes('right-')) {
        return cls.replace('right-', 'left-');
      }
      return cls;
    }).join(' ');
  };

  return {
    // Translation functions
    t,
    i18n,
    
    // Language context
    language,
    direction,
    isRTL,
    changeLanguage,
    
    // Formatting functions
    formatNumber,
    formatCurrency,
    formatDate,
    formatTime,
    formatRelativeTime,
    formatPercentage,
    
    // Helper functions
    getLocalizedContent,
    getDirectionClass,
    getSpacingClass,
  };
};

export default useLocalization;