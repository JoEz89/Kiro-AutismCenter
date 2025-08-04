import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import ProductFilters from '../ProductFilters';
import { useLocalization } from '@/hooks';
import type { ProductCategory } from '@/types';

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

const defaultFilters = {
  search: '',
  category: '',
  inStock: undefined as boolean | undefined,
  isActive: undefined as boolean | undefined,
};

const defaultProps = {
  filters: defaultFilters,
  categories: mockCategories,
  onFiltersChange: vi.fn(),
};

describe('ProductFilters', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.search': 'Search Products',
          'admin.products.searchPlaceholder': 'Search by name or description...',
          'admin.products.category': 'Category',
          'admin.products.allCategories': 'All Categories',
          'admin.products.stockStatus': 'Stock Status',
          'admin.products.allStock': 'All Stock Levels',
          'admin.products.inStock': 'In Stock',
          'admin.products.outOfStock': 'Out of Stock',
          'admin.products.status': 'Status',
          'admin.products.allStatuses': 'All Statuses',
          'admin.products.active': 'Active',
          'admin.products.inactive': 'Inactive',
          'admin.products.clearFilters': 'Clear Filters',
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

  it('renders all filter inputs correctly', () => {
    render(<ProductFilters {...defaultProps} />);
    
    expect(screen.getByPlaceholderText('admin.products.searchPlaceholder')).toBeInTheDocument();
    expect(screen.getByDisplayValue('admin.products.allCategories')).toBeInTheDocument();
    expect(screen.getByDisplayValue('admin.products.allStock')).toBeInTheDocument();
    expect(screen.getByDisplayValue('admin.products.allStatuses')).toBeInTheDocument();
  });

  it('displays categories in category filter', () => {
    render(<ProductFilters {...defaultProps} />);
    
    const categorySelect = screen.getByDisplayValue('admin.products.allCategories');
    expect(categorySelect).toBeInTheDocument();
    
    // Check that categories are rendered as options
    expect(screen.getByText('Category 1')).toBeInTheDocument();
    expect(screen.getByText('Category 2')).toBeInTheDocument();
  });

  it('handles search input change', () => {
    const onFiltersChange = vi.fn();
    render(<ProductFilters {...defaultProps} onFiltersChange={onFiltersChange} />);
    
    const searchInput = screen.getByPlaceholderText('admin.products.searchPlaceholder');
    fireEvent.change(searchInput, { target: { value: 'test search' } });
    
    expect(onFiltersChange).toHaveBeenCalledWith({
      ...defaultFilters,
      search: 'test search',
    });
  });

  it('handles category filter change', () => {
    const onFiltersChange = vi.fn();
    render(<ProductFilters {...defaultProps} onFiltersChange={onFiltersChange} />);
    
    const categorySelect = screen.getByDisplayValue('admin.products.allCategories');
    fireEvent.change(categorySelect, { target: { value: 'cat1' } });
    
    expect(onFiltersChange).toHaveBeenCalledWith({
      ...defaultFilters,
      category: 'cat1',
    });
  });

  it('handles stock status filter change', () => {
    const onFiltersChange = vi.fn();
    render(<ProductFilters {...defaultProps} onFiltersChange={onFiltersChange} />);
    
    const stockSelect = screen.getByDisplayValue('admin.products.allStock');
    fireEvent.change(stockSelect, { target: { value: 'true' } });
    
    expect(onFiltersChange).toHaveBeenCalledWith({
      ...defaultFilters,
      inStock: true,
    });
  });

  it('handles active status filter change', () => {
    const onFiltersChange = vi.fn();
    render(<ProductFilters {...defaultProps} onFiltersChange={onFiltersChange} />);
    
    const activeSelect = screen.getByDisplayValue('admin.products.allStatuses');
    fireEvent.change(activeSelect, { target: { value: 'false' } });
    
    expect(onFiltersChange).toHaveBeenCalledWith({
      ...defaultFilters,
      isActive: false,
    });
  });

  it('shows clear filters button when filters are active', () => {
    const filtersWithValues = {
      search: 'test',
      category: 'cat1',
      inStock: true,
      isActive: true,
    };
    
    render(<ProductFilters {...defaultProps} filters={filtersWithValues} />);
    
    expect(screen.getByText('admin.products.clearFilters')).toBeInTheDocument();
  });

  it('hides clear filters button when no filters are active', () => {
    render(<ProductFilters {...defaultProps} />);
    
    expect(screen.queryByText('admin.products.clearFilters')).not.toBeInTheDocument();
  });

  it('handles clear filters action', () => {
    const onFiltersChange = vi.fn();
    const filtersWithValues = {
      search: 'test',
      category: 'cat1',
      inStock: true,
      isActive: true,
    };
    
    render(<ProductFilters {...defaultProps} filters={filtersWithValues} onFiltersChange={onFiltersChange} />);
    
    const clearButton = screen.getByText('admin.products.clearFilters');
    fireEvent.click(clearButton);
    
    expect(onFiltersChange).toHaveBeenCalledWith({
      search: '',
      category: '',
      inStock: undefined,
      isActive: undefined,
    });
  });

  it('displays current filter values correctly', () => {
    const filtersWithValues = {
      search: 'test product',
      category: 'cat1',
      inStock: false,
      isActive: true,
    };
    
    render(<ProductFilters {...defaultProps} filters={filtersWithValues} />);
    
    expect(screen.getByDisplayValue('test product')).toBeInTheDocument();
    expect(screen.getByDisplayValue('cat1')).toBeInTheDocument();
    expect(screen.getByDisplayValue('false')).toBeInTheDocument();
    expect(screen.getByDisplayValue('true')).toBeInTheDocument();
  });

  it('renders Arabic category names when RTL', () => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.search': 'Search Products',
          'admin.products.searchPlaceholder': 'Search by name or description...',
          'admin.products.category': 'Category',
          'admin.products.allCategories': 'All Categories',
          'admin.products.stockStatus': 'Stock Status',
          'admin.products.allStock': 'All Stock Levels',
          'admin.products.inStock': 'In Stock',
          'admin.products.outOfStock': 'Out of Stock',
          'admin.products.status': 'Status',
          'admin.products.allStatuses': 'All Statuses',
          'admin.products.active': 'Active',
          'admin.products.inactive': 'Inactive',
          'admin.products.clearFilters': 'Clear Filters',
        };
        return translations[key] || fallback || key;
      },
      isRTL: true,
      language: 'ar',
      direction: 'rtl',
      setLanguage: vi.fn(),
    });

    render(<ProductFilters {...defaultProps} />);
    
    expect(screen.getByText('فئة 1')).toBeInTheDocument();
    expect(screen.getByText('فئة 2')).toBeInTheDocument();
  });

  it('handles undefined boolean values correctly', () => {
    const onFiltersChange = vi.fn();
    render(<ProductFilters {...defaultProps} onFiltersChange={onFiltersChange} />);
    
    // Change to a boolean value then back to undefined
    const stockSelect = screen.getByDisplayValue('admin.products.allStock');
    fireEvent.change(stockSelect, { target: { value: 'true' } });
    fireEvent.change(stockSelect, { target: { value: '' } });
    
    expect(onFiltersChange).toHaveBeenLastCalledWith({
      ...defaultFilters,
      inStock: undefined,
    });
  });

  it('has proper accessibility attributes', () => {
    render(<ProductFilters {...defaultProps} />);
    
    const searchInput = screen.getByPlaceholderText('admin.products.searchPlaceholder');
    const categorySelect = screen.getByDisplayValue('admin.products.allCategories');
    const stockSelect = screen.getByDisplayValue('admin.products.allStock');
    const activeSelect = screen.getByDisplayValue('admin.products.allStatuses');
    
    expect(searchInput).toHaveAttribute('id', 'search');
    expect(categorySelect).toHaveAttribute('id', 'category');
    expect(stockSelect).toHaveAttribute('id', 'stock');
    expect(activeSelect).toHaveAttribute('id', 'active');
  });

  it('shows correct filter labels', () => {
    render(<ProductFilters {...defaultProps} />);
    
    expect(screen.getByText('admin.products.search')).toBeInTheDocument();
    expect(screen.getByText('admin.products.category')).toBeInTheDocument();
    expect(screen.getByText('admin.products.stockStatus')).toBeInTheDocument();
    expect(screen.getByText('admin.products.status')).toBeInTheDocument();
  });

  it('renders with proper styling classes', () => {
    render(<ProductFilters {...defaultProps} />);
    
    const container = screen.getByText('admin.products.search').closest('div');
    expect(container?.parentElement).toHaveClass('bg-white', 'dark:bg-gray-800');
  });
});