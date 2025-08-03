import { apiClient } from './api';
import type { Product, ProductCategory, ApiResponse } from '@/types';

export interface CreateProductData {
  nameEn: string;
  nameAr: string;
  descriptionEn: string;
  descriptionAr: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
  imageUrls: string[];
  isActive: boolean;
}

export interface UpdateProductData extends Partial<CreateProductData> {
  id: string;
}

export interface BulkUpdateData {
  productIds: string[];
  updates: Partial<Omit<CreateProductData, 'nameEn' | 'nameAr' | 'descriptionEn' | 'descriptionAr'>>;
}

export interface InventoryUpdateData {
  productId: string;
  stockQuantity: number;
  reason?: string;
}

export interface ProductImportData {
  file: File;
  updateExisting: boolean;
}

export interface ProductExportOptions {
  format: 'csv' | 'xlsx';
  includeInactive?: boolean;
  categoryIds?: string[];
}

class AdminProductService {
  // Product CRUD operations
  async createProduct(data: CreateProductData): Promise<Product> {
    const response = await apiClient.post<ApiResponse<Product>>('/admin/products', data);
    return response.data.data;
  }

  async updateProduct(data: UpdateProductData): Promise<Product> {
    const { id, ...updateData } = data;
    const response = await apiClient.put<ApiResponse<Product>>(`/admin/products/${id}`, updateData);
    return response.data.data;
  }

  async deleteProduct(id: string): Promise<void> {
    await apiClient.delete(`/admin/products/${id}`);
  }

  async bulkUpdateProducts(data: BulkUpdateData): Promise<void> {
    await apiClient.patch('/admin/products/bulk', data);
  }

  async bulkDeleteProducts(productIds: string[]): Promise<void> {
    await apiClient.delete('/admin/products/bulk', { data: { productIds } });
  }

  // Inventory management
  async updateInventory(data: InventoryUpdateData): Promise<void> {
    await apiClient.patch(`/admin/products/${data.productId}/inventory`, {
      stockQuantity: data.stockQuantity,
      reason: data.reason,
    });
  }

  async bulkUpdateInventory(updates: InventoryUpdateData[]): Promise<void> {
    await apiClient.patch('/admin/products/inventory/bulk', { updates });
  }

  async getInventoryHistory(productId: string): Promise<InventoryHistoryEntry[]> {
    const response = await apiClient.get<ApiResponse<InventoryHistoryEntry[]>>(`/admin/products/${productId}/inventory/history`);
    return response.data.data;
  }

  // Category management
  async createCategory(data: Omit<ProductCategory, 'id'>): Promise<ProductCategory> {
    const response = await apiClient.post<ApiResponse<ProductCategory>>('/admin/categories', data);
    return response.data.data;
  }

  async updateCategory(id: string, data: Partial<Omit<ProductCategory, 'id'>>): Promise<ProductCategory> {
    const response = await apiClient.put<ApiResponse<ProductCategory>>(`/admin/categories/${id}`, data);
    return response.data.data;
  }

  async deleteCategory(id: string): Promise<void> {
    await apiClient.delete(`/admin/categories/${id}`);
  }

  // Import/Export operations
  async importProducts(data: ProductImportData): Promise<ImportResult> {
    const formData = new FormData();
    formData.append('file', data.file);
    formData.append('updateExisting', data.updateExisting.toString());

    const response = await apiClient.post<ApiResponse<ImportResult>>('/admin/products/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data.data;
  }

  async exportProducts(options: ProductExportOptions): Promise<Blob> {
    const params = new URLSearchParams({
      format: options.format,
    });

    if (options.includeInactive !== undefined) {
      params.append('includeInactive', options.includeInactive.toString());
    }

    if (options.categoryIds?.length) {
      options.categoryIds.forEach(id => params.append('categoryIds', id));
    }

    const response = await apiClient.get(`/admin/products/export?${params}`, {
      responseType: 'blob',
    });
    return response.data;
  }

  async getProductTemplate(): Promise<Blob> {
    const response = await apiClient.get('/admin/products/template', {
      responseType: 'blob',
    });
    return response.data;
  }
}

export interface InventoryHistoryEntry {
  id: string;
  productId: string;
  previousQuantity: number;
  newQuantity: number;
  changeAmount: number;
  reason: string;
  createdAt: Date;
  createdBy: string;
}

export interface ImportResult {
  totalProcessed: number;
  successCount: number;
  errorCount: number;
  errors: ImportError[];
  warnings: ImportWarning[];
}

export interface ImportError {
  row: number;
  field: string;
  message: string;
  value: string;
}

export interface ImportWarning {
  row: number;
  message: string;
}

export const adminProductService = new AdminProductService();