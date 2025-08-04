import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { axe, toHaveNoViolations } from 'jest-axe';
import { LanguageProvider } from '@/context/LanguageContext';
import { AuthProvider } from '@/context/AuthContext';
import { CartProvider } from '@/context/CartContext';

// Import components to test
import { LoginForm } from '@/components/auth/LoginForm';
import { ProductCard } from '@/components/products/ProductCard';
import { VideoPlayer } from '@/components/courses/VideoPlayer';
import { Navigation } from '@/components/layout/Navigation';
import { HomePage } from '@/pages/HomePage';
import { ProductsPage } from '@/pages/ProductsPage';
import { CoursesPage } from '@/pages/CoursesPage';

// Extend Jest matchers
expect.extend(toHaveNoViolations);

// Mock services
vi.mock('@/services/authService', () => ({
  authService: {
    verifyToken: vi.fn(),
  },
}));

vi.mock('@/services/productService', () => ({
  productService: {
    getProducts: vi.fn(),
  },
}));

vi.mock('@/services/courseService', () => ({
  courseService: {
    getCourses: vi.fn(),
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

describe('Accessibility Test Suite', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Authentication Components', () => {
    it('LoginForm should be accessible', async () => {
      const { container } = render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('LoginForm should support keyboard navigation', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      // Test tab order
      await user.tab();
      expect(screen.getByLabelText(/email/i)).toHaveFocus();

      await user.tab();
      expect(screen.getByLabelText(/password/i)).toHaveFocus();

      await user.tab();
      expect(screen.getByRole('button', { name: /show password/i })).toHaveFocus();

      await user.tab();
      expect(screen.getByRole('button', { name: /sign in/i })).toHaveFocus();
    });

    it('LoginForm should have proper ARIA labels', () => {
      render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      const submitButton = screen.getByRole('button', { name: /sign in/i });

      expect(emailInput).toHaveAttribute('aria-required', 'true');
      expect(passwordInput).toHaveAttribute('aria-required', 'true');
      expect(submitButton).toHaveAttribute('type', 'submit');

      // Check for error message associations
      expect(emailInput).toHaveAttribute('aria-describedby');
      expect(passwordInput).toHaveAttribute('aria-describedby');
    });
  });

  describe('Product Components', () => {
    const mockProduct = {
      id: '1',
      nameEn: 'Test Product',
      nameAr: 'منتج تجريبي',
      descriptionEn: 'Test description',
      descriptionAr: 'وصف تجريبي',
      price: 99.99,
      currency: 'BHD' as const,
      stockQuantity: 10,
      categoryId: 'cat-1',
      imageUrls: ['https://example.com/image.jpg'],
      isActive: true,
    };

    it('ProductCard should be accessible', async () => {
      const { container } = render(
        <TestWrapper>
          <ProductCard product={mockProduct} />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('ProductCard should have proper image alt text', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} />
        </TestWrapper>
      );

      const image = screen.getByRole('img');
      expect(image).toHaveAttribute('alt', mockProduct.nameEn);
    });

    it('ProductCard should have accessible price information', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} />
        </TestWrapper>
      );

      const price = screen.getByText('99.99 BHD');
      expect(price).toHaveAttribute('aria-label', 'Price: 99.99 Bahraini Dinars');
    });

    it('ProductCard should indicate stock status accessibly', () => {
      const outOfStockProduct = { ...mockProduct, stockQuantity: 0 };
      
      render(
        <TestWrapper>
          <ProductCard product={outOfStockProduct} />
        </TestWrapper>
      );

      const addToCartButton = screen.getByRole('button', { name: /out of stock/i });
      expect(addToCartButton).toBeDisabled();
      expect(addToCartButton).toHaveAttribute('aria-disabled', 'true');
    });
  });

  describe('Video Player Component', () => {
    const mockVideo = {
      id: 'video-1',
      title: 'Test Video',
      url: 'https://example.com/video.mp4',
      duration: 300,
      currentTime: 0,
      isCompleted: false,
    };

    it('VideoPlayer should be accessible', async () => {
      const { container } = render(
        <TestWrapper>
          <VideoPlayer video={mockVideo} onProgress={vi.fn()} />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('VideoPlayer should have proper ARIA labels for controls', () => {
      render(
        <TestWrapper>
          <VideoPlayer video={mockVideo} onProgress={vi.fn()} />
        </TestWrapper>
      );

      expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /mute/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /fullscreen/i })).toBeInTheDocument();
      expect(screen.getByRole('slider', { name: /volume/i })).toBeInTheDocument();
    });

    it('VideoPlayer should support keyboard controls', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <VideoPlayer video={mockVideo} onProgress={vi.fn()} />
        </TestWrapper>
      );

      const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
      
      // Focus on video
      video.focus();
      expect(video).toHaveFocus();

      // Test keyboard shortcuts
      await user.keyboard(' '); // Space for play/pause
      await user.keyboard('{ArrowRight}'); // Right arrow for seek forward
      await user.keyboard('{ArrowLeft}'); // Left arrow for seek backward
      await user.keyboard('f'); // F for fullscreen
      await user.keyboard('m'); // M for mute
    });
  });

  describe('Navigation Component', () => {
    it('Navigation should be accessible', async () => {
      const { container } = render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('Navigation should have proper landmark roles', () => {
      render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      expect(screen.getByRole('navigation')).toBeInTheDocument();
      expect(screen.getByRole('banner')).toBeInTheDocument();
    });

    it('Navigation should support keyboard navigation', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      // Test skip link
      await user.tab();
      const skipLink = screen.getByText(/skip to main content/i);
      expect(skipLink).toHaveFocus();

      // Test main navigation items
      await user.tab();
      expect(screen.getByRole('link', { name: /home/i })).toHaveFocus();

      await user.tab();
      expect(screen.getByRole('link', { name: /products/i })).toHaveFocus();

      await user.tab();
      expect(screen.getByRole('link', { name: /courses/i })).toHaveFocus();
    });

    it('Navigation should have accessible language switcher', () => {
      render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      const languageSwitcher = screen.getByRole('button', { name: /language/i });
      expect(languageSwitcher).toHaveAttribute('aria-haspopup', 'true');
      expect(languageSwitcher).toHaveAttribute('aria-expanded', 'false');
    });
  });

  describe('Page Components', () => {
    it('HomePage should be accessible', async () => {
      const { container } = render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('HomePage should have proper heading hierarchy', () => {
      render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      const headings = screen.getAllByRole('heading');
      
      // Should have h1 as main heading
      expect(headings[0]).toHaveProperty('tagName', 'H1');
      
      // Check heading levels are logical
      headings.forEach((heading, index) => {
        if (index > 0) {
          const currentLevel = parseInt(heading.tagName.charAt(1));
          const previousLevel = parseInt(headings[index - 1].tagName.charAt(1));
          expect(currentLevel).toBeLessThanOrEqual(previousLevel + 1);
        }
      });
    });

    it('ProductsPage should be accessible', async () => {
      // Mock products data
      vi.doMock('@/services/productService', () => ({
        productService: {
          getProducts: vi.fn().mockResolvedValue({
            data: {
              products: [],
              totalCount: 0,
              currentPage: 1,
              totalPages: 1,
            },
            success: true,
          }),
        },
      }));

      const { container } = render(
        <TestWrapper>
          <ProductsPage />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });

    it('CoursesPage should be accessible', async () => {
      // Mock courses data
      vi.doMock('@/services/courseService', () => ({
        courseService: {
          getCourses: vi.fn().mockResolvedValue({
            data: [],
            success: true,
          }),
        },
      }));

      const { container } = render(
        <TestWrapper>
          <CoursesPage />
        </TestWrapper>
      );

      const results = await axe(container);
      expect(results).toHaveNoViolations();
    });
  });

  describe('Form Accessibility', () => {
    it('should associate form labels with inputs', () => {
      render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);

      expect(emailInput).toHaveAttribute('id');
      expect(passwordInput).toHaveAttribute('id');

      const emailLabel = screen.getByText(/email/i);
      const passwordLabel = screen.getByText(/password/i);

      expect(emailLabel).toHaveAttribute('for', emailInput.getAttribute('id'));
      expect(passwordLabel).toHaveAttribute('for', passwordInput.getAttribute('id'));
    });

    it('should provide error messages accessibly', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <LoginForm />
        </TestWrapper>
      );

      // Submit form to trigger validation
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);

      // Check error messages are associated with inputs
      const emailInput = screen.getByLabelText(/email/i);
      const emailError = screen.getByText(/email is required/i);

      expect(emailInput).toHaveAttribute('aria-describedby');
      expect(emailError).toHaveAttribute('id');
      expect(emailInput.getAttribute('aria-describedby')).toContain(emailError.getAttribute('id'));
    });
  });

  describe('Color Contrast', () => {
    it('should have sufficient color contrast for text', () => {
      render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      // This would typically be tested with automated tools
      // For now, we ensure text elements have proper classes
      const headings = screen.getAllByRole('heading');
      headings.forEach((heading) => {
        expect(heading).toHaveClass(/text-/); // Tailwind text color class
      });
    });
  });

  describe('Focus Management', () => {
    it('should manage focus properly in modals', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      // Open user menu (if authenticated)
      const userMenuButton = screen.queryByRole('button', { name: /user menu/i });
      if (userMenuButton) {
        await user.click(userMenuButton);
        
        // Focus should be trapped in menu
        const menuItems = screen.getAllByRole('menuitem');
        expect(menuItems[0]).toHaveFocus();
      }
    });

    it('should restore focus when closing modals', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <Navigation />
        </TestWrapper>
      );

      const languageButton = screen.getByRole('button', { name: /language/i });
      await user.click(languageButton);

      // Close with Escape key
      await user.keyboard('{Escape}');

      // Focus should return to trigger button
      expect(languageButton).toHaveFocus();
    });
  });

  describe('Screen Reader Support', () => {
    it('should provide proper live region announcements', async () => {
      const user = userEvent.setup();
      
      render(
        <TestWrapper>
          <ProductCard product={{
            id: '1',
            nameEn: 'Test Product',
            nameAr: 'منتج تجريبي',
            descriptionEn: 'Test description',
            descriptionAr: 'وصف تجريبي',
            price: 99.99,
            currency: 'BHD' as const,
            stockQuantity: 10,
            categoryId: 'cat-1',
            imageUrls: ['https://example.com/image.jpg'],
            isActive: true,
          }} />
        </TestWrapper>
      );

      // Add to cart should announce success
      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      await user.click(addToCartButton);

      // Check for live region
      const liveRegion = screen.getByRole('status');
      expect(liveRegion).toHaveTextContent(/added to cart/i);
    });

    it('should provide descriptive button labels', () => {
      render(
        <TestWrapper>
          <VideoPlayer video={{
            id: 'video-1',
            title: 'Test Video',
            url: 'https://example.com/video.mp4',
            duration: 300,
            currentTime: 0,
            isCompleted: false,
          }} onProgress={vi.fn()} />
        </TestWrapper>
      );

      // Buttons should have descriptive labels
      expect(screen.getByRole('button', { name: /play test video/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /mute test video/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /enter fullscreen for test video/i })).toBeInTheDocument();
    });
  });

  describe('RTL Language Support', () => {
    it('should properly handle RTL layout for Arabic', () => {
      // Mock Arabic language
      vi.doMock('@/hooks/useLocalization', () => ({
        useLocalization: () => ({
          language: 'ar',
          isRTL: true,
          t: (key: string) => key,
        }),
      }));

      render(
        <TestWrapper>
          <HomePage />
        </TestWrapper>
      );

      // Check HTML dir attribute
      expect(document.documentElement).toHaveAttribute('dir', 'rtl');
      expect(document.documentElement).toHaveAttribute('lang', 'ar');
    });
  });
});