import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { ProductCard } from '../ProductCard';
import { LanguageProvider } from '@/context';
import type { Product } from '@/types';

const mockProduct: Product = {
  id: '1',
  nameEn: 'Test Product',
  nameAr: 'منتج تجريبي',
  descriptionEn: 'Test product description',
  descriptionAr: 'وصف المنتج التجريبي',
  price: 29.99,
  currency: 'USD',
  stockQuantity: 10,
  categoryId: 'cat1',
  imageUrls: ['https://example.com/image1.jpg'],
  isActive: true,
};

const mockOutOfStockProduct: Product = {
  ...mockProduct,
  id: '2',
  stockQuantity: 0,
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageProvider>
        {component}
      </LanguageProvider>
    </BrowserRouter>
  );
};

describe('ProductCard', () => {
  it('renders product information correctly', () => {
    renderWithProviders(<ProductCard product={mockProduct} />);
    
    expect(screen.getByText('Test Product')).toBeInTheDocument();
    expect(screen.getByText('Test product description')).toBeInTheDocument();
    expect(screen.getByText('$29.99')).toBeInTheDocument();
  });

  it('displays out of stock badge when product is out of stock', () => {
    renderWithProviders(<ProductCard product={mockOutOfStockProduct} />);
    
    expect(screen.getByText('Out of Stock')).toBeInTheDocument();
  });

  it('calls onAddToCart when add to cart button is clicked', async () => {
    const mockAddToCart = vi.fn();
    renderWithProviders(
      <ProductCard product={mockProduct} onAddToCart={mockAddToCart} />
    );
    
    const addToCartButton = screen.getByText('Add to Cart');
    fireEvent.click(addToCartButton);
    
    await waitFor(() => {
      expect(mockAddToCart).toHaveBeenCalledWith('1');
    });
  });

  it('disables add to cart button when product is out of stock', () => {
    const mockAddToCart = vi.fn();
    renderWithProviders(
      <ProductCard product={mockOutOfStockProduct} onAddToCart={mockAddToCart} />
    );
    
    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
    expect(addToCartButton).toBeDisabled();
  });

  it('shows low stock warning when stock is low', () => {
    const lowStockProduct = { ...mockProduct, stockQuantity: 3 };
    renderWithProviders(<ProductCard product={lowStockProduct} />);
    
    expect(screen.getByText('Only 3 left')).toBeInTheDocument();
  });

  it('renders fallback image when image fails to load', () => {
    renderWithProviders(<ProductCard product={mockProduct} />);
    
    const image = screen.getByAltText('Test Product');
    fireEvent.error(image);
    
    // Should show fallback SVG icon
    expect(screen.getByRole('img', { hidden: true })).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    const mockAddToCart = vi.fn();
    renderWithProviders(
      <ProductCard product={mockProduct} onAddToCart={mockAddToCart} />
    );
    
    const addToCartButton = screen.getByRole('button', { 
      name: 'Add to Cart Test Product' 
    });
    expect(addToCartButton).toBeInTheDocument();
  });
});