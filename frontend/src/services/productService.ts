import { apiClient } from './api';
import type { Product, ApiResponse } from '@/types';

export interface ProductFilters {
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  search?: string;
}

export interface ProductSortOptions {
  field: 'name' | 'price' | 'createdAt';
  direction: 'asc' | 'desc';
}

export interface ProductsResponse {
  products: Product[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface ProductCategory {
  id: string;
  nameEn: string;
  nameAr: string;
  isActive: boolean;
}

class ProductService {
  async getProducts(
    page: number = 1,
    pageSize: number = 12,
    filters?: ProductFilters,
    sort?: ProductSortOptions
  ): Promise<ProductsResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (filters) {
      if (filters.category) params.append('category', filters.category);
      if (filters.minPrice !== undefined) params.append('minPrice', filters.minPrice.toString());
      if (filters.maxPrice !== undefined) params.append('maxPrice', filters.maxPrice.toString());
      if (filters.inStock !== undefined) params.append('inStock', filters.inStock.toString());
      if (filters.search) params.append('search', filters.search);
    }

    if (sort) {
      params.append('sortBy', sort.field);
      params.append('sortDirection', sort.direction);
    }

    const response = await apiClient.get<ApiResponse<ProductsResponse>>(`/products?${params}`);
    return response.data.data;
  }

  async getProduct(id: string): Promise<Product> {
    const response = await apiClient.get<ApiResponse<Product>>(`/products/${id}`);
    return response.data.data;
  }

  async getCategories(): Promise<ProductCategory[]> {
    const response = await apiClient.get<ApiResponse<ProductCategory[]>>('/products/categories');
    return response.data.data;
  }

  async searchProducts(query: string, limit: number = 10): Promise<Product[]> {
    const response = await apiClient.get<ApiResponse<Product[]>>(`/products/search?q=${encodeURIComponent(query)}&limit=${limit}`);
    return response.data.data;
  }
}

export const productService = new ProductService();