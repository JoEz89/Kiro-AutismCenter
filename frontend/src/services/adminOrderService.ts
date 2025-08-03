import { apiClient } from './api';
import { Order, OrderStatus, ApiResponse } from '@/types';

export interface OrderFilters {
  status?: OrderStatus | 'all';
  paymentStatus?: string | 'all';
  dateRange?: 'all' | 'today' | 'week' | 'month';
  search?: string;
  page?: number;
  limit?: number;
}

export interface OrderStats {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  totalRevenue: number;
}

export interface OrderListResponse {
  orders: Order[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
}

class AdminOrderService {
  async getOrders(filters: OrderFilters = {}): Promise<OrderListResponse> {
    const params = new URLSearchParams();
    
    if (filters.status && filters.status !== 'all') {
      params.append('status', filters.status);
    }
    if (filters.paymentStatus && filters.paymentStatus !== 'all') {
      params.append('paymentStatus', filters.paymentStatus);
    }
    if (filters.dateRange && filters.dateRange !== 'all') {
      params.append('dateRange', filters.dateRange);
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

    const response = await apiClient.get<ApiResponse<OrderListResponse>>(
      `/admin/orders?${params.toString()}`
    );
    return response.data.data;
  }

  async getOrderStats(): Promise<OrderStats> {
    const response = await apiClient.get<ApiResponse<OrderStats>>('/admin/orders/stats');
    return response.data.data;
  }

  async getOrderById(orderId: string): Promise<Order> {
    const response = await apiClient.get<ApiResponse<Order>>(`/admin/orders/${orderId}`);
    return response.data.data;
  }

  async updateOrderStatus(orderId: string, status: OrderStatus): Promise<Order> {
    const response = await apiClient.put<ApiResponse<Order>>(
      `/admin/orders/${orderId}/status`,
      { status }
    );
    return response.data.data;
  }

  async processRefund(orderId: string, amount?: number): Promise<Order> {
    const response = await apiClient.post<ApiResponse<Order>>(
      `/admin/orders/${orderId}/refund`,
      { amount }
    );
    return response.data.data;
  }

  async exportOrders(filters: OrderFilters = {}, format: 'csv' | 'pdf' = 'csv'): Promise<Blob> {
    const params = new URLSearchParams();
    
    if (filters.status && filters.status !== 'all') {
      params.append('status', filters.status);
    }
    if (filters.paymentStatus && filters.paymentStatus !== 'all') {
      params.append('paymentStatus', filters.paymentStatus);
    }
    if (filters.dateRange && filters.dateRange !== 'all') {
      params.append('dateRange', filters.dateRange);
    }
    if (filters.search) {
      params.append('search', filters.search);
    }
    
    params.append('format', format);

    const response = await apiClient.get(
      `/admin/orders/export?${params.toString()}`,
      {
        responseType: 'blob',
      }
    );
    
    return response.data;
  }
}

export const adminOrderService = new AdminOrderService();
export default adminOrderService;