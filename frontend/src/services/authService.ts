import { apiClient } from './api';
import type { LoginCredentials, RegisterData, User, ApiResponse } from '@/types';

interface AuthResponse {
  user: User;
  token: string;
  refreshToken?: string;
}

interface GoogleAuthResponse {
  user: User;
  token: string;
}

export const authService = {
  // Login with email and password
  async login(credentials: LoginCredentials): Promise<ApiResponse<AuthResponse>> {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/login', credentials);
    return response.data;
  },

  // Register new user
  async register(data: RegisterData): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.post<ApiResponse<{ message: string }>>('/auth/register', data);
    return response.data;
  },

  // Google OAuth login
  async loginWithGoogle(): Promise<ApiResponse<GoogleAuthResponse>> {
    // This would typically redirect to Google OAuth or handle OAuth flow
    // For now, we'll implement a placeholder that would work with the backend
    const response = await apiClient.post<ApiResponse<GoogleAuthResponse>>('/auth/google');
    return response.data;
  },

  // Verify current token
  async verifyToken(): Promise<ApiResponse<User>> {
    const response = await apiClient.get<ApiResponse<User>>('/auth/verify');
    return response.data;
  },

  // Refresh access token
  async refreshToken(): Promise<ApiResponse<{ token: string }>> {
    const refreshToken = localStorage.getItem('refreshToken');
    const response = await apiClient.post<ApiResponse<{ token: string }>>('/auth/refresh', {
      refreshToken,
    });
    return response.data;
  },

  // Verify email
  async verifyEmail(token: string): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.post<ApiResponse<{ message: string }>>('/auth/verify-email', {
      token,
    });
    return response.data;
  },

  // Request password reset
  async forgotPassword(email: string): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.post<ApiResponse<{ message: string }>>('/auth/forgot-password', {
      email,
    });
    return response.data;
  },

  // Reset password
  async resetPassword(token: string, newPassword: string): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.post<ApiResponse<{ message: string }>>('/auth/reset-password', {
      token,
      newPassword,
    });
    return response.data;
  },

  // Change password (for authenticated users)
  async changePassword(currentPassword: string, newPassword: string): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.post<ApiResponse<{ message: string }>>('/auth/change-password', {
      currentPassword,
      newPassword,
    });
    return response.data;
  },

  // Logout (optional - mainly for server-side session invalidation)
  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      // Ignore errors on logout - we'll clear local storage anyway
      console.warn('Logout request failed:', error);
    }
  },
};