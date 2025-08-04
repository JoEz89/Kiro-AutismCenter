import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { LoginForm } from '../LoginForm';
import { AuthProvider } from '@/context/AuthContext';
import { LanguageProvider } from '@/context/LanguageContext';
import { testAccessibility } from '@/test/accessibility';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    loginWithGoogle: vi.fn(),
  },
}));

const TestWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>
    <LanguageProvider>
      <AuthProvider>
        {children}
      </AuthProvider>
    </LanguageProvider>
  </BrowserRouter>
);

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render login form with all required fields', () => {
    const renderResult = render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /continue with google/i })).toBeInTheDocument();
  });

  it('should validate required fields', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const submitButton = screen.getByRole('button', { name: /sign in/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
      expect(screen.getByText(/password is required/i)).toBeInTheDocument();
    });
  });

  it('should validate email format', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const emailInput = screen.getByLabelText(/email/i);
    await user.type(emailInput, 'invalid-email');
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/please enter a valid email address/i)).toBeInTheDocument();
    });
  });

  it('should handle successful login', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockResolvedValue({});
    
    // Mock the useAuth hook
    vi.doMock('@/hooks/useAuth', () => ({
      useAuth: () => ({
        login: mockLogin,
        isLoading: false,
      }),
    }));

    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /sign in/i });

    await user.type(emailInput, 'test@example.com');
    await user.type(passwordInput, 'password123');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
    });
  });

  it('should display loading state during login', async () => {
    const user = userEvent.setup();
    
    // Mock loading state
    vi.doMock('@/hooks/useAuth', () => ({
      useAuth: () => ({
        login: vi.fn(),
        isLoading: true,
      }),
    }));

    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const submitButton = screen.getByRole('button', { name: /signing in/i });
    expect(submitButton).toBeDisabled();
  });

  it('should handle login error', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockRejectedValue(new Error('Invalid credentials'));
    
    vi.doMock('@/hooks/useAuth', () => ({
      useAuth: () => ({
        login: mockLogin,
        isLoading: false,
      }),
    }));

    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /sign in/i });

    await user.type(emailInput, 'test@example.com');
    await user.type(passwordInput, 'wrongpassword');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
    });
  });

  it('should be accessible', async () => {
    const renderResult = render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    await testAccessibility(renderResult);
  });

  it('should support keyboard navigation', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /sign in/i });

    // Test tab navigation
    await user.tab();
    expect(emailInput).toHaveFocus();

    await user.tab();
    expect(passwordInput).toHaveFocus();

    await user.tab();
    expect(submitButton).toHaveFocus();
  });

  it('should toggle password visibility', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <LoginForm />
      </TestWrapper>
    );

    const passwordInput = screen.getByLabelText(/password/i);
    const toggleButton = screen.getByRole('button', { name: /show password/i });

    expect(passwordInput).toHaveAttribute('type', 'password');

    await user.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'text');
    expect(screen.getByRole('button', { name: /hide password/i })).toBeInTheDocument();

    await user.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'password');
  });
});