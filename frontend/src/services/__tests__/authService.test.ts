import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../authService';
import { apiClient } from '../api';
import { LoginCredentials, RegisterData } from '@/types';

// Mock the API client
vi.mock('../api', () => ({
  apiClient: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

const mockApiClient = vi.mocked(apiClient);

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Clear localStorage
    localStorage.clear();
  });

  describe('login', () => {
    it('should login successfully with valid credentials', async () => {
      const credentials: LoginCredentials = {
        email: 'test@example.com',
        password: 'password123',
      };

      const mockResponse = {
        data: {
          data: {
            user: {
              id: '1',
              email: 'test@example.com',
              firstName: 'John',
              lastName: 'Doe',
              role: 'user',
              preferredLanguage: 'en',
              isEmailVerified: true,
              createdAt: new Date(),
            },
            token: 'mock-jwt-token',
            refreshToken: 'mock-refresh-token',
          },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.login(credentials);

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/login', credentials);
      expect(result).toEqual(mockResponse.data);
    });

    it('should throw error on invalid credentials', async () => {
      const credentials: LoginCredentials = {
        email: 'test@example.com',
        password: 'wrongpassword',
      };

      const mockError = new Error('Invalid credentials');
      mockApiClient.post.mockRejectedValueOnce(mockError);

      await expect(authService.login(credentials)).rejects.toThrow('Invalid credentials');
    });
  });

  describe('register', () => {
    it('should register successfully with valid data', async () => {
      const registerData: RegisterData = {
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'Jane',
        lastName: 'Doe',
        preferredLanguage: 'en',
      };

      const mockResponse = {
        data: {
          data: { message: 'Registration successful. Please check your email for verification.' },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.register(registerData);

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/register', registerData);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('verifyToken', () => {
    it('should verify token successfully', async () => {
      const mockResponse = {
        data: {
          data: {
            id: '1',
            email: 'test@example.com',
            firstName: 'John',
            lastName: 'Doe',
            role: 'user',
            preferredLanguage: 'en',
            isEmailVerified: true,
            createdAt: new Date(),
          },
          success: true,
        },
      };

      mockApiClient.get.mockResolvedValueOnce(mockResponse);

      const result = await authService.verifyToken();

      expect(mockApiClient.get).toHaveBeenCalledWith('/auth/verify');
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('refreshToken', () => {
    it('should refresh token successfully', async () => {
      const mockRefreshToken = 'mock-refresh-token';
      localStorage.setItem('refreshToken', mockRefreshToken);

      const mockResponse = {
        data: {
          data: { token: 'new-jwt-token' },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.refreshToken();

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/refresh', {
        refreshToken: mockRefreshToken,
      });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('forgotPassword', () => {
    it('should send forgot password email successfully', async () => {
      const email = 'test@example.com';
      const mockResponse = {
        data: {
          data: { message: 'Password reset email sent successfully.' },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.forgotPassword(email);

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/forgot-password', { email });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('resetPassword', () => {
    it('should reset password successfully', async () => {
      const token = 'reset-token';
      const newPassword = 'newpassword123';
      const mockResponse = {
        data: {
          data: { message: 'Password reset successfully.' },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.resetPassword(token, newPassword);

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/reset-password', {
        token,
        newPassword,
      });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('changePassword', () => {
    it('should change password successfully', async () => {
      const currentPassword = 'oldpassword';
      const newPassword = 'newpassword123';
      const mockResponse = {
        data: {
          data: { message: 'Password changed successfully.' },
          success: true,
        },
      };

      mockApiClient.post.mockResolvedValueOnce(mockResponse);

      const result = await authService.changePassword(currentPassword, newPassword);

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/change-password', {
        currentPassword,
        newPassword,
      });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('logout', () => {
    it('should logout successfully', async () => {
      mockApiClient.post.mockResolvedValueOnce({});

      await authService.logout();

      expect(mockApiClient.post).toHaveBeenCalledWith('/auth/logout');
    });

    it('should not throw error if logout request fails', async () => {
      const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      mockApiClient.post.mockRejectedValueOnce(new Error('Network error'));

      await expect(authService.logout()).resolves.not.toThrow();
      expect(consoleSpy).toHaveBeenCalledWith('Logout request failed:', expect.any(Error));

      consoleSpy.mockRestore();
    });
  });
});