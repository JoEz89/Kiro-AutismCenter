import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LanguageProvider } from '@/context/LanguageContext';
import LanguageSwitcher from '../LanguageSwitcher';

import { vi } from 'vitest';

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: any) => {
      const translations: Record<string, string> = {
        'language.switchTo': `Switch to ${options?.language || ''}`,
      };
      return translations[key] || key;
    },
    i18n: {
      changeLanguage: vi.fn(),
    },
  }),
}));

const renderWithProvider = (component: React.ReactElement) => {
  return render(
    <LanguageProvider>
      {component}
    </LanguageProvider>
  );
};

describe('LanguageSwitcher', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.dir = 'ltr';
    document.documentElement.lang = 'en';
    document.body.className = '';
  });

  describe('button variant', () => {
    it('should render both language buttons', () => {
      renderWithProvider(<LanguageSwitcher variant="button" />);
      
      expect(screen.getByText('English')).toBeInTheDocument();
      expect(screen.getByText('العربية')).toBeInTheDocument();
    });

    it('should highlight current language', () => {
      renderWithProvider(<LanguageSwitcher variant="button" />);
      
      const englishButton = screen.getByText('English');
      const arabicButton = screen.getByText('العربية');
      
      expect(englishButton).toHaveClass('bg-blue-600');
      expect(arabicButton).not.toHaveClass('bg-blue-600');
    });

    it('should change language when clicked', async () => {
      const user = userEvent.setup();
      renderWithProvider(<LanguageSwitcher variant="button" />);
      
      const arabicButton = screen.getByText('العربية');
      await user.click(arabicButton);
      
      await waitFor(() => {
        expect(document.documentElement.dir).toBe('rtl');
        expect(document.documentElement.lang).toBe('ar');
      });
    });
  });

  describe('toggle variant', () => {
    it('should render current language name', () => {
      renderWithProvider(<LanguageSwitcher variant="toggle" />);
      
      expect(screen.getByText('English')).toBeInTheDocument();
    });

    it('should toggle to Arabic when clicked', async () => {
      const user = userEvent.setup();
      renderWithProvider(<LanguageSwitcher variant="toggle" />);
      
      const toggleButton = screen.getByRole('button');
      await user.click(toggleButton);
      
      await waitFor(() => {
        expect(screen.getByText('العربية')).toBeInTheDocument();
        expect(document.documentElement.dir).toBe('rtl');
      });
    });

    it('should have proper aria-label', () => {
      renderWithProvider(<LanguageSwitcher variant="toggle" />);
      
      const toggleButton = screen.getByRole('button');
      expect(toggleButton).toHaveAttribute('aria-label', 'Switch to Arabic');
    });
  });

  describe('dropdown variant', () => {
    it('should render select element with both options', () => {
      renderWithProvider(<LanguageSwitcher variant="dropdown" />);
      
      const select = screen.getByRole('combobox');
      expect(select).toBeInTheDocument();
      
      const englishOption = screen.getByDisplayValue('English');
      const arabicOption = screen.getByText('العربية');
      
      expect(englishOption).toBeInTheDocument();
      expect(arabicOption).toBeInTheDocument();
    });

    it('should change language when option is selected', async () => {
      const user = userEvent.setup();
      renderWithProvider(<LanguageSwitcher variant="dropdown" />);
      
      const select = screen.getByRole('combobox');
      await user.selectOptions(select, 'ar');
      
      await waitFor(() => {
        expect(document.documentElement.dir).toBe('rtl');
        expect(document.documentElement.lang).toBe('ar');
      });
    });

    it('should show current language as selected', () => {
      localStorage.setItem('language', 'ar');
      renderWithProvider(<LanguageSwitcher variant="dropdown" />);
      
      const select = screen.getByRole('combobox') as HTMLSelectElement;
      expect(select.value).toBe('ar');
    });
  });

  describe('accessibility', () => {
    it('should have proper aria attributes for button variant', () => {
      renderWithProvider(<LanguageSwitcher variant="button" />);
      
      const englishButton = screen.getByText('English');
      const arabicButton = screen.getByText('العربية');
      
      expect(englishButton).toHaveAttribute('aria-pressed', 'true');
      expect(arabicButton).toHaveAttribute('aria-pressed', 'false');
    });

    it('should be keyboard accessible', async () => {
      const user = userEvent.setup();
      renderWithProvider(<LanguageSwitcher variant="button" />);
      
      const arabicButton = screen.getByText('العربية');
      
      // Focus and activate with keyboard
      arabicButton.focus();
      await user.keyboard('{Enter}');
      
      await waitFor(() => {
        expect(document.documentElement.lang).toBe('ar');
      });
    });
  });

  describe('custom className', () => {
    it('should apply custom className', () => {
      renderWithProvider(<LanguageSwitcher className="custom-class" variant="button" />);
      
      const container = screen.getByText('English').parentElement;
      expect(container).toHaveClass('custom-class');
    });
  });
});