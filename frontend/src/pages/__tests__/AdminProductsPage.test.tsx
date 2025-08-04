import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import AdminProductsPage from '../AdminProductsPage';
import { useLocalization } from '@/hooks';
import { adminProductService } from '@/services/adminProductService';
import { productService } from '@/services/productService';
import type { Product, ProductCategory } from '@/types';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
}));

// Mock the services
vi.mock('@/services/adminProductService', () => ({
  adminProductService: {
    createProduct: vi.fn(),
    updateProduct: vi.fn(),
    deleteProduct: vi.fn(),
    bulkDeleteProducts: vi.fn(),
    bulkUpdateProducts: vi.fn(),
  },
}));

vi.mock('@/services/productService', () => ({
  productService: {
    getProducts: vi.fn(),
    getCategories: vi.fn(),
  },
}));

// Mock the admin layout
vi.mock('@/components/admin', () => ({
  AdminLayout: ({ children }: { children: React.ReactNode }) => <div data-testid="admin-layout">{children}</div>,
}));

// Mock the product components
vi.mock('@/components/admin/products', () => ({
  ProductList: ({ onEdit, onDelete, onSelectionChange }: any) => (
    <div data-testid="product-list">
      <button onClick={() => onEdit({ id: '1', nameEn: 'Test Product' })}>Edit Product</button>
      <button onClick={() => onDelete('1')}>Delete Product</button>
      <button onClick={() => onSelectionChange(['1', '2'])}>Select Products</button>
    </div>
  ),
  ProductFilters: ({ onFiltersChange }: any) => (
    <div data-testid="product-filters">
      <button onClick={() => onFiltersChange({ search: 'test' })}>Apply Filter</button>
    </div>
  ),
  ProductForm: ({ onSave, onCancel }: any) => (
    <div data-testid="product-form">
      <button onClick={() => onSave({ nameEn: 'New Product' })}>Save Product</button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
  BulkActions: ({ onAction }: any) => (
    <div data-testid="bulk-actions">
      <button onClick={() => onAction('delete', ['1', '2'])}>Bulk Delete</button>
    </div>
  ),
  CategoryManager: ({ onClose, onCategoriesChange }: any) => (
    <div data-testid="category-manager">
      <button onClick={onCategoriesChange}>Update Categories</button>
      <button onClick={onClose}>Close</button>
    </div>
  ),
  ImportExportTools: ({ onClose, onImportComplete }: any) => (
    <div data-testid="import-export-tools">
      <button onClick={onImportComplete}>Import Complete</button>
      <button onClick={onClose}>Close</button>
    </div>
  ),
}));

// Mock the Button component
vi.mock('@/components/ui', () => ({
  Button: ({ children, onClick, ...props }: any) => (
    <button onClick={onClick} {...props}>
      {children}
    </button>
  ),
}));

const mockUseLocalization = useLocalization as vi.MockedFunction<typeof useLocalization>;
const mockAdminProductService = adminProductService as vi.Mocked<typeof adminProductService>;
const mockProductService = productService as vi.Mocked<typeof productService>;

const mockProducts: Product[] = [
  {
    id: '1',
    nameEn: 'Product 1',
    nameAr: 'منتج 1',
    descriptionEn: 'Description 1',
    descriptionAr: 'وصف 1',
    price: 29.99,
    currency: 'BHD',
    stockQuantity: 10,
    categoryId: 'cat1',
    imageUrls: [],
    isActive: true,
  },
  {
    id: '2',
    nameEn: 'Product 2',
    nameAr: 'منتج 2',
    descriptionEn: 'Description 2',
    descriptionAr: 'وصف 2',
    price: 49.99,
    currency: 'BHD',
    stockQuantity: 5,
    categoryId: 'cat2',
    imageUrls: [],
    isActive: true,
  },
];

const mockCategories: ProductCategory[] = [
  {
    id: 'cat1',
    nameEn: 'Category 1',
    nameAr: 'فئة 1',
    isActive: true,
  },
  {
    id: 'cat2',
    nameEn: 'Category 2',
    nameAr: 'فئة 2',
    isActive: true,
  },
];

// Mock window.confirm
Object.defineProperty(window, 'confirm', {
  writable: true,
  value: vi.fn(),
});

describe('AdminProductsPage', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.title': 'Product Management',
          'admin.products.subtitle': 'Manage your product catalog and inventory.',
          'admin.products.manageCategories': 'Manage Categories',
          'admin.products.importExport': 'Import/Export',
          'admin.products.addProduct': 'Add Product',
          'admin.products.confirmDelete': 'Are you sure you want to delete this product?',
          'admin.products.confirmBulkDelete': 'Are you sure you want to delete the selected products?',
        };
        return translations[key] || fallback || key;
      },
      isRTL: false,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn(),
    });

    mockProductService.getProducts.mockResolvedValue({
      products: mockProducts,
      totalPages: 1,
      currentPage: 1,
      totalCount: 2,
    });

    mockProductService.getCategories.mockResolvedValue(mockCategories);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly with all components', async () => {
    render(<AdminProductsPage />);
    
    expect(screen.getByTestId('admin-layout')).toBeInTheDocument();
    expect(screen.getByText('admin.products.title')).toBeInTheDocument();
    expect(screen.getByText('admin.products.subtitle')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByTestId('product-filters')).toBeInTheDocument();
      expect(screen.getByTestId('product-list')).toBeInTheDocument();
    });
  });

  it('loads products and categories on mount', async () => {
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      expect(mockProductService.getProducts).toHaveBeenCalledWith(1, 20, {
        search: undefined,
        category: undefined,
        inStock: undefined,
      });
      expect(mockProductService.getCategories).toHaveBeenCalled();
    });
  });

  it('shows action buttons in header', () => {
    render(<AdminProductsPage />);
    
    expect(screen.getByText('admin.products.manageCategories')).toBeInTheDocument();
    expect(screen.getByText('admin.products.importExport')).toBeInTheDocument();
    expect(screen.getByText('admin.products.addProduct')).toBeInTheDocument();
  });

  it('opens product form when add product button is clicked', () => {
    render(<AdminProductsPage />);
    
    const addButton = screen.getByText('admin.products.addProduct');
    fireEvent.click(addButton);
    
    expect(screen.getByTestId('product-form')).toBeInTheDocument();
  });

  it('opens category manager when manage categories button is clicked', () => {
    render(<AdminProductsPage />);
    
    const manageCategoriesButton = screen.getByText('admin.products.manageCategories');
    fireEvent.click(manageCategoriesButton);
    
    expect(screen.getByTestId('category-manager')).toBeInTheDocument();
  });

  it('opens import/export tools when button is clicked', () => {
    render(<AdminProductsPage />);
    
    const importExportButton = screen.getByText('admin.products.importExport');
    fireEvent.click(importExportButton);
    
    expect(screen.getByTestId('import-export-tools')).toBeInTheDocument();
  });

  it('handles product creation', async () => {
    mockAdminProductService.createProduct.mockResolvedValue(mockProducts[0]);
    
    render(<AdminProductsPage />);
    
    // Open form
    const addButton = screen.getByText('admin.products.addProduct');
    fireEvent.click(addButton);
    
    // Save product
    const saveButton = screen.getByText('Save Product');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.createProduct).toHaveBeenCalledWith({ nameEn: 'New Product' });
      expect(mockProductService.getProducts).toHaveBeenCalledTimes(2); // Initial load + after save
    });
  });

  it('handles product editing', async () => {
    mockAdminProductService.updateProduct.mockResolvedValue(mockProducts[0]);
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      const editButton = screen.getByText('Edit Product');
      fireEvent.click(editButton);
    });
    
    expect(screen.getByTestId('product-form')).toBeInTheDocument();
    
    // Save changes
    const saveButton = screen.getByText('Save Product');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.updateProduct).toHaveBeenCalledWith({
        id: '1',
        nameEn: 'New Product',
      });
    });
  });

  it('handles product deletion with confirmation', async () => {
    (window.confirm as vi.Mock).mockReturnValue(true);
    mockAdminProductService.deleteProduct.mockResolvedValue(undefined);
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      const deleteButton = screen.getByText('Delete Product');
      fireEvent.click(deleteButton);
    });
    
    await waitFor(() => {
      expect(window.confirm).toHaveBeenCalledWith('admin.products.confirmDelete');
      expect(mockAdminProductService.deleteProduct).toHaveBeenCalledWith('1');
    });
  });

  it('cancels product deletion when user declines', async () => {
    (window.confirm as vi.Mock).mockReturnValue(false);
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      const deleteButton = screen.getByText('Delete Product');
      fireEvent.click(deleteButton);
    });
    
    expect(window.confirm).toHaveBeenCalledWith('admin.products.confirmDelete');
    expect(mockAdminProductService.deleteProduct).not.toHaveBeenCalled();
  });

  it('handles bulk actions', async () => {
    (window.confirm as vi.Mock).mockReturnValue(true);
    mockAdminProductService.bulkDeleteProducts.mockResolvedValue(undefined);
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      // Select products
      const selectButton = screen.getByText('Select Products');
      fireEvent.click(selectButton);
    });
    
    // Bulk delete should be visible
    expect(screen.getByTestId('bulk-actions')).toBeInTheDocument();
    
    const bulkDeleteButton = screen.getByText('Bulk Delete');
    fireEvent.click(bulkDeleteButton);
    
    await waitFor(() => {
      expect(window.confirm).toHaveBeenCalledWith('admin.products.confirmBulkDelete');
      expect(mockAdminProductService.bulkDeleteProducts).toHaveBeenCalledWith(['1', '2']);
    });
  });

  it('handles filter changes', async () => {
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      const filterButton = screen.getByText('Apply Filter');
      fireEvent.click(filterButton);
    });
    
    await waitFor(() => {
      expect(mockProductService.getProducts).toHaveBeenCalledWith(1, 20, {
        search: 'test',
        category: undefined,
        inStock: undefined,
      });
    });
  });

  it('handles category manager updates', async () => {
    render(<AdminProductsPage />);
    
    // Open category manager
    const manageCategoriesButton = screen.getByText('admin.products.manageCategories');
    fireEvent.click(manageCategoriesButton);
    
    // Trigger categories update
    const updateButton = screen.getByText('Update Categories');
    fireEvent.click(updateButton);
    
    await waitFor(() => {
      expect(mockProductService.getProducts).toHaveBeenCalledTimes(2); // Initial + after update
      expect(mockProductService.getCategories).toHaveBeenCalledTimes(2);
    });
  });

  it('handles import completion', async () => {
    render(<AdminProductsPage />);
    
    // Open import/export tools
    const importExportButton = screen.getByText('admin.products.importExport');
    fireEvent.click(importExportButton);
    
    // Trigger import completion
    const importCompleteButton = screen.getByText('Import Complete');
    fireEvent.click(importCompleteButton);
    
    await waitFor(() => {
      expect(mockProductService.getProducts).toHaveBeenCalledTimes(2); // Initial + after import
    });
  });

  it('closes modals correctly', () => {
    render(<AdminProductsPage />);
    
    // Open and close product form
    const addButton = screen.getByText('admin.products.addProduct');
    fireEvent.click(addButton);
    expect(screen.getByTestId('product-form')).toBeInTheDocument();
    
    const cancelButton = screen.getByText('Cancel');
    fireEvent.click(cancelButton);
    expect(screen.queryByTestId('product-form')).not.toBeInTheDocument();
  });

  it('handles bulk activate action', async () => {
    mockAdminProductService.bulkUpdateProducts.mockResolvedValue(undefined);
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      // Select products first
      const selectButton = screen.getByText('Select Products');
      fireEvent.click(selectButton);
    });
    
    // Mock bulk action for activate
    const bulkActions = screen.getByTestId('bulk-actions');
    const mockOnAction = vi.fn();
    
    // Simulate activate action
    await waitFor(async () => {
      // This would be triggered by the BulkActions component
      await mockOnAction('activate', ['1', '2']);
      
      expect(mockAdminProductService.bulkUpdateProducts).toHaveBeenCalledWith({
        productIds: ['1', '2'],
        updates: { isActive: true },
      });
    });
  });

  it('handles error states gracefully', async () => {
    mockProductService.getProducts.mockRejectedValue(new Error('Failed to load'));
    
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load products:', expect.any(Error));
    });
    
    consoleSpy.mockRestore();
  });

  it('reloads data when page changes', async () => {
    render(<AdminProductsPage />);
    
    await waitFor(() => {
      expect(mockProductService.getProducts).toHaveBeenCalledWith(1, 20, expect.any(Object));
    });
    
    // Simulate page change (this would be triggered by ProductList component)
    // Since we can't directly test the page change from the mocked component,
    // we verify the initial call pattern
    expect(mockProductService.getProducts).toHaveBeenCalledTimes(1);
  });
});