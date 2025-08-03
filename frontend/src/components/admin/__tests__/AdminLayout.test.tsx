import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { AdminLayout } from '../AdminLayout';
import { AuthProvider, LanguageProvider, ThemeProvider } from '@/context';

// Mock the hooks
jest.mock('@/hooks', () => ({
  useAuth: () => ({
    user: {
      id: '1',
      email: 'admin@example.com',
      firstName: 'Admin',
      lastName: 'User',
      role: 'admin',
      preferredLanguage: 'en',
      isEmailVerified: true,
      createdAt: new Date(),
    },
  }),
  useLocalization: () => ({
    t: (key: string, fallback?: string) => fallback || key,
    isRTL: false,
  }),
}));

const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <BrowserRouter>
    <ThemeProvider>
      <LanguageProvider>
        <AuthProvider>
          {children}
        </AuthProvider>
      </LanguageProvider>
    </ThemeProvider>
  </BrowserRouter>
);

describe('AdminLayout', () => {
  it('renders admin layout for admin user', () => {
    render(
      <TestWrapper>
        <AdminLayout>
          <div>Test Content</div>
        </AdminLayout>
      </TestWrapper>
    );

    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('shows access denied for non-admin user', () => {
    // Mock non-admin user
    jest.mocked(require('@/hooks').useAuth).mockReturnValue({
      user: {
        id: '1',
        email: 'user@example.com',
        firstName: 'Regular',
        lastName: 'User',
        role: 'user',
        preferredLanguage: 'en',
        isEmailVerified: true,
        createdAt: new Date(),
      },
    });

    render(
      <TestWrapper>
        <AdminLayout>
          <div>Test Content</div>
        </AdminLayout>
      </TestWrapper>
    );

    expect(screen.getByText('Access Denied')).toBeInTheDocument();
    expect(screen.getByText('Administrator access is required to view this page.')).toBeInTheDocument();
    expect(screen.queryByText('Test Content')).not.toBeInTheDocument();
  });

  it('shows access denied for unauthenticated user', () => {
    // Mock unauthenticated user
    jest.mocked(require('@/hooks').useAuth).mockReturnValue({
      user: null,
    });

    render(
      <TestWrapper>
        <AdminLayout>
          <div>Test Content</div>
        </AdminLayout>
      </TestWrapper>
    );

    expect(screen.getByText('Access Denied')).toBeInTheDocument();
    expect(screen.queryByText('Test Content')).not.toBeInTheDocument();
  });
});