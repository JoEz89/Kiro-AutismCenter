import { apiClient } from './api';
import type { CartItem, Product, ApiResponse } from '@/types';

export interface CartResponse {
  items: (CartItem & { product: Product })[];
  totalAmount: number;
  totalItems: number;
  currency: string;
}

export interface AddToCartRequest {
  productId: string;
  quantity: number;
}

export interface UpdateCartItemRequest {
  productId: string;
  quantity: number;
}

class CartService {
  async getCart(): Promise<CartResponse> {
    const response = await apiClient.get<ApiResponse<CartResponse>>('/cart');
    return response.data.data;
  }

  async addToCart(request: AddToCartRequest): Promise<CartResponse> {
    const response = await apiClient.post<ApiResponse<CartResponse>>('/cart/add', request);
    return response.data.data;
  }

  async updateCartItem(request: UpdateCartItemRequest): Promise<CartResponse> {
    const response = await apiClient.put<ApiResponse<CartResponse>>(`/cart/items/${request.productId}`, {
      quantity: request.quantity,
    });
    return response.data.data;
  }

  async removeFromCart(productId: string): Promise<CartResponse> {
    const response = await apiClient.delete<ApiResponse<CartResponse>>(`/cart/items/${productId}`);
    return response.data.data;
  }

  async clearCart(): Promise<void> {
    await apiClient.delete('/cart');
  }

  // Local storage methods for cart persistence
  private getLocalCartKey(): string {
    const user = localStorage.getItem('user');
    const userId = user ? JSON.parse(user).id : 'anonymous';
    return `cart_${userId}`;
  }

  saveCartToLocal(cart: CartResponse): void {
    try {
      localStorage.setItem(this.getLocalCartKey(), JSON.stringify(cart));
    } catch (error) {
      console.warn('Failed to save cart to localStorage:', error);
    }
  }

  getCartFromLocal(): CartResponse | null {
    try {
      const cartData = localStorage.getItem(this.getLocalCartKey());
      return cartData ? JSON.parse(cartData) : null;
    } catch (error) {
      console.warn('Failed to load cart from localStorage:', error);
      return null;
    }
  }

  clearLocalCart(): void {
    try {
      localStorage.removeItem(this.getLocalCartKey());
    } catch (error) {
      console.warn('Failed to clear cart from localStorage:', error);
    }
  }

  // Sync local cart with server when user logs in
  async syncCart(): Promise<CartResponse> {
    const localCart = this.getCartFromLocal();
    if (!localCart || localCart.items.length === 0) {
      return this.getCart();
    }

    // Add local cart items to server cart
    for (const item of localCart.items) {
      try {
        await this.addToCart({
          productId: item.productId,
          quantity: item.quantity,
        });
      } catch (error) {
        console.warn('Failed to sync cart item:', error);
      }
    }

    // Clear local cart after sync
    this.clearLocalCart();
    
    // Return updated server cart
    return this.getCart();
  }
}

export const cartService = new CartService();