import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider, useTheme } from '../ThemeContext';

// Test component that uses the theme context
const TestComponent = () => {
  const { theme, toggleTheme, setTheme } = useTheme();

  return (
    <div>
      <div data-testid="theme">{theme}</div>
      <button onClick={toggleTheme} data-testid="toggle-theme">
        Toggle Theme
      </button>
      <button onClick={() => setTheme('dark')} data-testid="set-dark">
        Set Dark
      </button>
      <button onClick={() => setTheme('light')} data-testid="set-light">
        Set Light
      </button>
    </div>
  );
};

const renderWithProvider = (component: React.ReactElement) => {
  return render(
    <ThemeProvider>
      {component}
    </ThemeProvider>
  );
};

import { vi } from 'vitest';

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

describe('ThemeContext', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    // Reset document classes
    document.documentElement.className = '';
  });

  it('should provide default theme as light', () => {
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('theme')).toHaveTextContent('light');
  });

  it('should toggle theme from light to dark', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('theme')).toHaveTextContent('light');
    
    await user.click(screen.getByTestId('toggle-theme'));
    
    await waitFor(() => {
      expect(screen.getByTestId('theme')).toHaveTextContent('dark');
    });
  });

  it('should toggle theme from dark to light', async () => {
    const user = userEvent.setup();
    localStorage.setItem('theme', 'dark');
    
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('theme')).toHaveTextContent('dark');
    
    await user.click(screen.getByTestId('toggle-theme'));
    
    await waitFor(() => {
      expect(screen.getByTestId('theme')).toHaveTextContent('light');
    });
  });

  it('should set theme to dark', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('set-dark'));
    
    await waitFor(() => {
      expect(screen.getByTestId('theme')).toHaveTextContent('dark');
    });
  });

  it('should set theme to light', async () => {
    const user = userEvent.setup();
    localStorage.setItem('theme', 'dark');
    
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('set-light'));
    
    await waitFor(() => {
      expect(screen.getByTestId('theme')).toHaveTextContent('light');
    });
  });

  it('should update document class when theme changes', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    // Initially should not have dark class
    expect(document.documentElement.classList.contains('dark')).toBe(false);
    
    await user.click(screen.getByTestId('set-dark'));
    
    await waitFor(() => {
      expect(document.documentElement.classList.contains('dark')).toBe(true);
    });
    
    await user.click(screen.getByTestId('set-light'));
    
    await waitFor(() => {
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });
  });

  it('should save theme preference to localStorage', async () => {
    const user = userEvent.setup();
    renderWithProvider(<TestComponent />);
    
    await user.click(screen.getByTestId('set-dark'));
    
    await waitFor(() => {
      expect(localStorage.getItem('theme')).toBe('dark');
    });
  });

  it('should load theme preference from localStorage', () => {
    localStorage.setItem('theme', 'dark');
    
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('theme')).toHaveTextContent('dark');
  });

  it('should use system preference when no localStorage value', () => {
    // Mock system preference for dark mode
    window.matchMedia = vi.fn().mockImplementation(query => ({
      matches: query === '(prefers-color-scheme: dark)',
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }));
    
    renderWithProvider(<TestComponent />);
    
    expect(screen.getByTestId('theme')).toHaveTextContent('dark');
  });

  it('should throw error when used outside provider', () => {
    // Suppress console.error for this test
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => {
      render(<TestComponent />);
    }).toThrow('useTheme must be used within a ThemeProvider');
    
    consoleSpy.mockRestore();
  });
});