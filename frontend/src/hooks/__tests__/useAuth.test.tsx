import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useAuth, useRole, useAuthStatus } from '../useAuth';
import { AuthProvider, AuthContext } from '@/context/AuthContext';
import { User, UserRole } from '@/types';
import React from 'react';

// Mock the auth service
vi.mock('@/services/authService', () => ({
  authService: {
    verifyToken: vi.fn(),
  },
}));

const createWrapper = (contextValue: any) => {
  return ({ children }: { children: React.ReactNode }) => (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

describe('useAuth', () => {
  it('should throw error when used outside AuthProvider', () => {
    expect(() => {
      renderHook(() => useAuth());
    }).toThrow('useAuth must be used within an AuthProvider');
  });

  it('should return auth context when used within provider', () => {
    const mockContextValue = {
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current).toEqual(mockContextValue);
  });
});

describe('useRole', () => {
  const mockUser: User = {
    id: '1',
    email: 'test@example.com',
    firstName: 'John',
    lastName: 'Doe',
    role: UserRole.ADMIN,
    preferredLanguage: 'en',
    isEmailVerified: true,
    createdAt: new Date(),
  };

  const mockContextValue = {
    user: mockUser,
    token: 'mock-token',
    isAuthenticated: true,
    isLoading: false,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    loginWithGoogle: vi.fn(),
    refreshToken: vi.fn(),
  };

  it('should return correct role information for admin user', () => {
    const { result } = renderHook(() => useRole(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current.hasRole(UserRole.ADMIN)).toBe(true);
    expect(result.current.hasRole(UserRole.USER)).toBe(false);
    expect(result.current.hasAnyRole([UserRole.ADMIN, UserRole.DOCTOR])).toBe(true);
    expect(result.current.isAdmin()).toBe(true);
    expect(result.current.isDoctor()).toBe(false);
    expect(result.current.isUser()).toBe(false);
    expect(result.current.currentRole).toBe(UserRole.ADMIN);
  });

  it('should return correct role information for doctor user', () => {
    const doctorUser = { ...mockUser, role: UserRole.DOCTOR };
    const doctorContextValue = { ...mockContextValue, user: doctorUser };

    const { result } = renderHook(() => useRole(), {
      wrapper: createWrapper(doctorContextValue),
    });

    expect(result.current.hasRole(UserRole.DOCTOR)).toBe(true);
    expect(result.current.hasRole(UserRole.ADMIN)).toBe(false);
    expect(result.current.isDoctor()).toBe(true);
    expect(result.current.isAdmin()).toBe(false);
    expect(result.current.isUser()).toBe(false);
    expect(result.current.currentRole).toBe(UserRole.DOCTOR);
  });

  it('should return correct role information for regular user', () => {
    const regularUser = { ...mockUser, role: UserRole.USER };
    const userContextValue = { ...mockContextValue, user: regularUser };

    const { result } = renderHook(() => useRole(), {
      wrapper: createWrapper(userContextValue),
    });

    expect(result.current.hasRole(UserRole.USER)).toBe(true);
    expect(result.current.hasRole(UserRole.ADMIN)).toBe(false);
    expect(result.current.isUser()).toBe(true);
    expect(result.current.isAdmin()).toBe(false);
    expect(result.current.isDoctor()).toBe(false);
    expect(result.current.currentRole).toBe(UserRole.USER);
  });

  it('should handle null user', () => {
    const nullUserContextValue = { ...mockContextValue, user: null };

    const { result } = renderHook(() => useRole(), {
      wrapper: createWrapper(nullUserContextValue),
    });

    expect(result.current.hasRole(UserRole.USER)).toBe(false);
    expect(result.current.hasAnyRole([UserRole.ADMIN, UserRole.DOCTOR])).toBe(false);
    expect(result.current.isAdmin()).toBe(false);
    expect(result.current.isDoctor()).toBe(false);
    expect(result.current.isUser()).toBe(false);
    expect(result.current.currentRole).toBeUndefined();
  });
});

describe('useAuthStatus', () => {
  it('should return correct auth status for authenticated verified user', () => {
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

    const mockContextValue = {
      user: mockUser,
      token: 'mock-token',
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    const { result } = renderHook(() => useAuthStatus(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.isEmailVerified).toBe(true);
    expect(result.current.needsEmailVerification).toBe(false);
  });

  it('should return correct auth status for authenticated unverified user', () => {
    const mockUser: User = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      role: UserRole.USER,
      preferredLanguage: 'en',
      isEmailVerified: false,
      createdAt: new Date(),
    };

    const mockContextValue = {
      user: mockUser,
      token: 'mock-token',
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    const { result } = renderHook(() => useAuthStatus(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.isEmailVerified).toBe(false);
    expect(result.current.needsEmailVerification).toBe(true);
  });

  it('should return correct auth status for unauthenticated user', () => {
    const mockContextValue = {
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      loginWithGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    const { result } = renderHook(() => useAuthStatus(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.isEmailVerified).toBe(false);
    expect(result.current.needsEmailVerification).toBe(false);
  });
});