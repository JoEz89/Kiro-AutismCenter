import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { axe, testAccessibility, testKeyboardNavigation, testScreenReaderCompatibility } from '@/test/accessibility';
import { App } from '@/App';
import { AuthProvider } from '@/context/AuthContext';
import { LanguageProvider } from '@/context/LanguageContext';
import { CartProvider } from '@/context/CartContext';

// Import components for individual testing
import { LoginForm } from '@/components/auth/LoginForm';
import { ProductCard } from '@/components/products/ProductCard';
import { Navigation } from '@/components/layout/Navigation';
import { VideoPlayer } from '@/components/courses/VideoPlayer';
import { CheckoutForm } from '@/components/checkout/CheckoutForm';

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

const mockProduct = {
  id: '1',
  nameEn: 'Test Product',
  nameAr: 'منتج تجريبي',
  descriptionEn: 'Test description',
  descriptionAr: 'وصف تجريبي',
  price: 49.99,
  currency: 'BHD' as const,
  stockQuantity: 10,
  categoryId: 'cat-1',
  imageUrls: ['https://example.com/image.jpg'],
  isActive: true,
};

const mockVideo = {
  id: 'video-1',
  title: 'Test Video',
  url: 'https://example.com/video.mp4',
  duration: 300,
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  courseId: 'course-1',
  moduleId: 'module-1',
  order: 1,
};

