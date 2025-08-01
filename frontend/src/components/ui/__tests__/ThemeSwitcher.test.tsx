import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@/context/ThemeContext';
import ThemeSwitcher from '../ThemeSwitcher';

import { vi } from 'vitest';

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

const renderWithProvider = (component: React.ReactElement) => {
  return render(
    <ThemeProvider>
      {component}
    </ThemeProvider>
  );
};

describe('ThemeSwitcher', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.className = '';
  });

  describe('icon variant (default)', () => {
    it('should render moon icon for light theme', () => {
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      expect(button).toBeInTheDocument();
      
      // Check for moon icon (dark mode icon when in light theme)
      const moonIcon = button.querySelector('svg path[d*="20.354"]');
      expect(moonIcon).toBeInTheDocument();
    });

    it('should render sun icon for dark theme', () => {
      localStorage.setItem('theme', 'dark');
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      
      // Check for sun icon (light mode icon when in dark theme)
      const sunIcon = button.querySelector('svg path[d*="M12 3v1m0 16v1"]');
      expect(sunIcon).toBeInTheDocument();
    });

    it('should toggle theme when clicked', async () => {
      const user = userEvent.setup();
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      
      // Initially should not have dark class
      expect(document.documentElement.classList.contains('dark')).toBe(false);
      
      await user.click(button);
      
      await waitFor(() => {
        expect(document.documentElement.classList.contains('dark')).toBe(true);
      });
    });

    it('should have proper aria-label for light theme', () => {
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', 'Switch to dark theme');
    });

    it('should have proper aria-label for dark theme', () => {
      localStorage.setItem('theme', 'dark');
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', 'Switch to light theme');
    });
  });

  describe('button variant', () => {
    it('should render "Dark" text for light theme', () => {
      renderWithProvider(<ThemeSwitcher variant="button" />);
      
      expect(screen.getByText('Dark')).toBeInTheDocument();
    });

    it('should render "Light" text for dark theme', () => {
      localStorage.setItem('theme', 'dark');
      renderWithProvider(<ThemeSwitcher variant="button" />);
      
      expect(screen.getByText('Light')).toBeInTheDocument();
    });

    it('should toggle theme when clicked', async () => {
      const user = userEvent.setup();
      renderWithProvider(<ThemeSwitcher variant="button" />);
      
      const button = screen.getByRole('button');
      
      expect(document.documentElement.classList.contains('dark')).toBe(false);
      
      await user.click(button);
      
      await waitFor(() => {
        expect(document.documentElement.classList.contains('dark')).toBe(true);
        expect(screen.getByText('Light')).toBeInTheDocument();
      });
    });

    it('should show both icon and text', () => {
      renderWithProvider(<ThemeSwitcher variant="button" />);
      
      const button = screen.getByRole('button');
      const icon = button.querySelector('svg');
      const text = screen.getByText('Dark');
      
      expect(icon).toBeInTheDocument();
      expect(text).toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('should be keyboard accessible', async () => {
      const user = userEvent.setup();
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      
      // Focus and activate with keyboard
      button.focus();
      await user.keyboard('{Enter}');
      
      await waitFor(() => {
        expect(document.documentElement.classList.contains('dark')).toBe(true);
      });
    });

    it('should have focus styles', () => {
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      expect(button).toHaveClass('focus:outline-none', 'focus:ring-2', 'focus:ring-blue-500');
    });
  });

  describe('custom className', () => {
    it('should apply custom className', () => {
      renderWithProvider(<ThemeSwitcher className="custom-class" />);
      
      const button = screen.getByRole('button');
      expect(button).toHaveClass('custom-class');
    });
  });

  describe('theme persistence', () => {
    it('should save theme to localStorage when toggled', async () => {
      const user = userEvent.setup();
      renderWithProvider(<ThemeSwitcher />);
      
      const button = screen.getByRole('button');
      await user.click(button);
      
      await waitFor(() => {
        expect(localStorage.getItem('theme')).toBe('dark');
      });
    });

    it('should load theme from localStorage on mount', () => {
      localStorage.setItem('theme', 'dark');
      renderWithProvider(<ThemeSwitcher variant="button" />);
      
      expect(screen.getByText('Light')).toBeInTheDocument();
      expect(document.documentElement.classList.contains('dark')).toBe(true);
    });
  });
});