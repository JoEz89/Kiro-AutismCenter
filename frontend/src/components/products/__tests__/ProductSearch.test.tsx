import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { ProductSearch } from '../ProductSearch';
import { LanguageProvider } from '@/context';
import { productService } from '@/services/productService';
import type { Product } from '@/types';

// Mock the product service
vi.mock('@/services/productService');
const mockProductService = productService as any;

const mockSearchResults: Product[] = [
  {
    id: '1',
    nameEn: 'Test Product',
    nameAr: 'منتج تجريبي',
    descriptionEn: 'Test description',
    descriptionAr: 'وصف تجريبي',
    price: 29.99,
    currency: 'USD',
    stockQuantity: 10,
    categoryId: 'cat1',
    imageUrls: ['https://example.com/image1.jpg'],
    isActive: true,
  },
];

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageProvider>
        {component}
      </LanguageProvider>
    </BrowserRouter>
  );
};

describe('ProductSearch', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders search input correctly', () => {
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    expect(searchInput).toBeInTheDocument();
  });

  it('shows search results when typing', async () => {
    mockProductService.searchProducts.mockResolvedValue(mockSearchResults);
    
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    fireEvent.change(searchInput, { target: { value: 'test' } });
    
    await waitFor(() => {
      expect(screen.getByText('Test Product')).toBeInTheDocument();
    });
  });

  it('calls onSearchSubmit when form is submitted', async () => {
    const mockOnSearchSubmit = vi.fn();
    renderWithProviders(<ProductSearch onSearchSubmit={mockOnSearchSubmit} />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    fireEvent.change(searchInput, { target: { value: 'test query' } });
    fireEvent.submit(searchInput.closest('form')!);
    
    expect(mockOnSearchSubmit).toHaveBeenCalledWith('test query');
  });

  it('shows loading indicator when searching', async () => {
    mockProductService.searchProducts.mockImplementation(
      () => new Promise(resolve => setTimeout(() => resolve(mockSearchResults), 100))
    );
    
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    fireEvent.change(searchInput, { target: { value: 'test' } });
    
    // Should show loading spinner
    expect(screen.getByRole('generic', { hidden: true })).toBeInTheDocument();
  });

  it('shows no results message when search returns empty', async () => {
    mockProductService.searchProducts.mockResolvedValue([]);
    
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    fireEvent.change(searchInput, { target: { value: 'nonexistent' } });
    
    await waitFor(() => {
      expect(screen.getByText('No products found for "nonexistent"')).toBeInTheDocument();
    });
  });

  it('closes dropdown when clicking outside', async () => {
    mockProductService.searchProducts.mockResolvedValue(mockSearchResults);
    
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByPlaceholderText('Search products...');
    fireEvent.change(searchInput, { target: { value: 'test' } });
    
    await waitFor(() => {
      expect(screen.getByText('Test Product')).toBeInTheDocument();
    });
    
    // Click outside
    fireEvent.mouseDown(document.body);
    
    await waitFor(() => {
      expect(screen.queryByText('Test Product')).not.toBeInTheDocument();
    });
  });

  it('has proper accessibility attributes', () => {
    renderWithProviders(<ProductSearch />);
    
    const searchInput = screen.getByLabelText('Search products');
    expect(searchInput).toBeInTheDocument();
  });
});