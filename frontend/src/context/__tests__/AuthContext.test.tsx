import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { act } from 'react';
import { AuthProvider } from '../AuthContext';
import { useAuth } from '@/hooks/useAuth';
import { authService } from '@/services/authService';
import { LoginCredentials, RegisterData, User, UserRole } from '@/types';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    verifyToken: vi.fn(),
    refreshToken: vi.fn(),
    logout: vi.fn(),
  },
}));

const mockAuthService = vi.mocked(authService);

// Test component that uses the auth context
const TestComponent = () => {
  const auth = useAuth();

  return (
    <div>
      <div data-testid="loading">{auth.isLoading.toString()}</div>
      <div data-testid="authenticated">{auth.isAuthenticated.toString()}</div>
      <div data-testid="user">{auth.user ? auth.user.email : 'null'}</div>
      <button onClick={() => auth.login({ email: 'test@example.com', password: 'password' })}>
        Login
      </button>
      <button onClick={() => auth.logout()}>Logout</button>
    </div>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('should initialize with loading state', async () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    // Initially should be loading
    expect(screen.getByTestId('loading')).toHaveTextContent('true');
    expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
    expect(screen.getByTestId('user')).toHaveTextContent('null');

    // Wait for loading to complete
    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false');
    });
  });

  it('should restore user from localStorage on initialization', async () => {
    const mockUser: User = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      role: UserRole.USER,
      preferredLanguage: 'en',
      isEmailVerified: true,
      createdAt: new Date(),
    };

    localStorage.setItem('authToken', 'mock-token');
    localStorage.setItem('user', JSON.stringify(mockUser));
    mockAuthService.verifyToken.mockResolvedValueOnce({
      data: mockUser,
      success: true,
    });

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false');
      expect(screen.getByTestId('authenticated')).toHaveTextContent('true');
      expect(screen.getByTestId('user')).toHaveTextContent('test@example.com');
    });

    expect(mockAuthService.verifyToken).toHaveBeenCalled();
  });

  it('should handle login successfully', async () => {
    const mockUser: User = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      role: UserRole.USER,
      preferredLanguage: 'en',
      isEmailVerified: true,
      createdAt: new Date(),
    };

    const mockResponse = {
      data: {
        user: mockUser,
        token: 'mock-token',
        refreshToken: 'mock-refresh-token',
      },
      success: true,
    };

    mockAuthService.login.mockResolvedValueOnce(mockResponse);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    // Wait for initial loading to complete
    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false');
    });

    await act(async () => {
      screen.getByText('Login').click();
    });

    await waitFor(() => {
      expect(screen.getByTestId('authenticated')).toHaveTextContent('true');
      expect(screen.getByTestId('user')).toHaveTextContent('test@example.com');
    });

    expect(localStorage.getItem('authToken')).toBe('mock-token');
    expect(localStorage.getItem('refreshToken')).toBe('mock-refresh-token');
    expect(localStorage.getItem('user')).toBe(JSON.stringify(mockUser));
  });

  it('should handle logout successfully', async () => {
    const mockUser: User = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      role: UserRole.USER,
      preferredLanguage: 'en',
      isEmailVerified: true,
      createdAt: new Date(),
    };

    // Set up initial authenticated state
    localStorage.setItem('authToken', 'mock-token');
    localStorage.setItem('user', JSON.stringify(mockUser));
    mockAuthService.verifyToken.mockResolvedValueOnce({
      data: mockUser,
      success: true,
    });

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    // Wait for authentication to be established
    await waitFor(() => {
      expect(screen.getByTestId('authenticated')).toHaveTextContent('true');
    });

    mockAuthService.logout.mockResolvedValueOnce(undefined);

    await act(async () => {
      screen.getByText('Logout').click();
    });

    await waitFor(() => {
      expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
      expect(screen.getByTestId('user')).toHaveTextContent('null');
    });

    expect(localStorage.getItem('authToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });

  it('should handle token verification failure', async () => {
    localStorage.setItem('authToken', 'invalid-token');
    localStorage.setItem('user', JSON.stringify({ email: 'test@example.com' }));

    mockAuthService.verifyToken.mockRejectedValueOnce(new Error('Token invalid'));
    mockAuthService.logout.mockResolvedValueOnce(undefined);

    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => { });

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false');
      expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
    });

    expect(consoleSpy).toHaveBeenCalledWith('Token verification failed:', expect.any(Error));
    expect(localStorage.getItem('authToken')).toBeNull();

    consoleSpy.mockRestore();
  });

  it('should throw error when useAuth is used outside provider', () => {
    const TestComponentOutsideProvider = () => {
      useAuth();
      return <div>Test</div>;
    };

    expect(() => {
      render(<TestComponentOutsideProvider />);
    }).toThrow('useAuth must be used within an AuthProvider');
  });
});