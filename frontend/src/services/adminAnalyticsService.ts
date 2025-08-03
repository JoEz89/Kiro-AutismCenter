import { apiClient } from './api';
import { ApiResponse } from '@/types';

export interface AnalyticsData {
  revenue: {
    total: number;
    growth: number;
    monthlyData: Array<{ month: string; amount: number }>;
  };
  orders: {
    total: number;
    growth: number;
    statusBreakdown: Array<{ status: string; count: number; percentage: number }>;
  };
  users: {
    total: number;
    growth: number;
    registrationData: Array<{ month: string; count: number }>;
  };
  courses: {
    totalEnrollments: number;
    completionRate: number;
    popularCourses: Array<{ name: string; enrollments: number }>;
  };
  appointments: {
    total: number;
    completionRate: number;
    monthlyData: Array<{ month: string; count: number }>;
  };
}

export interface AnalyticsFilters {
  period: 'week' | 'month' | 'quarter' | 'year';
  startDate?: string;
  endDate?: string;
}

class AdminAnalyticsService {
  async getAnalyticsData(filters: AnalyticsFilters): Promise<AnalyticsData> {
    const params = new URLSearchParams();
    params.append('period', filters.period);
    
    if (filters.startDate) {
      params.append('startDate', filters.startDate);
    }
    if (filters.endDate) {
      params.append('endDate', filters.endDate);
    }

    const response = await apiClient.get<ApiResponse<AnalyticsData>>(
      `/admin/analytics?${params.toString()}`
    );
    return response.data.data;
  }

  async exportAnalyticsData(
    filters: AnalyticsFilters,
    format: 'csv' | 'pdf' = 'csv'
  ): Promise<Blob> {
    const params = new URLSearchParams();
    params.append('period', filters.period);
    params.append('format', format);
    
    if (filters.startDate) {
      params.append('startDate', filters.startDate);
    }
    if (filters.endDate) {
      params.append('endDate', filters.endDate);
    }

    const response = await apiClient.get(
      `/admin/analytics/export?${params.toString()}`,
      {
        responseType: 'blob',
      }
    );
    
    return response.data;
  }

  async getRevenueData(period: string): Promise<Array<{ month: string; amount: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ month: string; amount: number }>>>(
      `/admin/analytics/revenue?period=${period}`
    );
    return response.data.data;
  }

  async getOrderStatusBreakdown(): Promise<Array<{ status: string; count: number; percentage: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ status: string; count: number; percentage: number }>>>(
      '/admin/analytics/orders/status-breakdown'
    );
    return response.data.data;
  }

  async getUserRegistrationTrend(period: string): Promise<Array<{ month: string; count: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ month: string; count: number }>>>(
      `/admin/analytics/users/registration-trend?period=${period}`
    );
    return response.data.data;
  }

  async getPopularCourses(limit: number = 5): Promise<Array<{ name: string; enrollments: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ name: string; enrollments: number }>>>(
      `/admin/analytics/courses/popular?limit=${limit}`
    );
    return response.data.data;
  }

  async getAppointmentData(period: string): Promise<Array<{ month: string; count: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ month: string; count: number }>>>(
      `/admin/analytics/appointments?period=${period}`
    );
    return response.data.data;
  }
}

export const adminAnalyticsService = new AdminAnalyticsService();
export default adminAnalyticsService;