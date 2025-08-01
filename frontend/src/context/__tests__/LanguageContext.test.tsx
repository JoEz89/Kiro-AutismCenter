import { render, screen, act, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LanguageProvider, useLanguage } from '../LanguageContext';
import { Language } from '@/types';

import { vi } from 'vitest';

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    i18n: {
      changeLanguage: vi.fn(),
    },
  }),
}));

// Test component that uses the language context
const TestComponent = () => {
  const { 
    language, 
    direction, 
    isRTL, 
    changeLanguage, 
    formatNumber, 
    formatCurrency, 
    formatDate 
  } = useLanguage();

  return (
    <div>
      <div data-testid="language">{language}</div>
      <div data-testid="direction">{direction}</div>
      <div data-testid="isRTL">{isRTL.toString()}</div>
      <div data-testid="formatted-number">{formatNumber(1234)}</div>
      <div data-testid="formatted-currency">{formatCurrency(25.5, 'BHD')}</div>
      <div data-testid="formatted-date">{formatDate(new Date('2024-01-15'))}</div>
      <button onClick={() => changeLanguage('ar')} data-testid="change-to-arabic">
        Change to Arabic
      </button>
      <button onClick={() => changeLanguage('en')} data-testid="change-to-english">
        Change to English
      </button>
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

describe('LanguageContext', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    // Reset document properties
    document.documentElement.dir = 'ltr';
    document.documentElement.lang = 'en';
    document.body.className = '';
  });

  it('should provide default language as English', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('language')).toHaveTextContent('en');
    expect(screen.getByTestId('direction')).toHaveTextContent('ltr');
    expect(screen.getByTestId('isRTL')).toHaveTextContent('false');
  });

  it('should change language to Arabic and update direction', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      expect(screen.getByTestId('language')).toHaveTextContent('ar');
      expect(screen.getByTestId('direction')).toHaveTextContent('rtl');
      expect(screen.getByTestId('isRTL')).toHaveTextContent('true');
    });
  });

  it('should format numbers correctly for Arabic', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      // Arabic-Indic numerals for 1234 should be ١٢٣٤
      expect(screen.getByTestId('formatted-number')).toHaveTextContent('١٢٣٤');
    });
  });

  it('should format numbers correctly for English', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('formatted-number')).toHaveTextContent('1234');
  });

  it('should format currency correctly for both languages', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    // Test English currency formatting
    const englishCurrency = screen.getByTestId('formatted-currency');
    expect(englishCurrency.textContent).toContain('25.5');
    
    // Change to Arabic and test Arabic currency formatting
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      const arabicCurrency = screen.getByTestId('formatted-currency');
      // Should contain Arabic-Indic numerals
      expect(arabicCurrency.textContent).toMatch(/[٠-٩]/);
    });
  });

  it('should update document properties when language changes', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    // Initially should be LTR
    expect(document.documentElement.dir).toBe('ltr');
    expect(document.documentElement.lang).toBe('en');
    
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      expect(document.documentElement.dir).toBe('rtl');
      expect(document.documentElement.lang).toBe('ar');
    });
  });

  it('should save language preference to localStorage', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      expect(localStorage.getItem('language')).toBe('ar');
    });
  });

  it('should load language preference from localStorage', () => {
    localStorage.setItem('language', 'ar');
    
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('language')).toHaveTextContent('ar');
    expect(screen.getByTestId('direction')).toHaveTextContent('rtl');
  });

  it('should update body classes for RTL/LTR', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    // Initially should have ltr class
    expect(document.body.classList.contains('ltr')).toBe(true);
    expect(document.body.classList.contains('rtl')).toBe(false);
    
    await user.click(screen.getByTestId('change-to-arabic'));
    
    await waitFor(() => {
      expect(document.body.classList.contains('rtl')).toBe(true);
      expect(document.body.classList.contains('ltr')).toBe(false);
    });
  });

  it('should throw error when used outside provider', () => {
    // Suppress console.error for this test
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => {
      render(<TestComponent />);
    }).toThrow('useLanguage must be used within a LanguageProvider');
    
    consoleSpy.mockRestore();
  });
});