import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { PublicRoute } from '../PublicRoute';
import { useAuth } from '@/hooks/useAuth';
import { UserRole } from '@/types';

// Mock the useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

const mockUseAuth = vi.mocked(useAuth);

const TestComponent = () => <div data-testid="public-content">Public Content</div>;

const renderWithRouter = (component: React.ReactElement, initialEntries = ['/']) => {
  return render(
    <MemoryRouter initialEntries={initialEntries}>
      {component}
    </MemoryRouter>
  );
};

describe('PublicRoute', () => {
  it('should show loading spinner when loading', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      user: null,
      isLoading: true,
      token: null,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    });

    renderWithRouter(
      <PublicRoute>
        <TestComponent />
      </PublicRoute>
    );

    expect(screen.getByRole('status', { hidden: true })).toBeInTheDocument();
    expect(screen.queryByTestId('public-content')).not.toBeInTheDocument();
  });

  it('should render children when not authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      token: null,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    });

    renderWithRouter(
      <PublicRoute>
        <TestComponent />
      </PublicRoute>
    );

    expect(screen.getByTestId('public-content')).toBeInTheDocument();
  });

  it('should redirect to default path when authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      user: {
        id: '1',
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: UserRole.USER,
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      },
      isLoading: false,
      token: 'mock-token',
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    });

    renderWithRouter(
      <PublicRoute>
        <TestComponent />
      </PublicRoute>
    );

    expect(screen.queryByTestId('public-content')).not.toBeInTheDocument();
  });

  it('should redirect to custom path when authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      user: {
        id: '1',
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: UserRole.USER,
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      },
      isLoading: false,
      token: 'mock-token',
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    });

    renderWithRouter(
      <PublicRoute redirectTo="/dashboard">
        <TestComponent />
      </PublicRoute>
    );

    expect(screen.queryByTestId('public-content')).not.toBeInTheDocument();
  });
});