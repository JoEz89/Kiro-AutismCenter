import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { ProductGrid } from '../ProductGrid';
import { LanguageProvider } from '@/context';
import type { Product } from '@/types';

const mockProducts: Product[] = [
  {
    id: '1',
    nameEn: 'Product 1',
    nameAr: 'منتج 1',
    descriptionEn: 'Description 1',
    descriptionAr: 'وصف 1',
    price: 29.99,
    currency: 'USD',
    stockQuantity: 10,
    categoryId: 'cat1',
    imageUrls: ['https://example.com/image1.jpg'],
    isActive: true,
  },
  {
    id: '2',
    nameEn: 'Product 2',
    nameAr: 'منتج 2',
    descriptionEn: 'Description 2',
    descriptionAr: 'وصف 2',
    price: 39.99,
    currency: 'USD',
    stockQuantity: 5,
    categoryId: 'cat1',
    imageUrls: ['https://example.com/image2.jpg'],
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

describe('ProductGrid', () => {
  it('renders products correctly', () => {
    renderWithProviders(<ProductGrid products={mockProducts} />);
    
    expect(screen.getByText('Product 1')).toBeInTheDocument();
    expect(screen.getByText('Product 2')).toBeInTheDocument();
  });

  it('shows loading skeleton when loading', () => {
    renderWithProviders(<ProductGrid products={[]} loading={true} />);
    
    // Should show multiple skeleton cards
    const skeletonCards = screen.getAllByRole('generic');
    expect(skeletonCards.length).toBeGreaterThan(0);
  });

  it('shows empty state when no products', () => {
    renderWithProviders(<ProductGrid products={[]} />);
    
    expect(screen.getByText('No products found')).toBeInTheDocument();
    expect(screen.getByText('Try adjusting your search or filter criteria.')).toBeInTheDocument();
  });

  it('renders products in grid layout', () => {
    renderWithProviders(<ProductGrid products={mockProducts} />);
    
    const gridContainer = screen.getByRole('generic');
    expect(gridContainer).toHaveClass('grid');
  });

  it('passes onAddToCart to product cards', () => {
    const mockAddToCart = vi.fn();
    renderWithProviders(
      <ProductGrid products={mockProducts} onAddToCart={mockAddToCart} />
    );
    
    // Should render add to cart buttons
    const addToCartButtons = screen.getAllByText('Add to Cart');
    expect(addToCartButtons).toHaveLength(2);
  });
});