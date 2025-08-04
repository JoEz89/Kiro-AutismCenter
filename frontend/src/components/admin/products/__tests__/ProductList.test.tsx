import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import ProductList from '../ProductList';
import { useLocalization } from '@/hooks';
import type { Product, ProductCategory } from '@/types';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
}));

// Mock the Button component
vi.mock('@/components/ui', () => ({
  Button: ({ children, onClick, disabled, ...props }: any) => (
    <button onClick={onClick} disabled={disabled} {...props}>
      {children}
    </button>
  ),
}));

const mockUseLocalization = useLocalization as vi.MockedFunction<typeof useLocalization>;

const mockProducts: Product[] = [
  {
    id: '1',
    nameEn: 'Test Product 1',
    nameAr: 'منتج تجريبي 1',
    descriptionEn: 'Test description 1',
    descriptionAr: 'وصف تجريبي 1',
    price: 29.99,
    currency: 'BHD',
    stockQuantity: 10,
    categoryId: 'cat1',
    imageUrls: ['https://example.com/image1.jpg'],
    isActive: true,
  },
  {
    id: '2',
    nameEn: 'Test Product 2',
    nameAr: 'منتج تجريبي 2',
    descriptionEn: 'Test description 2',
    descriptionAr: 'وصف تجريبي 2',
    price: 49.99,
    currency: 'BHD',
    stockQuantity: 0,
    categoryId: 'cat2',
    imageUrls: [],
    isActive: false,
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

const defaultProps = {
  products: mockProducts,
  categories: mockCategories,
  selectedProducts: [],
  onSelectionChange: vi.fn(),
  onEdit: vi.fn(),
  onDelete: vi.fn(),
  isLoading: false,
  currentPage: 1,
  totalPages: 1,
  onPageChange: vi.fn(),
};

describe('ProductList', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.selectAll': 'Select All',
          'admin.products.noProducts': 'No products found',
          'admin.products.noProductsDescription': 'Get started by creating your first product.',
          'admin.products.active': 'Active',
          'admin.products.inactive': 'Inactive',
          'admin.products.category': 'Category',
          'admin.products.stock': 'Stock',
          'admin.products.inStock': 'In Stock',
          'admin.products.outOfStock': 'Out of Stock',
          'admin.products.page': 'Page',
          'common.edit': 'Edit',
          'common.delete': 'Delete',
          'common.of': 'of',
          'common.previous': 'Previous',
          'common.next': 'Next',
        };
        return translations[key] || fallback || key;
      },
      isRTL: false,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn(),
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders loading state correctly', () => {
    render(<ProductList {...defaultProps} isLoading={true} />);
    
    const loadingContainer = document.querySelector('.animate-pulse');
    expect(loadingContainer).toBeInTheDocument();
  });

  it('renders empty state when no products', () => {
    render(<ProductList {...defaultProps} products={[]} />);
    
    expect(screen.getByText('No products found')).toBeInTheDocument();
    expect(screen.getByText('Get started by creating your first product.')).toBeInTheDocument();
  });

  it('renders products correctly', () => {
    render(<ProductList {...defaultProps} />);
    
    expect(screen.getByText('Test Product 1')).toBeInTheDocument();
    expect(screen.getByText('Test Product 2')).toBeInTheDocument();
    expect(screen.getByText('29.99 BHD')).toBeInTheDocument();
    expect(screen.getByText('49.99 BHD')).toBeInTheDocument();
  });

  it('shows correct stock status', () => {
    render(<ProductList {...defaultProps} />);
    
    expect(screen.getByText('In Stock')).toBeInTheDocument();
    expect(screen.getByText('Out of Stock')).toBeInTheDocument();
  });

  it('shows correct active status', () => {
    render(<ProductList {...defaultProps} />);
    
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
  });

  it('handles product selection', () => {
    const onSelectionChange = vi.fn();
    render(<ProductList {...defaultProps} onSelectionChange={onSelectionChange} />);
    
    const checkboxes = screen.getAllByRole('checkbox');
    fireEvent.click(checkboxes[1]); // First product checkbox (index 0 is select all)
    
    expect(onSelectionChange).toHaveBeenCalledWith(['1']);
  });

  it('handles select all', () => {
    const onSelectionChange = vi.fn();
    render(<ProductList {...defaultProps} onSelectionChange={onSelectionChange} />);
    
    const selectAllCheckbox = screen.getAllByRole('checkbox')[0];
    fireEvent.click(selectAllCheckbox);
    
    expect(onSelectionChange).toHaveBeenCalledWith(['1', '2']);
  });

  it('handles edit button click', () => {
    const onEdit = vi.fn();
    render(<ProductList {...defaultProps} onEdit={onEdit} />);
    
    const editButtons = screen.getAllByText('Edit');
    fireEvent.click(editButtons[0]);
    
    expect(onEdit).toHaveBeenCalledWith(mockProducts[0]);
  });

  it('handles delete button click', () => {
    const onDelete = vi.fn();
    render(<ProductList {...defaultProps} onDelete={onDelete} />);
    
    const deleteButtons = screen.getAllByText('Delete');
    fireEvent.click(deleteButtons[0]);
    
    expect(onDelete).toHaveBeenCalledWith('1');
  });

  it('renders pagination when multiple pages', () => {
    render(<ProductList {...defaultProps} totalPages={3} currentPage={2} />);
    
    expect(screen.getByText(/Page/)).toBeInTheDocument();
    expect(screen.getByText('Previous')).toBeInTheDocument();
    expect(screen.getByText('Next')).toBeInTheDocument();
  });

  it('handles page navigation', () => {
    const onPageChange = vi.fn();
    render(<ProductList {...defaultProps} totalPages={3} currentPage={2} onPageChange={onPageChange} />);
    
    const nextButton = screen.getByText('Next');
    fireEvent.click(nextButton);
    
    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it('renders Arabic content when RTL', () => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.selectAll': 'Select All',
          'admin.products.noProducts': 'No products found',
          'admin.products.noProductsDescription': 'Get started by creating your first product.',
          'admin.products.active': 'Active',
          'admin.products.inactive': 'Inactive',
          'admin.products.category': 'Category',
          'admin.products.stock': 'Stock',
          'admin.products.inStock': 'In Stock',
          'admin.products.outOfStock': 'Out of Stock',
          'admin.products.page': 'Page',
          'common.edit': 'Edit',
          'common.delete': 'Delete',
          'common.of': 'of',
          'common.previous': 'Previous',
          'common.next': 'Next',
        };
        return translations[key] || fallback || key;
      },
      isRTL: true,
      language: 'ar',
      direction: 'rtl',
      setLanguage: vi.fn(),
    });

    render(<ProductList {...defaultProps} />);
    
    expect(screen.getByText('منتج تجريبي 1')).toBeInTheDocument();
    expect(screen.getByText('منتج تجريبي 2')).toBeInTheDocument();
  });

  it('displays product images correctly', () => {
    render(<ProductList {...defaultProps} />);
    
    const productImage = screen.getByAltText('Test Product 1');
    expect(productImage).toHaveAttribute('src', 'https://example.com/image1.jpg');
  });

  it('shows placeholder for products without images', () => {
    render(<ProductList {...defaultProps} />);
    
    // Product 2 has no images, should show placeholder
    const placeholders = screen.getAllByRole('generic');
    const imagePlaceholder = placeholders.find(el => 
      el.className.includes('bg-gray-200') && el.className.includes('h-16')
    );
    expect(imagePlaceholder).toBeInTheDocument();
  });
});