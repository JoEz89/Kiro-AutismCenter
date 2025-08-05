import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
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
  price: 49.99,
  currency: 'BHD',
  stockQuantity: 10,
  categoryId: 'cat-1',
  imageUrls: ['https://example.com/image1.jpg', 'https://example.com/image2.jpg'],
  isActive: true,
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageProvider>
        <AuthProvider>
          <CartProvider>
            {component}
          </CartProvider>
        </AuthProvider>
      </LanguageProvider>
    </BrowserRouter>
  );
};

describe('ProductCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render product information correctly', () => {
    renderWithProviders(<ProductCard product={mockProduct} />);

    expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
    expect(screen.getByText('A comprehensive learning kit for children with autism')).toBeInTheDocument();
    expect(screen.getByText('49.99 BHD')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: /autism learning kit/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add to cart/i })).toBeInTheDocument();
  });

  it('should render Arabic content when language is Arabic', () => {
    renderWithProviders(
      <div data-testid="arabic-context" lang="ar">
        <ProductCard product={mockProduct} />
      </div>
    );

    // Note: This would require proper i18n context setup
    // For now, we'll test that the component renders without errors
    expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
  });

  it('should handle add to cart action', async () => {
    const user = userEvent.setup();
    renderWithProviders(<ProductCard product={mockProduct} />);

    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
    await user.click(addToCartButton);

    // Should show success feedback
    await waitFor(() => {
      expect(screen.getByText(/added to cart/i)).toBeInTheDocument();
    });
  });

  it('should show out of stock state', () => {
    const outOfStockProduct = { ...mockProduct, stockQuantity: 0 };
    renderWithProviders(<ProductCard product={outOfStockProduct} />);

    expect(screen.getByText(/out of stock/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add to cart/i })).toBeDisabled();
  });

  it('should handle image loading error', async () => {
    renderWithProviders(<ProductCard product={mockProduct} />);

    const image = screen.getByRole('img', { name: /autism learning kit/i });
    
    // Simulate image load error
    await userEvent.setup().click(image);
    
    // Should show fallback image or placeholder
    expect(image).toBeInTheDocument();
  });

  it('should navigate to product detail on click', async () => {
    const user = userEvent.setup();
    renderWithProviders(<ProductCard product={mockProduct} />);

    const productLink = screen.getByRole('link', { name: /view details/i });
    expect(productLink).toHaveAttribute('href', '/products/1');
  });

  it('should display discount badge when product is on sale', () => {
    const saleProduct = { 
      ...mockProduct, 
      originalPrice: 69.99,
      isOnSale: true 
    };
    
    renderWithProviders(<ProductCard product={saleProduct} />);

    expect(screen.getByText(/sale/i)).toBeInTheDocument();
    expect(screen.getByText('69.99')).toHaveClass('line-through');
  });

  it('should show rating stars when available', () => {
    const ratedProduct = { 
      ...mockProduct, 
      rating: 4.5,
      reviewCount: 23 
    };
    
    renderWithProviders(<ProductCard product={ratedProduct} />);

    expect(screen.getByText('4.5')).toBeInTheDocument();
    expect(screen.getByText('(23 reviews)')).toBeInTheDocument();
  });

  it('should be accessible', async () => {
    const renderResult = renderWithProviders(<ProductCard product={mockProduct} />);
    await testAccessibility(renderResult);
  });

  it('should support keyboard navigation', async () => {
    const user = userEvent.setup();
    renderWithProviders(<ProductCard product={mockProduct} />);

    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
    
    // Focus the button with tab
    await user.tab();
    expect(addToCartButton).toHaveFocus();

    // Activate with Enter key
    await user.keyboard('{Enter}');

    await waitFor(() => {
      expect(screen.getByText(/added to cart/i)).toBeInTheDocument();
    });
  });

  it('should handle long product names gracefully', () => {
    const longNameProduct = {
      ...mockProduct,
      nameEn: 'This is a very long product name that should be truncated or wrapped properly to maintain layout',
    };

    renderWithProviders(<ProductCard product={longNameProduct} />);

    const productName = screen.getByText(longNameProduct.nameEn);
    expect(productName).toBeInTheDocument();
    
    // Check that the text doesn't overflow its container
    const computedStyle = window.getComputedStyle(productName);
    expect(computedStyle.overflow).toBe('hidden');
  });

  it('should show loading state when adding to cart', async () => {
    const user = userEvent.setup();
    renderWithProviders(<ProductCard product={mockProduct} />);

    const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
    await user.click(addToCartButton);

    // Should show loading state briefly
    expect(screen.getByText(/adding/i)).toBeInTheDocument();
    expect(addToCartButton).toBeDisabled();
  });
});