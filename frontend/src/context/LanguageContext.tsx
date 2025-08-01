import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Language } from '@/types';

export type Direction = 'ltr' | 'rtl';

interface LanguageContextType {
  language: Language;
  direction: Direction;
  isRTL: boolean;
  changeLanguage: (lang: Language) => void;
  formatNumber: (num: number) => string;
  formatCurrency: (amount: number, currency?: 'BHD' | 'USD') => string;
  formatDate: (date: Date | string, options?: Intl.DateTimeFormatOptions) => string;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

interface LanguageProviderProps {
  children: ReactNode;
}

export const LanguageProvider: React.FC<LanguageProviderProps> = ({ children }) => {
  const { i18n } = useTranslation();
  const [language, setLanguage] = useState<Language>(() => {
    // Get language from localStorage or default to 'en'
    const savedLanguage = localStorage.getItem('language') as Language;
    return savedLanguage || 'en';
  });

  const direction: Direction = language === 'ar' ? 'rtl' : 'ltr';
  const isRTL = direction === 'rtl';

  useEffect(() => {
    // Change i18n language
    i18n.changeLanguage(language);
    
    // Update document direction and language
    document.documentElement.dir = direction;
    document.documentElement.lang = language;
    
    // Save to localStorage
    localStorage.setItem('language', language);
    
    // Update body class for styling
    document.body.classList.toggle('rtl', isRTL);
    document.body.classList.toggle('ltr', !isRTL);
  }, [language, direction, isRTL, i18n]);

  const changeLanguage = (lang: Language) => {
    setLanguage(lang);
  };

  // Format numbers according to language (Arabic numerals for Arabic)
  const formatNumber = (num: number): string => {
    if (language === 'ar') {
      // Convert to Arabic-Indic numerals
      return num.toString().replace(/\d/g, (digit) => {
        const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
        return arabicNumerals[parseInt(digit)];
      });
    }
    return num.toString();
  };

  // Format currency according to language conventions
  const formatCurrency = (amount: number, currency: 'BHD' | 'USD' = 'BHD'): string => {
    const locale = language === 'ar' ? 'ar-BH' : 'en-US';
    
    try {
      const formatted = new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: currency === 'BHD' ? 3 : 2,
        maximumFractionDigits: currency === 'BHD' ? 3 : 2,
      }).format(amount);
      
      // For Arabic, convert numbers to Arabic-Indic numerals
      if (language === 'ar') {
        return formatted.replace(/\d/g, (digit) => {
          const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
          return arabicNumerals[parseInt(digit)];
        });
      }
      
      return formatted;
    } catch (error) {
      // Fallback formatting
      const symbol = currency === 'BHD' ? (language === 'ar' ? 'د.ب' : 'BHD') : '$';
      const formattedAmount = formatNumber(amount);
      return language === 'ar' ? `${formattedAmount} ${symbol}` : `${symbol}${formattedAmount}`;
    }
  };

  // Format dates according to language conventions
  const formatDate = (date: Date | string, options?: Intl.DateTimeFormatOptions): string => {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const locale = language === 'ar' ? 'ar-BH' : 'en-US';
    
    const defaultOptions: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      ...options,
    };
    
    try {
      const formatted = new Intl.DateTimeFormat(locale, defaultOptions).format(dateObj);
      
      // For Arabic, convert numbers to Arabic-Indic numerals
      if (language === 'ar') {
        return formatted.replace(/\d/g, (digit) => {
          const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
          return arabicNumerals[parseInt(digit)];
        });
      }
      
      return formatted;
    } catch (error) {
      // Fallback formatting
      return dateObj.toLocaleDateString(locale);
    }
  };

  const value: LanguageContextType = {
    language,
    direction,
    isRTL,
    changeLanguage,
    formatNumber,
    formatCurrency,
    formatDate,
  };

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = (): LanguageContextType => {
  const context = useContext(LanguageContext);
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};

export { LanguageContext };