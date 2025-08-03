import { apiClient } from './api';
import { User, UserRole, ApiResponse } from '@/types';

export interface UserFilters {
  role?: UserRole | 'all';
  status?: 'all' | 'verified' | 'unverified';
  search?: string;
  page?: number;
  limit?: number;
}

export interface UserStats {
  totalUsers: number;
  adminUsers: number;
  doctorUsers: number;
  regularUsers: number;
  verifiedUsers: number;
  unverifiedUsers: number;
}

export interface UserListResponse {
  users: User[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
}

class AdminUserService {
  async getUsers(filters: UserFilters = {}): Promise<UserListResponse> {
    const params = new URLSearchParams();
    
    if (filters.role && filters.role !== 'all') {
      params.append('role', filters.role);
    }
    if (filters.status && filters.status !== 'all') {
      params.append('status', filters.status);
    }
    if (filters.search) {
      params.append('search', filters.search);
    }
    if (filters.page) {
      params.append('page', filters.page.toString());
    }
    if (filters.limit) {
      params.append('limit', filters.limit.toString());
    }

    const response = await apiClient.get<ApiResponse<UserListResponse>>(
      `/admin/users?${params.toString()}`
    );
    return response.data.data;
  }

  async getUserStats(): Promise<UserStats> {
    const response = await apiClient.get<ApiResponse<UserStats>>('/admin/users/stats');
    return response.data.data;
  }

  async getUserById(userId: string): Promise<User> {
    const response = await apiClient.get<ApiResponse<User>>(`/admin/users/${userId}`);
    return response.data.data;
  }

  async updateUserRole(userId: string, role: UserRole): Promise<User> {
    const response = await apiClient.put<ApiResponse<User>>(
      `/admin/users/${userId}/role`,
      { role }
    );
    return response.data.data;
  }

  async verifyUserEmail(userId: string): Promise<User> {
    const response = await apiClient.post<ApiResponse<User>>(
      `/admin/users/${userId}/verify-email`
    );
    return response.data.data;
  }

  async deactivateUser(userId: string): Promise<User> {
    const response = await apiClient.post<ApiResponse<User>>(
      `/admin/users/${userId}/deactivate`
    );
    return response.data.data;
  }

  async activateUser(userId: string): Promise<User> {
    const response = await apiClient.post<ApiResponse<User>>(
      `/admin/users/${userId}/activate`
    );
    return response.data.data;
  }

  async exportUsers(filters: UserFilters = {}, format: 'csv' | 'pdf' = 'csv'): Promise<Blob> {
    const params = new URLSearchParams();
    
    if (filters.role && filters.role !== 'all') {
      params.append('role', filters.role);
    }
    if (filters.status && filters.status !== 'all') {
      params.append('status', filters.status);
    }
    if (filters.search) {
      params.append('search', filters.search);
    }
    
    params.append('format', format);

    const response = await apiClient.get(
      `/admin/users/export?${params.toString()}`,
      {
        responseType: 'blob',
      }
    );
    
    return response.data;
  }
}

export const adminUserService = new AdminUserService();
export default adminUserService;