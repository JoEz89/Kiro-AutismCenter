import { apiClient } from './api';
import type { Order, Address, ApiResponse } from '@/types';

export interface CheckoutRequest {
  shippingAddress: Address;
  billingAddress: Address;
  paymentMethodId: string;
  notes?: string;
}

export interface PaymentIntent {
  id: string;
  clientSecret: string;
  amount: number;
  currency: string;
  status: string;
}

export interface ShippingOption {
  id: string;
  name: string;
  description: string;
  price: number;
  estimatedDays: number;
}

class CheckoutService {
  async createPaymentIntent(amount: number, currency: string = 'USD'): Promise<PaymentIntent> {
    const response = await apiClient.post<ApiResponse<PaymentIntent>>('/checkout/payment-intent', {
      amount,
      currency,
    });
    return response.data.data;
  }

  async getShippingOptions(address: Address): Promise<ShippingOption[]> {
    const response = await apiClient.post<ApiResponse<ShippingOption[]>>('/checkout/shipping-options', address);
    return response.data.data;
  }

  async calculateTax(address: Address, amount: number): Promise<{ taxAmount: number; taxRate: number }> {
    const response = await apiClient.post<ApiResponse<{ taxAmount: number; taxRate: number }>>('/checkout/calculate-tax', {
      address,
      amount,
    });
    return response.data.data;
  }

  async createOrder(checkoutData: CheckoutRequest): Promise<Order> {
    const response = await apiClient.post<ApiResponse<Order>>('/orders', checkoutData);
    return response.data.data;
  }

  async confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<{ success: boolean; orderId?: string; error?: string }> {
    const response = await apiClient.post<ApiResponse<{ success: boolean; orderId?: string; error?: string }>>('/checkout/confirm-payment', {
      paymentIntentId,
      paymentMethodId,
    });
    return response.data.data;
  }

  async getOrder(orderId: string): Promise<Order> {
    const response = await apiClient.get<ApiResponse<Order>>(`/orders/${orderId}`);
    return response.data.data;
  }

  // Address validation
  async validateAddress(address: Address): Promise<{ isValid: boolean; suggestions?: Address[]; errors?: string[] }> {
    const response = await apiClient.post<ApiResponse<{ isValid: boolean; suggestions?: Address[]; errors?: string[] }>>('/checkout/validate-address', address);
    return response.data.data;
  }
}

export const checkoutService = new CheckoutService();