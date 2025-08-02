import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import ForgotPasswordForm from '../ForgotPasswordForm';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    forgotPassword: vi.fn(),
  },
}));

vi.mock('@/hooks/useLocalization', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'auth.forgotPassword': 'Forgot Password?',
        'auth.checkEmail': 'Check your email for reset instructions',
        'auth.email': 'Email',
        'auth.emailPlaceholder': 'Enter your email address',
        'auth.sendResetEmail': 'Send Reset Email',
        'auth.backToLogin': 'Back to Login',
        'auth.resetEmailSent': 'Reset email sent successfully',
        'auth.resetPasswordInstructions': 'Enter your email address and we\'ll send you a link to reset your password',
        'common.loading': 'Loading...',
        'validation.required': 'This field is required',
        'validation.invalidEmail': 'Please enter a valid email address',
        'errors.generic': 'Something went wrong. Please try again.',
      };
      return translations[key] || key;
    },
    direction: 'ltr',
  }),
}));

const renderForgotPasswordForm = (props = {}) => {
  return render(
    <BrowserRouter>
      <ForgotPasswordForm {...props} />
    </BrowserRouter>
  );
};

describe('ForgotPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders forgot password form with all required elements', () => {
    renderForgotPasswordForm();

    expect(screen.getByRole('heading', { name: /forgot password/i })).toBeInTheDocument();
    expect(screen.getByText(/check your email for reset instructions/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset email/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /back to login/i })).toBeInTheDocument();
  });

  it('displays validation error for empty email', async () => {
    renderForgotPasswordForm();

    const submitButton = screen.getByRole('button', { name: /send reset email/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('This field is required')).toBeInTheDocument();
    });
  });

  it('displays validation error for invalid email format', async () => {
    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Please enter a valid email address')).toBeInTheDocument();
    });
  });

  it('clears validation error when user starts typing', async () => {
    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

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

  it('shows success message when email is sent successfully', async () => {
    const mockForgotPassword = vi.fn().mockResolvedValue({});
    vi.mocked(require('@/services/authService').authService.forgotPassword).mockImplementation(mockForgotPassword);

    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Reset email sent successfully')).toBeInTheDocument();
    });

    expect(mockForgotPassword).toHaveBeenCalledWith('test@example.com');
  });

  it('shows error message when request fails', async () => {
    const mockForgotPassword = vi.fn().mockRejectedValue({
      response: { data: { message: 'User not found' } }
    });
    vi.mocked(require('@/services/authService').authService.forgotPassword).mockImplementation(mockForgotPassword);

    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('User not found')).toBeInTheDocument();
    });
  });

  it('shows loading state during submission', async () => {
    const mockForgotPassword = vi.fn().mockImplementation(() => 
      new Promise(resolve => setTimeout(resolve, 100))
    );
    vi.mocked(require('@/services/authService').authService.forgotPassword).mockImplementation(mockForgotPassword);

    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(submitButton);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /loading/i })).toBeDisabled();
  });

  it('has proper accessibility attributes', () => {
    renderForgotPasswordForm();

    const emailInput = screen.getByLabelText(/email/i);

    expect(emailInput).toHaveAttribute('type', 'email');
    expect(emailInput).toHaveAttribute('aria-invalid', 'false');
    expect(emailInput).toHaveAttribute('autoComplete', 'email');
    expect(emailInput).toHaveAttribute('aria-describedby', 'email-help');
  });

  it('disables submit button when email is empty', () => {
    renderForgotPasswordForm();

    const submitButton = screen.getByRole('button', { name: /send reset email/i });
    expect(submitButton).toBeDisabled();

    const emailInput = screen.getByLabelText(/email/i);
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    
    expect(submitButton).not.toBeDisabled();
  });

  it('has correct link to login page', () => {
    renderForgotPasswordForm();

    const loginLink = screen.getByRole('link', { name: /back to login/i });
    expect(loginLink).toHaveAttribute('href', '/login');
  });

  it('calls onSuccess callback when email is sent successfully', async () => {
    const mockOnSuccess = vi.fn();
    const mockForgotPassword = vi.fn().mockResolvedValue({});
    vi.mocked(require('@/services/authService').authService.forgotPassword).mockImplementation(mockForgotPassword);

    renderForgotPasswordForm({ onSuccess: mockOnSuccess });

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /send reset email/i });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnSuccess).toHaveBeenCalled();
    });
  });
});