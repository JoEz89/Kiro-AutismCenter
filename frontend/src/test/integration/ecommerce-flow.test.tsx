import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { App } from '@/App';
import { productService } from '@/services/productService';
import { cartService } from '@/services/cartService';
import { checkoutService } from '@/services/checkoutService';
import { authService } from '@/services/authService';
import { Product, User, UserRole } from '@/types';

// Mock services
vi.mock('@/services/productService', () => ({
  productService: {
    getProducts: vi.fn(),
    getProductById: vi.fn(),
    searchProducts: vi.fn(),
  },
}));

vi.mock('@/services/cartService', () => ({
  cartService: {
    getCart: vi.fn(),
    addToCart: vi.fn(),
    updateCartItem: vi.fn(),
    removeFromCart: vi.fn(),
    clearCart: vi.fn(),
  },
}));

vi.mock('@/services/checkoutService', () => ({
  checkoutService: {
    createOrder: vi.fn(),
    processPayment: vi.fn(),
  },
}));

vi.mock('@/services/authService', () => ({
  authService: {
    verifyToken: vi.fn(),
  },
}));

const mockProductService = vi.mocked(productService);
const mockCartService = vi.mocked(cartService);
const mockCheckoutService = vi.mocked(checkoutService);
const mockAuthService = vi.mocked(authService);

const mockProducts: Product[] = [
  {
    id: '1',
    nameEn: 'Autism Learning Kit',
    nameAr: 'مجموعة تعلم التوحد',
    descriptionEn: 'A comprehensive learning kit for children with autism',
    descriptionAr: 'مجموعة تعلم شاملة للأطفال المصابين بالتوحد',
    price: 99.99,
    currency: 'BHD',
    stockQuantity: 10,
    categoryId: 'cat-1',
    imageUrls: ['https://example.com/image1.jpg'],
    isActive: true,
  },
  {
    id: '2',
    nameEn: 'Sensory Toys Set',
    nameAr: 'مجموعة ألعاب حسية',
    descriptionEn: 'Sensory toys for autism therapy',
    descriptionAr: 'ألعاب حسية لعلاج التوحد',
    price: 49.99,
    currency: 'BHD',
    stockQuantity: 5,
    categoryId: 'cat-1',
    imageUrls: ['https://example.com/image2.jpg'],
    isActive: true,
  },
];

const mockUser: User = {
  id: '1',
  email: 'john.doe@example.com',
  firstName: 'John',
  lastName: 'Doe',
  role: UserRole.USER,
  preferredLanguage: 'en',
  isEmailVerified: true,
  createdAt: new Date(),
};

