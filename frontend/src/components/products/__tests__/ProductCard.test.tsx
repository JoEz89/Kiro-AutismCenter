import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ProductCard } from '../ProductCard';
import { CartProvider } from '@/context/CartContext';
import { LanguageProvider } from '@/context/LanguageContext';
import { AuthProvider } from '@/context/AuthContext';
import { testAccessibility } from '@/test/accessibility';
import { Product } from '@/types';

const mockProduct: Product = {
  id: '1',
  nameEn: 'Autism Learning Kit',
  nameAr: 'مجموعة تعلم التوحد',
  descriptionEn: 'A comprehensive learning kit for children with autism',
  descriptionAr: 'مجموعة تعلم شاملة للأطفال المصابين بالتوحد',
  price: 99.99,
  currency: 'BHD',
  stockQuantity: 10,
  categoryId: 'cat-1',
  imageUrls: ['https://example.com/image1.jpg', 'https://example.com/image2.jpg'],
  isActive: true,
};

const TestWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>
    <LanguageProvider>
      <AuthProvider>
        <CartProvider>
          {children}
        </CartProvider>
      </AuthProvider>
    </LanguageProvider>
  </BrowserRouter>
);

describe('ProductCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render product information correctly', () => {
    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
    expect(screen.getByText('A comprehensive learning kit for children with autism')).toBeInTheDocument();
    expect(screen.getByText('99.99 BHD')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: /autism learning kit/i })).toBeInTheDocument();
  });

  it('should render Arabic content when Arabic language is selected', () => {
    // Mock Arabic language context
    vi.doMock('@/hooks/useLocalization', () => ({
      useLocalization: () => ({
        language: 'ar',
        t: (key: string) => key,
        isRTL: true,
      }),
    }));

    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    expect(screen.getByText('مجموعة تعلم التوحد')).toBeInTheDocument();
    expect(screen.getByText('مجموعة تعلم شاملة للأطفال المصابين بالتوحد')).toBeInTheDocument();
  });

  it('should handle add to cart action', async () => {
    const user = userEvent.setup();
    const mockAddToCart = vi.fn();

    // Mock cart context
    vi.doMock('@/context/CartContext', () => ({
      useCart: () => ({
        addToCart: mockAddToCart,
        isLoading: false,
      }),
    }));

    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
    await user.click(addToCartButton);

    expect(mockAddToCart).toHaveBeenCalledWith(mockProduct.id, 1);
  });

  it('should show out of stock state when product is out of stock', () => {
    const outOfStockProduct = { ...mockProduct, stockQuantity: 0 };

    render(
      <TestWrapper>
        <ProductCard product={outOfStockProduct} />
      </TestWrapper>
    );

    expect(screen.getByText(/out of stock/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /out of stock/i })).toBeDisabled();
  });

  it('should show loading state when adding to cart', () => {
    // Mock loading state
    vi.doMock('@/context/CartContext', () => ({
      useCart: () => ({
        addToCart: vi.fn(),
        isLoading: true,
      }),
    }));

    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    expect(screen.getByRole('button', { name: /adding/i })).toBeDisabled();
  });

  it('should navigate to product detail page when clicked', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    const productLink = screen.getByRole('link');
    expect(productLink).toHaveAttribute('href', `/products/${mockProduct.id}`);
  });

  it('should display product image with proper alt text', () => {
    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    const image = screen.getByRole('img');
    expect(image).toHaveAttribute('src', mockProduct.imageUrls[0]);
    expect(image).toHaveAttribute('alt', mockProduct.nameEn);
  });

  it('should handle image loading error gracefully', () => {
    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    const image = screen.getByRole('img');
    fireEvent.error(image);

    // Should show placeholder or fallback image
    expect(image).toHaveAttribute('src', expect.stringContaining('placeholder'));
  });

  it('should be accessible', async () => {
    const renderResult = render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    await testAccessibility(renderResult);
  });

  it('should support keyboard navigation', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ProductCard product={mockProduct} />
      </TestWrapper>
    );

    const productLink = screen.getByRole('link');
    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });

    // Test tab navigation
    await user.tab();
    expect(productLink).toHaveFocus();

    await user.tab();
    expect(addToCartButton).toHaveFocus();

    // Test enter key on button
    await user.keyboard('{Enter}');
    // Should trigger add to cart action
  });

  it('should display price in correct currency format', () => {
    const usdProduct = { ...mockProduct, currency: 'USD' as const, price: 25.50 };

    render(
      <TestWrapper>
        <ProductCard product={usdProduct} />
      </TestWrapper>
    );

    expect(screen.getByText('25.50 USD')).toBeInTheDocument();
  });

  it('should truncate long product names appropriately', () => {
    const longNameProduct = {
      ...mockProduct,
      nameEn: 'This is a very long product name that should be truncated to prevent layout issues',
    };

    render(
      <TestWrapper>
        <ProductCard product={longNameProduct} />
      </TestWrapper>
    );

    const productName = screen.getByText(/this is a very long product name/i);
    expect(productName).toHaveClass('truncate');
  });

  it('should show discount badge when product is on sale', () => {
    const saleProduct = {
      ...mockProduct,
      originalPrice: 149.99,
      isOnSale: true,
    };

    render(
      <TestWrapper>
        <ProductCard product={saleProduct} />
      </TestWrapper>
    );

    expect(screen.getByText(/sale/i)).toBeInTheDocument();
    expect(screen.getByText('149.99')).toHaveClass('line-through');
  });
});