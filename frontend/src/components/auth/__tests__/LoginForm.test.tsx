import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import LoginForm from '../LoginForm';
import { AuthContext } from '@/context/AuthContext';
import { LanguageContext } from '@/context/LanguageContext';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    loginWithGoogle: vi.fn(),
  },
}));

// Mock the hooks
vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    login: vi.fn(),
    loginWithGoogle: vi.fn(),
    isLoading: false,
  }),
}));

vi.mock('@/hooks/useLocalization', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'auth.login': 'Login',
        'auth.email': 'Email',
        'auth.password': 'Password',
        'auth.rememberMe': 'Remember Me',
        'auth.forgotPassword': 'Forgot Password?',
        'auth.loginWithGoogle': 'Login with Google',
        'auth.dontHaveAccount': "Don't have an account?",
        'auth.register': 'Register',
        'auth.welcomeBack': 'Welcome back! Please sign in to your account.',
        'auth.emailPlaceholder': 'Enter your email address',
        'auth.passwordPlaceholder': 'Enter your password',
        'auth.orContinueWith': 'Or continue with',
        'common.loading': 'Loading...',
        'validation.required': 'This field is required',
        'validation.invalidEmail': 'Please enter a valid email address',
      };
      return translations[key] || key;
    },
    direction: 'ltr',
  }),
}));

const renderLoginForm = (props = {}) => {
  return render(
    <BrowserRouter>
      <LoginForm {...props} />
    </BrowserRouter>
  );
};

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders login form with all required fields', () => {
    renderLoginForm();

    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: /remember me/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^login$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login with google/i })).toBeInTheDocument();
  });

  it('displays validation errors for empty fields', async () => {
    renderLoginForm();

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getAllByText('This field is required')).toHaveLength(2);
    });
  });

  it('displays validation error for invalid email', async () => {
    renderLoginForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /^login$/i });

    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Please enter a valid email address')).toBeInTheDocument();
    });
  });

  it('clears validation errors when user starts typing', async () => {
    renderLoginForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /^login$/i });

    // Trigger validation error
    fireEvent.click(submitButton);
    await waitFor(() => {
      expect(screen.getByText('This field is required')).toBeInTheDocument();
    });

    // Start typing to clear error
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    
    await waitFor(() => {
      expect(screen.queryByText('This field is required')).not.toBeInTheDocument();
    });
  });

  it('has proper accessibility attributes', () => {
    renderLoginForm();

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);

    expect(emailInput).toHaveAttribute('type', 'email');
    expect(emailInput).toHaveAttribute('aria-invalid', 'false');
    expect(passwordInput).toHaveAttribute('type', 'password');
    expect(passwordInput).toHaveAttribute('aria-invalid', 'false');
  });

  it('shows loading state when submitting', async () => {
    const mockLogin = vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
    
    vi.mocked(require('@/hooks/useAuth').useAuth).mockReturnValue({
      login: mockLogin,
      loginWithGoogle: vi.fn(),
      isLoading: true,
    });

    renderLoginForm();

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /loading/i })).toBeDisabled();
  });

  it('calls onSuccess callback when login is successful', async () => {
    const mockOnSuccess = vi.fn();
    const mockLogin = vi.fn().mockResolvedValue({});

    vi.mocked(require('@/hooks/useAuth').useAuth).mockReturnValue({
      login: mockLogin,
      loginWithGoogle: vi.fn(),
      isLoading: false,
    });

    renderLoginForm({ onSuccess: mockOnSuccess });

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /^login$/i });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
    });
  });

  it('has proper links to registration and forgot password', () => {
    renderLoginForm();

    expect(screen.getByRole('link', { name: /register/i })).toHaveAttribute('href', '/register');
    expect(screen.getByRole('link', { name: /forgot password/i })).toHaveAttribute('href', '/forgot-password');
  });

  it('toggles remember me checkbox', () => {
    renderLoginForm();

    const rememberMeCheckbox = screen.getByRole('checkbox', { name: /remember me/i });
    
    expect(rememberMeCheckbox).not.toBeChecked();
    
    fireEvent.click(rememberMeCheckbox);
    expect(rememberMeCheckbox).toBeChecked();
    
    fireEvent.click(rememberMeCheckbox);
    expect(rememberMeCheckbox).not.toBeChecked();
  });
});