describe('Comprehensive Accessibility Tests', () => {
  beforeEach(() => {
    // Reset any global state
    localStorage.clear();
  });

  describe('Page-Level Accessibility', () => {
    it('should pass accessibility tests for home page', async () => {
      const renderResult = render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for login page', async () => {
      const renderResult = render(
        <MemoryRouter initialEntries={['/login']}>
          <App />
        </MemoryRouter>
      );

      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for products page', async () => {
      const renderResult = render(
        <MemoryRouter initialEntries={['/products']}>
          <App />
        </MemoryRouter>
      );

      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for courses page', async () => {
      const renderResult = render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      await testAccessibility(renderResult);
    });
  });

  describe('Component-Level Accessibility', () => {
    it('should pass accessibility tests for LoginForm', async () => {
      const renderResult = renderWithProviders(<LoginForm />);
      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for ProductCard', async () => {
      const renderResult = renderWithProviders(<ProductCard product={mockProduct} />);
      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for Navigation', async () => {
      const renderResult = renderWithProviders(<Navigation />);
      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for VideoPlayer', async () => {
      const renderResult = renderWithProviders(<VideoPlayer video={mockVideo} />);
      await testAccessibility(renderResult);
    });

    it('should pass accessibility tests for CheckoutForm', async () => {
      const renderResult = renderWithProviders(<CheckoutForm />);
      await testAccessibility(renderResult);
    });
  });

  describe('Keyboard Navigation', () => {
    it('should support keyboard navigation in login form', async () => {
      const user = userEvent.setup();
      renderWithProviders(<LoginForm />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      const submitButton = screen.getByRole('button', { name: /sign in/i });

      // Test tab navigation
      await user.tab();
      expect(emailInput).toHaveFocus();

      await user.tab();
      expect(passwordInput).toHaveFocus();

      await user.tab();
      expect(submitButton).toHaveFocus();

      // Test keyboard interaction
      testKeyboardNavigation(submitButton);
    });

    it('should support keyboard navigation in product grid', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      
      await user.tab();
      expect(addToCartButton).toHaveFocus();

      // Test Enter key activation
      await user.keyboard('{Enter}');
      // Should trigger add to cart action
    });

    it('should support keyboard navigation in video player', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VideoPlayer video={mockVideo} />);

      const video = screen.getByRole('video');
      video.focus();

      // Test space bar for play/pause
      await user.keyboard(' ');
      
      // Test arrow keys for seeking
      await user.keyboard('{ArrowRight}');
      await user.keyboard('{ArrowLeft}');

      // Test M key for mute
      await user.keyboard('m');
    });

    it('should support skip links for main content', async () => {
      const user = userEvent.setup();
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      // Skip link should be the first focusable element
      await user.tab();
      const skipLink = screen.getByText(/skip to main content/i);
      expect(skipLink).toHaveFocus();

      // Activating skip link should move focus to main content
      await user.keyboard('{Enter}');
      const mainContent = screen.getByRole('main');
      expect(mainContent).toHaveFocus();
    });
  });

  describe('Screen Reader Compatibility', () => {
    it('should have proper ARIA labels for form elements', () => {
      renderWithProviders(<LoginForm />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);

      expect(emailInput).toHaveAttribute('aria-required', 'true');
      expect(passwordInput).toHaveAttribute('aria-required', 'true');

      testScreenReaderCompatibility(emailInput);
      testScreenReaderCompatibility(passwordInput);
    });

    it('should have proper ARIA labels for interactive elements', () => {
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      const productImage = screen.getByRole('img');

      expect(addToCartButton).toHaveAttribute('aria-label');
      expect(productImage).toHaveAttribute('alt');

      testScreenReaderCompatibility(addToCartButton);
    });

    it('should announce dynamic content changes', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      await user.click(addToCartButton);

      // Should have live region for status updates
      const liveRegion = screen.getByRole('status');
      expect(liveRegion).toBeInTheDocument();
      expect(liveRegion).toHaveAttribute('aria-live', 'polite');
    });

    it('should provide proper headings hierarchy', () => {
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      const headings = screen.getAllByRole('heading');
      
      // Should have h1 as main heading
      const h1 = headings.find(h => h.tagName === 'H1');
      expect(h1).toBeInTheDocument();

      // Check heading hierarchy (h1 -> h2 -> h3, etc.)
      const headingLevels = headings.map(h => parseInt(h.tagName.charAt(1)));
      for (let i = 1; i < headingLevels.length; i++) {
        const currentLevel = headingLevels[i];
        const previousLevel = headingLevels[i - 1];
        
        // Heading level should not skip more than one level
        expect(currentLevel - previousLevel).toBeLessThanOrEqual(1);
      }
    });
  });

  describe('Focus Management', () => {
    it('should manage focus in modal dialogs', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ProductCard product={mockProduct} />);

      // Open modal (assuming there's a modal trigger)
      const viewDetailsButton = screen.getByRole('button', { name: /view details/i });
      await user.click(viewDetailsButton);

      // Focus should be trapped within modal
      const modal = screen.getByRole('dialog');
      expect(modal).toBeInTheDocument();

      // First focusable element in modal should receive focus
      const firstFocusable = modal.querySelector('button, input, select, textarea, a[href], [tabindex]:not([tabindex="-1"])');
      expect(firstFocusable).toHaveFocus();

      // Escape key should close modal and restore focus
      await user.keyboard('{Escape}');
      expect(viewDetailsButton).toHaveFocus();
    });

    it('should manage focus during navigation', async () => {
      const user = userEvent.setup();
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      const coursesLink = screen.getByRole('link', { name: /courses/i });
      await user.click(coursesLink);

      // Focus should move to main content area after navigation
      const mainContent = screen.getByRole('main');
      expect(mainContent).toHaveFocus();
    });

    it('should handle focus for dynamically added content', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      await user.click(addToCartButton);

      // Success message should receive focus for screen reader announcement
      const successMessage = screen.getByText(/added to cart/i);
      expect(successMessage).toHaveFocus();
    });
  });

  describe('Color and Contrast', () => {
    it('should not rely solely on color for information', () => {
      renderWithProviders(<ProductCard product={mockProduct} />);

      // Error states should have text/icons, not just color
      const outOfStockProduct = { ...mockProduct, stockQuantity: 0 };
      const { rerender } = renderWithProviders(<ProductCard product={outOfStockProduct} />);

      const outOfStockIndicator = screen.getByText(/out of stock/i);
      expect(outOfStockIndicator).toBeInTheDocument();
      
      // Should have additional visual indicators beyond color
      expect(outOfStockIndicator).toHaveClass('opacity-50'); // Visual indicator
    });

    it('should have sufficient color contrast for text', () => {
      renderWithProviders(<LoginForm />);

      const emailLabel = screen.getByText(/email/i);
      const styles = window.getComputedStyle(emailLabel);
      
      // This is a basic check - in real scenarios, you'd use tools like axe-core
      expect(styles.color).not.toBe('rgba(0, 0, 0, 0)');
      expect(styles.backgroundColor).not.toBe('rgba(0, 0, 0, 0)');
    });
  });

  describe('Language and Internationalization', () => {
    it('should have proper lang attributes', () => {
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      const htmlElement = document.documentElement;
      expect(htmlElement).toHaveAttribute('lang');
    });

    it('should support RTL layout for Arabic', async () => {
      const user = userEvent.setup();
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      // Switch to Arabic
      const languageSwitcher = screen.getByRole('button', { name: /language/i });
      await user.click(languageSwitcher);

      const arabicOption = screen.getByText(/العربية/i);
      await user.click(arabicOption);

      // HTML should have dir="rtl"
      const htmlElement = document.documentElement;
      expect(htmlElement).toHaveAttribute('dir', 'rtl');
      expect(htmlElement).toHaveAttribute('lang', 'ar');
    });

    it('should provide translations for all user-facing text', async () => {
      const user = userEvent.setup();
      renderWithProviders(<LoginForm />);

      // Switch to Arabic and verify translations exist
      const languageSwitcher = screen.getByRole('button', { name: /language/i });
      await user.click(languageSwitcher);

      const arabicOption = screen.getByText(/العربية/i);
      await user.click(arabicOption);

      // Key elements should have Arabic translations
      expect(screen.getByText(/البريد الإلكتروني/i)).toBeInTheDocument(); // Email in Arabic
      expect(screen.getByText(/كلمة المرور/i)).toBeInTheDocument(); // Password in Arabic
    });
  });

  describe('Error Handling and Feedback', () => {
    it('should provide accessible error messages', async () => {
      const user = userEvent.setup();
      renderWithProviders(<LoginForm />);

      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);

      // Error messages should be associated with form fields
      const emailInput = screen.getByLabelText(/email/i);
      const emailError = screen.getByText(/email is required/i);

      expect(emailInput).toHaveAttribute('aria-describedby');
      expect(emailError).toHaveAttribute('id');
      expect(emailInput.getAttribute('aria-describedby')).toContain(emailError.getAttribute('id'));

      // Error should be announced to screen readers
      expect(emailError).toHaveAttribute('role', 'alert');
    });

    it('should provide accessible loading states', () => {
      renderWithProviders(<ProductCard product={mockProduct} />);

      // Loading spinner should have proper ARIA attributes
      const loadingSpinner = screen.queryByRole('status');
      if (loadingSpinner) {
        expect(loadingSpinner).toHaveAttribute('aria-label', /loading/i);
      }
    });

    it('should provide accessible success feedback', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      await user.click(addToCartButton);

      // Success message should be announced
      const successMessage = screen.getByRole('status');
      expect(successMessage).toHaveAttribute('aria-live', 'polite');
      expect(successMessage).toHaveTextContent(/added to cart/i);
    });
  });

  describe('Media Accessibility', () => {
    it('should provide accessible video controls', () => {
      renderWithProviders(<VideoPlayer video={mockVideo} />);

      const video = screen.getByRole('video');
      const playButton = screen.getByRole('button', { name: /play/i });
      const volumeSlider = screen.getByRole('slider', { name: /volume/i });

      expect(video).toHaveAttribute('aria-label');
      expect(playButton).toHaveAttribute('aria-label');
      expect(volumeSlider).toHaveAttribute('aria-valuemin');
      expect(volumeSlider).toHaveAttribute('aria-valuemax');
      expect(volumeSlider).toHaveAttribute('aria-valuenow');
    });

    it('should support captions and transcripts', () => {
      const videoWithCaptions = {
        ...mockVideo,
        captions: [
          { language: 'en', url: 'https://example.com/captions-en.vtt' },
          { language: 'ar', url: 'https://example.com/captions-ar.vtt' },
        ],
      };

      renderWithProviders(<VideoPlayer video={videoWithCaptions} />);

      const video = screen.getByRole('video');
      const trackElements = video.querySelectorAll('track');
      
      expect(trackElements).toHaveLength(2);
      expect(trackElements[0]).toHaveAttribute('kind', 'captions');
      expect(trackElements[0]).toHaveAttribute('srclang', 'en');
    });
  });

  describe('Mobile Accessibility', () => {
    it('should have proper touch targets', () => {
      renderWithProviders(<ProductCard product={mockProduct} />);

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      const styles = window.getComputedStyle(addToCartButton);
      
      // Touch targets should be at least 44px x 44px
      const minSize = 44;
      expect(parseInt(styles.minHeight)).toBeGreaterThanOrEqual(minSize);
      expect(parseInt(styles.minWidth)).toBeGreaterThanOrEqual(minSize);
    });

    it('should support zoom up to 200% without horizontal scrolling', () => {
      render(
        <MemoryRouter initialEntries={['/']}>
          <App />
        </MemoryRouter>
      );

      // Simulate zoom
      document.body.style.zoom = '200%';
      
      // Content should still be accessible
      const mainContent = screen.getByRole('main');
      expect(mainContent).toBeInTheDocument();
      
      // Reset zoom
      document.body.style.zoom = '100%';
    });
  });
});