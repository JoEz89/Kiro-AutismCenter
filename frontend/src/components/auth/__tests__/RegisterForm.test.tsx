import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import RegisterForm from '../RegisterForm';

// Mock the hooks
vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    register: vi.fn(),
    loginWithGoogle: vi.fn(),
    isLoading: false,
  }),
}));

vi.mock('@/hooks/useLocalization', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'auth.register': 'Register',
        'auth.createAccount': 'Create your account',
        'auth.firstName': 'First Name',
        'auth.lastName': 'Last Name',
        'auth.email': 'Email',
        'auth.password': 'Password',
        'auth.confirmPassword': 'Confirm Password',
        'auth.preferredLanguage': 'Preferred Language',
        'auth.agreeToTerms': 'I agree to the Terms of Service and Privacy Policy',
        'auth.termsOfService': 'Terms of Service',
        'auth.privacyPolicy': 'Privacy Policy',
        'auth.registerWithGoogle': 'Register with Google',
        'auth.alreadyHaveAccount': 'Already have an account?',
        'auth.login': 'Login',
        'auth.emailPlaceholder': 'Enter your email address',
        'auth.passwordPlaceholder': 'Enter your password',
        'auth.orContinueWith': 'Or continue with',
        'auth.passwordStrength': 'Password Strength',
        'auth.passwordRequirements': 'Password must be at least 8 characters with uppercase, lowercase, and number',
        'auth.weak': 'Weak',
        'auth.medium': 'Medium',
        'auth.strong': 'Strong',
        'common.loading': 'Loading...',
        'common.and': 'and',
        'validation.required': 'This field is required',
        'validation.invalidEmail': 'Please enter a valid email address',
        'validation.passwordTooShort': 'Password must be at least 8 characters',
        'validation.passwordRequirements': 'Password must contain uppercase, lowercase, and number',
        'validation.passwordMismatch': 'Passwords do not match',
      };
      return translations[key] || key;
    },
    direction: 'ltr',
    language: 'en',
  }),
}));

const renderRegisterForm = (props = {}) => {
  return render(
    <BrowserRouter>
      <RegisterForm {...props} />
    </BrowserRouter>
  );
};

describe('RegisterForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders registration form with all required fields', () => {
    renderRegisterForm();

    expect(screen.getByRole('heading', { name: /register/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/preferred language/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument();
    expect(screen.getByRole('checkbox')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^register$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /register with google/i })).toBeInTheDocument();
  });

  it('displays validation errors for empty required fields', async () => {
    renderRegisterForm();

    const submitButton = screen.getByRole('button', { name: /^register$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      // Should show errors for firstName, lastName, email, password, confirmPassword, and terms
      expect(screen.getAllByText('This field is required')).toHaveLength(6);
    });
  });

  it('validates email format', async () => {
    renderRegisterForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /^register$/i });

    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Please enter a valid email address')).toBeInTheDocument();
    });
  });

  it('validates password requirements', async () => {
    renderRegisterForm();

    const passwordInput = screen.getByLabelText(/^password$/i);
    const submitButton = screen.getByRole('button', { name: /^register$/i });

    // Test short password
    fireEvent.change(passwordInput, { target: { value: '123' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Password must be at least 8 characters')).toBeInTheDocument();
    });

    // Test password without requirements
    fireEvent.change(passwordInput, { target: { value: 'password' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Password must contain uppercase, lowercase, and number')).toBeInTheDocument();
    });
  });

  it('validates password confirmation match', async () => {
    renderRegisterForm();

    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i);
    const submitButton = screen.getByRole('button', { name: /^register$/i });

    fireEvent.change(passwordInput, { target: { value: 'Password123' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'DifferentPassword123' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Passwords do not match')).toBeInTheDocument();
    });
  });

  it('shows password strength indicator', () => {
    renderRegisterForm();

    const passwordInput = screen.getByLabelText(/^password$/i);

    // Test weak password
    fireEvent.change(passwordInput, { target: { value: 'weak' } });
    expect(screen.getByText('Weak')).toBeInTheDocument();

    // Test strong password
    fireEvent.change(passwordInput, { target: { value: 'StrongPassword123' } });
    expect(screen.getByText('Strong')).toBeInTheDocument();
  });

  it('clears validation errors when user starts typing', async () => {
    renderRegisterForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /^register$/i });

    // Trigger validation error
    fireEvent.click(submitButton);
    await waitFor(() => {
      expect(screen.getByText('This field is required')).toBeInTheDocument();
    });

    // Start typing to clear error
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    
    await waitFor(() => {
      const requiredErrors = screen.queryAllByText('This field is required');
      // Should have one less error after typing in email field
      expect(requiredErrors.length).toBeLessThan(6);
    });
  });

  it('has proper accessibility attributes', () => {
    renderRegisterForm();

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

    expect(emailInput).toHaveAttribute('type', 'email');
    expect(emailInput).toHaveAttribute('aria-invalid', 'false');
    expect(passwordInput).toHaveAttribute('type', 'password');
    expect(passwordInput).toHaveAttribute('aria-invalid', 'false');
    expect(confirmPasswordInput).toHaveAttribute('type', 'password');
    expect(confirmPasswordInput).toHaveAttribute('aria-invalid', 'false');
  });

  it('shows loading state when submitting', () => {
    vi.mocked(require('@/hooks/useAuth').useAuth).mockReturnValue({
      register: vi.fn(),
      loginWithGoogle: vi.fn(),
      isLoading: true,
    });

    renderRegisterForm();

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /loading/i })).toBeDisabled();
  });

  it('has proper links to login and terms', () => {
    renderRegisterForm();

    expect(screen.getByRole('link', { name: /login/i })).toHaveAttribute('href', '/login');
    expect(screen.getByRole('link', { name: /terms of service/i })).toHaveAttribute('href', '/terms');
    expect(screen.getByRole('link', { name: /privacy policy/i })).toHaveAttribute('href', '/privacy');
  });

  it('allows language selection', () => {
    renderRegisterForm();

    const languageSelect = screen.getByLabelText(/preferred language/i);
    
    expect(languageSelect).toHaveValue('en');
    
    fireEvent.change(languageSelect, { target: { value: 'ar' } });
    expect(languageSelect).toHaveValue('ar');
  });

  it('requires terms agreement', async () => {
    renderRegisterForm();

    const termsCheckbox = screen.getByRole('checkbox');
    const submitButton = screen.getByRole('button', { name: /^register$/i });

    expect(termsCheckbox).not.toBeChecked();
    
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('This field is required')).toBeInTheDocument();
    });

    fireEvent.click(termsCheckbox);
    expect(termsCheckbox).toBeChecked();
  });
});