import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { LanguageProvider } from '@/context/LanguageContext';
import { AuthProvider } from '@/context/AuthContext';
import { CartProvider } from '@/context/CartContext';

// Import components for visual testing
import { LoginForm } from '@/components/auth/LoginForm';
import { ProductCard } from '@/components/products/ProductCard';
import { Navigation } from '@/components/layout/Navigation';
import { HomePage } from '@/pages/HomePage';

// Mock services
vi.mock('@/services/authService', () => ({
  authService: {
    verifyToken: vi.fn(),
  },
}));

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

// Mock product data
const mockProduct = {
  id: '1',
  nameEn: 'Autism Learning Kit',
  nameAr: 'مجموعة تعلم التوحد',
  descriptionEn: 'A comprehensive learning kit for children with autism',
  descriptionAr: 'مجموعة تعلم شاملة للأطفال المصابين بالتوحد',
  price: 99.99,
  currency: 'BHD' as const,
  stockQuantity: 10,
  categoryId: 'cat-1',
  imageUrls: ['https://example.com/image1.jpg'],
  isActive: true,
};

describe('Visual Regression Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Set consistent viewport for visual tests
    Object.defineProperty(window, 'innerWidth', { value: 1280 });
    Object.defineProperty(window, 'innerHeight', { value: 720 });
  });

  describe('Authentication Components', () => {
    it('should render LoginForm consistently', () => {
      const { container } = render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      // Take snapshot of the component
      expect(container.firstChild).toMatchSnapshot('login-form');
    });

    it('should render LoginForm with validation errors', async () => {
      const { container } = render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      // Simulate form submission to show errors
      const form = container.querySelector('form');
      if (form) {
        form.dispatchEvent(new Event('submit', { bubbles: true }));
      }

      // Wait for validation errors to appear
      await new Promise(resolve => setTimeout(resolve, 100));

      expect(container.firstChild).toMatchSnapshot('login-form-with-errors');
    });

    it('should render LoginForm in loading state', () => {
      // Mock loading state
      vi.doMock('@/hooks/useAuth', () => ({
        useAuth: () => ({
          login: vi.fn(),
          isLoading: true,
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('login-form-loading');
    });
  });

  describe('Product Components', () => {
    it('should render ProductCard consistently', () => {
      const { container } = render(
        <TestWrapper>
          <ProductCard product={mockProduct} />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('product-card');
    });

    it('should render ProductCard in out-of-stock state', () => {
      const outOfStockProduct = { ...mockProduct, stockQuantity: 0 };

      const { container } = render(
        <TestWrapper>
          <ProductCard product={outOfStockProduct} />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('product-card-out-of-stock');
    });

    it('should render ProductCard with sale badge', () => {
      const saleProduct = {
        ...mockProduct,
        originalPrice: 149.99,
        isOnSale: true,
      };

      const { container } = render(
        <TestWrapper>
          <ProductCard product={saleProduct} />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('product-card-on-sale');
    });

    it('should render ProductCard in Arabic', () => {
      // Mock Arabic language
      vi.doMock('@/hooks/useLocalization', () => ({
        useLocalization: () => ({
          language: 'ar',
          isRTL: true,
          t: (key: string) => key,
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <ProductCard product={mockProduct} />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('product-card-arabic');
    });
  });

  describe('Navigation Component', () => {
    it('should render Navigation for unauthenticated user', () => {
      const { container } = render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('navigation-unauthenticated');
    });

    it('should render Navigation for authenticated user', () => {
      // Mock authenticated state
      vi.doMock('@/hooks/useAuth', () => ({
        useAuth: () => ({
          isAuthenticated: true,
          user: {
            id: '1',
            firstName: 'John',
            lastName: 'Doe',
            email: 'john.doe@example.com',
          },
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('navigation-authenticated');
    });

    it('should render Navigation in Arabic', () => {
      // Mock Arabic language
      vi.doMock('@/hooks/useLocalization', () => ({
        useLocalization: () => ({
          language: 'ar',
          isRTL: true,
          t: (key: string) => key,
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('navigation-arabic');
    });
  });

  describe('Page Components', () => {
    it('should render HomePage consistently', () => {
      const { container } = render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('home-page');
    });

    it('should render HomePage in Arabic', () => {
      // Mock Arabic language
      vi.doMock('@/hooks/useLocalization', () => ({
        useLocalization: () => ({
          language: 'ar',
          isRTL: true,
          t: (key: string) => key,
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('home-page-arabic');
    });
  });

  describe('Responsive Design', () => {
    it('should render components on mobile viewport', () => {
      // Set mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 375 });
      Object.defineProperty(window, 'innerHeight', { value: 667 });

      const { container } = render(
        <TestWrapper>
          <div>
            <Navigation />
            <ProductCard product={mockProduct} />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('mobile-layout');
    });

    it('should render components on tablet viewport', () => {
      // Set tablet viewport
      Object.defineProperty(window, 'innerWidth', { value: 768 });
      Object.defineProperty(window, 'innerHeight', { value: 1024 });

      const { container } = render(
        <TestWrapper>
          <div>
            <Navigation />
            <ProductCard product={mockProduct} />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('tablet-layout');
    });

    it('should render components on desktop viewport', () => {
      // Set desktop viewport
      Object.defineProperty(window, 'innerWidth', { value: 1920 });
      Object.defineProperty(window, 'innerHeight', { value: 1080 });

      const { container } = render(
        <TestWrapper>
          <div>
            <Navigation />
            <ProductCard product={mockProduct} />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('desktop-layout');
    });
  });

  describe('Theme Variations', () => {
    it('should render components in light theme', () => {
      // Mock light theme
      vi.doMock('@/context/ThemeContext', () => ({
        useTheme: () => ({
          theme: 'light',
          toggleTheme: vi.fn(),
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <div>
            <Navigation />
            <LoginForm />
            <ProductCard product={mockProduct} />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('light-theme');
    });

    it('should render components in dark theme', () => {
      // Mock dark theme
      vi.doMock('@/context/ThemeContext', () => ({
        useTheme: () => ({
          theme: 'dark',
          toggleTheme: vi.fn(),
        }),
      }));

      const { container } = render(
        <TestWrapper>
          <div>
            <Navigation />
            <LoginForm />
            <ProductCard product={mockProduct} />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('dark-theme');
    });
  });

  describe('Loading States', () => {
    it('should render loading spinners consistently', () => {
      const { container } = render(
        <TestWrapper>
          <div>
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
            <div className="animate-pulse bg-gray-300 h-4 w-full rounded" />
            <div className="animate-bounce bg-blue-600 h-2 w-2 rounded-full" />
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('loading-states');
    });
  });

  describe('Error States', () => {
    it('should render error messages consistently', () => {
      const { container } = render(
        <TestWrapper>
          <div>
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              <p className="font-medium">Error occurred</p>
              <p className="text-sm">Please try again later</p>
            </div>
            <div className="bg-yellow-50 border border-yellow-200 text-yellow-700 px-4 py-3 rounded">
              <p className="font-medium">Warning</p>
              <p className="text-sm">This action cannot be undone</p>
            </div>
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('error-states');
    });
  });

  describe('Form States', () => {
    it('should render form validation states consistently', () => {
      const { container } = render(
        <TestWrapper>
          <div className="space-y-4">
            {/* Valid input */}
            <div>
              <input
                type="text"
                className="border-green-300 focus:border-green-500 focus:ring-green-500"
                placeholder="Valid input"
              />
              <p className="text-green-600 text-sm">Looks good!</p>
            </div>
            
            {/* Invalid input */}
            <div>
              <input
                type="text"
                className="border-red-300 focus:border-red-500 focus:ring-red-500"
                placeholder="Invalid input"
              />
              <p className="text-red-600 text-sm">This field is required</p>
            </div>
            
            {/* Disabled input */}
            <div>
              <input
                type="text"
                disabled
                className="bg-gray-100 border-gray-300 text-gray-500"
                placeholder="Disabled input"
              />
            </div>
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('form-states');
    });
  });

  describe('Button Variations', () => {
    it('should render button states consistently', () => {
      const { container } = render(
        <TestWrapper>
          <div className="space-x-4 space-y-4">
            <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded">
              Primary Button
            </button>
            <button className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded">
              Secondary Button
            </button>
            <button className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded">
              Danger Button
            </button>
            <button
              disabled
              className="bg-gray-300 text-gray-500 px-4 py-2 rounded cursor-not-allowed"
            >
              Disabled Button
            </button>
            <button className="border border-blue-600 text-blue-600 hover:bg-blue-50 px-4 py-2 rounded">
              Outline Button
            </button>
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('button-variations');
    });
  });

  describe('Card Layouts', () => {
    it('should render card layouts consistently', () => {
      const { container } = render(
        <TestWrapper>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold mb-2">Card Title</h3>
              <p className="text-gray-600 mb-4">Card description goes here</p>
              <button className="bg-blue-600 text-white px-4 py-2 rounded">
                Action
              </button>
            </div>
            <div className="bg-white rounded-lg shadow-md p-6">
              <img
                src="https://example.com/image.jpg"
                alt="Card image"
                className="w-full h-32 object-cover rounded mb-4"
              />
              <h3 className="text-lg font-semibold mb-2">Image Card</h3>
              <p className="text-gray-600">With image content</p>
            </div>
            <div className="bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold mb-2">Gradient Card</h3>
              <p className="mb-4">With gradient background</p>
              <button className="bg-white text-blue-600 px-4 py-2 rounded">
                Action
              </button>
            </div>
          </div>
        </TestWrapper>
      );

      expect(container.firstChild).toMatchSnapshot('card-layouts');
    });
  });
});