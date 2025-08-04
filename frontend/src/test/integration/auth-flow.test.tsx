import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { App } from '@/App';
import { authService } from '@/services/authService';
import { User, UserRole } from '@/types';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    verifyToken: vi.fn(),
    logout: vi.fn(),
    forgotPassword: vi.fn(),
    resetPassword: vi.fn(),
  },
}));

const mockAuthService = vi.mocked(authService);

describe('Authentication Flow Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  describe('User Registration Flow', () => {
    it('should complete full registration flow', async () => {
      const user = userEvent.setup();

      // Mock successful registration
      mockAuthService.register.mockResolvedValueOnce({
        data: { message: 'Registration successful. Please check your email for verification.' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/register']}>
          <App />
        </MemoryRouter>
      );

      // Fill registration form
      const firstNameInput = screen.getByLabelText(/first name/i);
      const lastNameInput = screen.getByLabelText(/last name/i);
      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await user.type(firstNameInput, 'John');
      await user.type(lastNameInput, 'Doe');
      await user.type(emailInput, 'john.doe@example.com');
      await user.type(passwordInput, 'SecurePass123!');
      await user.type(confirmPasswordInput, 'SecurePass123!');

      // Submit form
      const submitButton = screen.getByRole('button', { name: /create account/i });
      await user.click(submitButton);

      // Verify registration was called with correct data
      await waitFor(() => {
        expect(mockAuthService.register).toHaveBeenCalledWith({
          firstName: 'John',
          lastName: 'Doe',
          email: 'john.doe@example.com',
          password: 'SecurePass123!',
          preferredLanguage: 'en',
        });
      });

      // Should show success message
      expect(screen.getByText(/registration successful/i)).toBeInTheDocument();
      expect(screen.getByText(/please check your email/i)).toBeInTheDocument();
    });

    it('should handle registration validation errors', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter initialEntries={['/register']}>
          <App />
        </MemoryRouter>
      );

      // Try to submit empty form
      const submitButton = screen.getByRole('button', { name: /create account/i });
      await user.click(submitButton);

      // Should show validation errors
      await waitFor(() => {
        expect(screen.getByText(/first name is required/i)).toBeInTheDocument();
        expect(screen.getByText(/last name is required/i)).toBeInTheDocument();
        expect(screen.getByText(/email is required/i)).toBeInTheDocument();
        expect(screen.getByText(/password is required/i)).toBeInTheDocument();
      });
    });

    it('should handle password mismatch error', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter initialEntries={['/register']}>
          <App />
        </MemoryRouter>
      );

      const passwordInput = screen.getByLabelText(/^password/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await user.type(passwordInput, 'SecurePass123!');
      await user.type(confirmPasswordInput, 'DifferentPass123!');

      const submitButton = screen.getByRole('button', { name: /create account/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
      });
    });
  });

  describe('User Login Flow', () => {
    it('should complete successful login flow', async () => {
      const user = userEvent.setup();

      const mockUser: User = {
        id: '1',
        email: 'john.doe@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: UserRole.USER,
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      };

      // Mock successful login
      mockAuthService.login.mockResolvedValueOnce({
        data: {
          user: mockUser,
          token: 'mock-jwt-token',
          refreshToken: 'mock-refresh-token',
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/login']}>
          <App />
        </MemoryRouter>
      );

      // Fill login form
      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);

      await user.type(emailInput, 'john.doe@example.com');
      await user.type(passwordInput, 'SecurePass123!');

      // Submit form
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);

      // Verify login was called
      await waitFor(() => {
        expect(mockAuthService.login).toHaveBeenCalledWith({
          email: 'john.doe@example.com',
          password: 'SecurePass123!',
        });
      });

      // Should redirect to dashboard or home page
      await waitFor(() => {
        expect(window.location.pathname).toBe('/');
      });

      // Should show user info in navigation
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    it('should handle login errors', async () => {
      const user = userEvent.setup();

      // Mock login failure
      mockAuthService.login.mockRejectedValueOnce(new Error('Invalid credentials'));

      render(
        <MemoryRouter initialEntries={['/login']}>
          <App />
        </MemoryRouter>
      );

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);

      await user.type(emailInput, 'john.doe@example.com');
      await user.type(passwordInput, 'wrongpassword');

      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
      });

      // Should remain on login page
      expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    });
  });

  describe('Password Reset Flow', () => {
    it('should complete forgot password flow', async () => {
      const user = userEvent.setup();

      // Mock successful forgot password request
      mockAuthService.forgotPassword.mockResolvedValueOnce({
        data: { message: 'Password reset email sent successfully.' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/forgot-password']}>
          <App />
        </MemoryRouter>
      );

      const emailInput = screen.getByLabelText(/email/i);
      await user.type(emailInput, 'john.doe@example.com');

      const submitButton = screen.getByRole('button', { name: /send reset email/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockAuthService.forgotPassword).toHaveBeenCalledWith('john.doe@example.com');
      });

      expect(screen.getByText(/password reset email sent/i)).toBeInTheDocument();
    });

    it('should complete password reset flow', async () => {
      const user = userEvent.setup();

      // Mock successful password reset
      mockAuthService.resetPassword.mockResolvedValueOnce({
        data: { message: 'Password reset successfully.' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/reset-password?token=reset-token']}>
          <App />
        </MemoryRouter>
      );

      const passwordInput = screen.getByLabelText(/new password/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await user.type(passwordInput, 'NewSecurePass123!');
      await user.type(confirmPasswordInput, 'NewSecurePass123!');

      const submitButton = screen.getByRole('button', { name: /reset password/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockAuthService.resetPassword).toHaveBeenCalledWith(
          'reset-token',
          'NewSecurePass123!'
        );
      });

      expect(screen.getByText(/password reset successfully/i)).toBeInTheDocument();
    });
  });

  describe('Protected Route Access', () => {
    it('should redirect unauthenticated users to login', async () => {
      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      // Should redirect to login page
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
      });
    });

    it('should allow authenticated users to access protected routes', async () => {
      const mockUser: User = {
        id: '1',
        email: 'john.doe@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: UserRole.USER,
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      };

      // Mock token verification
      mockAuthService.verifyToken.mockResolvedValueOnce({
        data: mockUser,
        success: true,
      });

      // Set up authenticated state
      localStorage.setItem('authToken', 'mock-token');
      localStorage.setItem('user', JSON.stringify(mockUser));

      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      // Should show courses page
      await waitFor(() => {
        expect(screen.getByText(/courses/i)).toBeInTheDocument();
      });
    });
  });

  describe('Logout Flow', () => {
    it('should complete logout flow', async () => {
      const user = userEvent.setup();

      const mockUser: User = {
        id: '1',
        email: 'john.doe@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: UserRole.USER,
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      };

      // Set up authenticated state
      localStorage.setItem('authToken', 'mock-token');
      localStorage.setItem('user', JSON.stringify(mockUser));

      mockAuthService.verifyToken.mockResolvedValueOnce({
        data: mockUser,
        success: true,
      });

      mockAuthService.logout.mockResolvedValueOnce(undefined);

      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      // Wait for authentication to be established
      await waitFor(() => {
        expect(screen.getByText('John Doe')).toBeInTheDocument();
      });

      // Click logout button
      const logoutButton = screen.getByRole('button', { name: /logout/i });
      await user.click(logoutButton);

      await waitFor(() => {
        expect(mockAuthService.logout).toHaveBeenCalled();
      });

      // Should clear authentication state
      expect(localStorage.getItem('authToken')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();

      // Should show login button
      expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    });
  });
});