describe('E-commerce Flow Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    
    // Set up authenticated user
    localStorage.setItem('authToken', 'mock-token');
    localStorage.setItem('user', JSON.stringify(mockUser));
    
    mockAuthService.verifyToken.mockResolvedValue({
      data: mockUser,
      success: true,
    });
  });

  describe('Product Browsing Flow', () => {
    it('should display products and allow filtering', async () => {
      const user = userEvent.setup();

      mockProductService.getProducts.mockResolvedValueOnce({
        data: {
          products: mockProducts,
          totalCount: 2,
          currentPage: 1,
          totalPages: 1,
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/products']}>
          <App />
        </MemoryRouter>
      );

      // Wait for products to load
      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
        expect(screen.getByText('Sensory Toys Set')).toBeInTheDocument();
      });

      // Test search functionality
      const searchInput = screen.getByPlaceholderText(/search products/i);
      await user.type(searchInput, 'learning');

      await waitFor(() => {
        expect(mockProductService.searchProducts).toHaveBeenCalledWith(
          expect.objectContaining({
            query: 'learning',
          })
        );
      });
    });

    it('should navigate to product detail page', async () => {
      const user = userEvent.setup();

      mockProductService.getProducts.mockResolvedValueOnce({
        data: {
          products: mockProducts,
          totalCount: 2,
          currentPage: 1,
          totalPages: 1,
        },
        success: true,
      });

      mockProductService.getProductById.mockResolvedValueOnce({
        data: mockProducts[0],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/products']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
      });

      // Click on product
      const productLink = screen.getByRole('link', { name: /autism learning kit/i });
      await user.click(productLink);

      // Should navigate to product detail page
      await waitFor(() => {
        expect(mockProductService.getProductById).toHaveBeenCalledWith('1');
      });
    });
  });

  describe('Shopping Cart Flow', () => {
    it('should add products to cart and update quantities', async () => {
      const user = userEvent.setup();

      mockProductService.getProducts.mockResolvedValueOnce({
        data: {
          products: mockProducts,
          totalCount: 2,
          currentPage: 1,
          totalPages: 1,
        },
        success: true,
      });

      mockCartService.addToCart.mockResolvedValueOnce({
        data: { message: 'Product added to cart' },
        success: true,
      });

      mockCartService.getCart.mockResolvedValueOnce({
        data: {
          items: [
            {
              productId: '1',
              quantity: 1,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
          totalAmount: 99.99,
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/products']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
      });

      // Add product to cart
      const addToCartButton = screen.getAllByRole('button', { name: /add to cart/i })[0];
      await user.click(addToCartButton);

      await waitFor(() => {
        expect(mockCartService.addToCart).toHaveBeenCalledWith('1', 1);
      });

      // Check cart icon shows item count
      expect(screen.getByText('1')).toBeInTheDocument(); // Cart badge
    });

    it('should remove items from cart', async () => {
      const user = userEvent.setup();

      mockCartService.getCart.mockResolvedValueOnce({
        data: {
          items: [
            {
              productId: '1',
              quantity: 2,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
          totalAmount: 199.98,
        },
        success: true,
      });

      mockCartService.removeFromCart.mockResolvedValueOnce({
        data: { message: 'Item removed from cart' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/cart']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
        expect(screen.getByText('199.98 BHD')).toBeInTheDocument();
      });

      // Remove item from cart
      const removeButton = screen.getByRole('button', { name: /remove/i });
      await user.click(removeButton);

      await waitFor(() => {
        expect(mockCartService.removeFromCart).toHaveBeenCalledWith('1');
      });
    });

    it('should update item quantities in cart', async () => {
      const user = userEvent.setup();

      mockCartService.getCart.mockResolvedValueOnce({
        data: {
          items: [
            {
              productId: '1',
              quantity: 1,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
          totalAmount: 99.99,
        },
        success: true,
      });

      mockCartService.updateCartItem.mockResolvedValueOnce({
        data: { message: 'Cart updated' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/cart']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
      });

      // Increase quantity
      const increaseButton = screen.getByRole('button', { name: /increase quantity/i });
      await user.click(increaseButton);

      await waitFor(() => {
        expect(mockCartService.updateCartItem).toHaveBeenCalledWith('1', 2);
      });
    });
  });

  describe('Checkout Flow', () => {
    it('should complete full checkout process', async () => {
      const user = userEvent.setup();

      mockCartService.getCart.mockResolvedValueOnce({
        data: {
          items: [
            {
              productId: '1',
              quantity: 1,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
          totalAmount: 99.99,
        },
        success: true,
      });

      mockCheckoutService.createOrder.mockResolvedValueOnce({
        data: {
          orderId: 'order-123',
          orderNumber: 'ORD-2024-001234',
          totalAmount: 99.99,
          status: 'pending',
        },
        success: true,
      });

      mockCheckoutService.processPayment.mockResolvedValueOnce({
        data: {
          paymentId: 'payment-123',
          status: 'completed',
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/checkout']}>
          <App />
        </MemoryRouter>
      );

      // Fill shipping address
      await waitFor(() => {
        expect(screen.getByText(/shipping address/i)).toBeInTheDocument();
      });

      const firstNameInput = screen.getByLabelText(/first name/i);
      const lastNameInput = screen.getByLabelText(/last name/i);
      const addressInput = screen.getByLabelText(/address/i);
      const cityInput = screen.getByLabelText(/city/i);
      const postalCodeInput = screen.getByLabelText(/postal code/i);

      await user.type(firstNameInput, 'John');
      await user.type(lastNameInput, 'Doe');
      await user.type(addressInput, '123 Main St');
      await user.type(cityInput, 'Manama');
      await user.type(postalCodeInput, '12345');

      // Continue to payment
      const continueButton = screen.getByRole('button', { name: /continue to payment/i });
      await user.click(continueButton);

      // Fill payment information
      await waitFor(() => {
        expect(screen.getByText(/payment information/i)).toBeInTheDocument();
      });

      const cardNumberInput = screen.getByLabelText(/card number/i);
      const expiryInput = screen.getByLabelText(/expiry date/i);
      const cvvInput = screen.getByLabelText(/cvv/i);

      await user.type(cardNumberInput, '4242424242424242');
      await user.type(expiryInput, '12/25');
      await user.type(cvvInput, '123');

      // Place order
      const placeOrderButton = screen.getByRole('button', { name: /place order/i });
      await user.click(placeOrderButton);

      await waitFor(() => {
        expect(mockCheckoutService.createOrder).toHaveBeenCalled();
        expect(mockCheckoutService.processPayment).toHaveBeenCalled();
      });

      // Should redirect to order confirmation
      await waitFor(() => {
        expect(screen.getByText(/order confirmed/i)).toBeInTheDocument();
        expect(screen.getByText('ORD-2024-001234')).toBeInTheDocument();
      });
    });

    it('should handle payment failures gracefully', async () => {
      const user = userEvent.setup();

      mockCartService.getCart.mockResolvedValueOnce({
        data: {
          items: [
            {
              productId: '1',
              quantity: 1,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
          totalAmount: 99.99,
        },
        success: true,
      });

      mockCheckoutService.createOrder.mockResolvedValueOnce({
        data: {
          orderId: 'order-123',
          orderNumber: 'ORD-2024-001234',
          totalAmount: 99.99,
          status: 'pending',
        },
        success: true,
      });

      mockCheckoutService.processPayment.mockRejectedValueOnce(
        new Error('Payment failed: Insufficient funds')
      );

      render(
        <MemoryRouter initialEntries={['/checkout']}>
          <App />
        </MemoryRouter>
      );

      // Complete checkout form (abbreviated for brevity)
      await waitFor(() => {
        expect(screen.getByText(/shipping address/i)).toBeInTheDocument();
      });

      // Fill required fields and submit
      const placeOrderButton = screen.getByRole('button', { name: /place order/i });
      await user.click(placeOrderButton);

      await waitFor(() => {
        expect(screen.getByText(/payment failed/i)).toBeInTheDocument();
        expect(screen.getByText(/insufficient funds/i)).toBeInTheDocument();
      });

      // Should remain on checkout page
      expect(screen.getByRole('button', { name: /place order/i })).toBeInTheDocument();
    });
  });

  describe('Order History Flow', () => {
    it('should display user order history', async () => {
      const mockOrders = [
        {
          id: 'order-1',
          orderNumber: 'ORD-2024-001234',
          totalAmount: 99.99,
          currency: 'BHD',
          status: 'delivered',
          createdAt: new Date('2024-01-15'),
          items: [
            {
              productId: '1',
              quantity: 1,
              price: 99.99,
              product: mockProducts[0],
            },
          ],
        },
      ];

      // Mock order service
      vi.doMock('@/services/orderService', () => ({
        orderService: {
          getUserOrders: vi.fn().mockResolvedValue({
            data: mockOrders,
            success: true,
          }),
        },
      }));

      render(
        <MemoryRouter initialEntries={['/orders']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('ORD-2024-001234')).toBeInTheDocument();
        expect(screen.getByText('99.99 BHD')).toBeInTheDocument();
        expect(screen.getByText(/delivered/i)).toBeInTheDocument();
      });
    });
  });

  describe('Product Search and Filtering', () => {
    it('should filter products by category and price', async () => {
      const user = userEvent.setup();

      mockProductService.getProducts.mockResolvedValueOnce({
        data: {
          products: mockProducts,
          totalCount: 2,
          currentPage: 1,
          totalPages: 1,
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/products']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Autism Learning Kit')).toBeInTheDocument();
      });

      // Apply price filter
      const priceFilter = screen.getByLabelText(/price range/i);
      await user.selectOptions(priceFilter, '0-50');

      await waitFor(() => {
        expect(mockProductService.getProducts).toHaveBeenCalledWith(
          expect.objectContaining({
            minPrice: 0,
            maxPrice: 50,
          })
        );
      });

      // Apply category filter
      const categoryFilter = screen.getByLabelText(/category/i);
      await user.selectOptions(categoryFilter, 'learning-materials');

      await waitFor(() => {
        expect(mockProductService.getProducts).toHaveBeenCalledWith(
          expect.objectContaining({
            categoryId: 'learning-materials',
          })
        );
      });
    });
  });
});