import { render, screen } from '@testing-library/react';
import { LanguageProvider } from '@/context/LanguageContext';
import { useLocalization } from '../useLocalization';

import { vi } from 'vitest';

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: any) => {
      // Simple mock translation function
      const translations: Record<string, string> = {
        'home.welcome': 'Welcome to Autism Center',
        'language.switchTo': `Switch to ${options?.language || ''}`,
      };
      return translations[key] || key;
    },
    i18n: {
      changeLanguage: vi.fn(),
    },
  }),
}));

// Test component that uses the localization hook
const TestComponent = () => {
  const {
    t,
    language,
    direction,
    isRTL,
    formatNumber,
    formatCurrency,
    formatDate,
    formatTime,
    formatRelativeTime,
    formatPercentage,
    getLocalizedContent,
    getDirectionClass,
    getSpacingClass,
  } = useLocalization();

  const testDate = new Date('2024-01-15T10:30:00');
  const testContent = { en: 'English Content', ar: 'Arabic Content' };

  return (
    <div>
      <div data-testid="translation">{t('home.welcome')}</div>
      <div data-testid="language">{language}</div>
      <div data-testid="direction">{direction}</div>
      <div data-testid="isRTL">{isRTL.toString()}</div>
      <div data-testid="formatted-number">{formatNumber(1234)}</div>
      <div data-testid="formatted-currency">{formatCurrency(25.5, 'BHD')}</div>
      <div data-testid="formatted-date">{formatDate(testDate)}</div>
      <div data-testid="formatted-time">{formatTime(testDate)}</div>
      <div data-testid="formatted-percentage">{formatPercentage(75)}</div>
      <div data-testid="localized-content">{getLocalizedContent(testContent)}</div>
      <div data-testid="direction-class">{getDirectionClass('text-left')}</div>
      <div data-testid="spacing-class">{getSpacingClass('ml-4 pr-2')}</div>
    </div>
  );
};

const renderWithProvider = (component: React.ReactElement) => {
  return render(
    <LanguageProvider>
      {component}
    </LanguageProvider>
  );
};

describe('useLocalization', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.dir = 'ltr';
    document.documentElement.lang = 'en';
  });

  it('should provide translation function', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('translation')).toHaveTextContent('Welcome to Autism Center');
  });

  it('should provide language context values', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('language')).toHaveTextContent('en');
    expect(screen.getByTestId('direction')).toHaveTextContent('ltr');
    expect(screen.getByTestId('isRTL')).toHaveTextContent('false');
  });

  it('should format numbers correctly', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('formatted-number')).toHaveTextContent('1234');
  });

  it('should format currency correctly', () => {
    renderWithProvider(<TestComponent />);
    
    const currencyText = screen.getByTestId('formatted-currency').textContent;
    expect(currencyText).toContain('25.5');
  });

  it('should format percentage correctly', () => {
    renderWithProvider(<TestComponent />);
    
    const percentageText = screen.getByTestId('formatted-percentage').textContent;
    expect(percentageText).toContain('75');
  });

  it('should get localized content for English', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('localized-content')).toHaveTextContent('English Content');
  });

  it('should get localized content for Arabic', () => {
    localStorage.setItem('language', 'ar');
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('localized-content')).toHaveTextContent('Arabic Content');
  });

  it('should return correct direction class for LTR', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('direction-class')).toHaveTextContent('text-left');
  });

  it('should return correct spacing class for LTR', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('spacing-class')).toHaveTextContent('ml-4 pr-2');
  });

  it('should return correct spacing class for RTL', () => {
    localStorage.setItem('language', 'ar');
    renderWithProvider(<TestComponent />);
    
    // ml-4 should become mr-4, pr-2 should become pl-2
    expect(screen.getByTestId('spacing-class')).toHaveTextContent('mr-4 pl-2');
  });

  it('should format dates correctly', () => {
    renderWithProvider(<TestComponent />);
    
    const dateText = screen.getByTestId('formatted-date').textContent;
    expect(dateText).toBeTruthy();
    expect(dateText).toContain('2024');
  });

  it('should format time correctly', () => {
    renderWithProvider(<TestComponent />);
    
    const timeText = screen.getByTestId('formatted-time').textContent;
    expect(timeText).toBeTruthy();
    expect(timeText).toMatch(/\d{1,2}:\d{2}/); // Should match time format
  });

  it('should handle Arabic numerals for Arabic language', () => {
    localStorage.setItem('language', 'ar');
    renderWithProvider(<TestComponent />);
    
    const numberText = screen.getByTestId('formatted-number').textContent;
    // Should contain Arabic-Indic numerals
    expect(numberText).toMatch(/[٠-٩]/);
  });
});