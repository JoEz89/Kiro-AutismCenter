import { useContext } from 'react';
import { AuthContext } from '@/context/AuthContext';
import { UserRole } from '@/types';

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Custom hook for role-based access control
export const useRole = () => {
  const { user } = useAuth();
  
  const hasRole = (role: UserRole): boolean => {
    return user?.role === role;
  };

  const hasAnyRole = (roles: UserRole[]): boolean => {
    return user ? roles.includes(user.role) : false;
  };

  const isAdmin = (): boolean => {
    return hasRole(UserRole.ADMIN);
  };

  const isDoctor = (): boolean => {
    return hasRole(UserRole.DOCTOR);
  };

  const isUser = (): boolean => {
    return hasRole(UserRole.USER);
  };

  return {
    hasRole,
    hasAnyRole,
    isAdmin,
    isDoctor,
    isUser,
    currentRole: user?.role,
  };
};

// Custom hook for authentication status checks
export const useAuthStatus = () => {
  const { isAuthenticated, isLoading, user } = useAuth();

  const isEmailVerified = (): boolean => {
    return user?.isEmailVerified ?? false;
  };

  const needsEmailVerification = (): boolean => {
    return isAuthenticated && !isEmailVerified();
  };

  return {
    isAuthenticated,
    isLoading,
    isEmailVerified: isEmailVerified(),
    needsEmailVerification: needsEmailVerification(),
  };
};