describe('E-commerce Flow', () => {
  beforeEach(() => {
    cy.loginAsUser();
  });

  describe('Product Browsing', () => {
    it('should display products and allow filtering', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Should display products
      cy.get('[data-testid="product-grid"]').should('be.visible');
      cy.get('[data-testid^="product-"]').should('have.length.greaterThan', 0);

      // Test search functionality
      cy.get('[data-testid="search-input"]').type('autism');
      cy.get('[data-testid="search-button"]').click();

      // Should filter products
      cy.get('[data-testid^="product-"]').each(($product) => {
        cy.wrap($product).should('contain.text', 'autism');
      });

      // Test category filter
      cy.get('[data-testid="category-filter"]').select('learning-materials');
      cy.get('[data-testid="apply-filters"]').click();

      // Should show filtered results
      cy.get('[data-testid="filter-results"]').should('contain', 'learning-materials');

      // Test price range filter
      cy.get('[data-testid="min-price-input"]').type('50');
      cy.get('[data-testid="max-price-input"]').type('200');
      cy.get('[data-testid="apply-filters"]').click();

      // Should show products in price range
      cy.get('[data-testid^="product-"] [data-testid="product-price"]').each(($price) => {
        const price = parseFloat($price.text().replace(/[^\d.]/g, ''));
        expect(price).to.be.within(50, 200);
      });
    });

    it('should navigate to product detail page', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Click on first product
      cy.get('[data-testid^="product-"]:first [data-testid="product-link"]').click();

      // Should navigate to product detail page
      cy.url().should('include', '/products/');
      cy.get('[data-testid="product-detail"]').should('be.visible');
      cy.get('[data-testid="product-title"]').should('be.visible');
      cy.get('[data-testid="product-description"]').should('be.visible');
      cy.get('[data-testid="product-price"]').should('be.visible');
      cy.get('[data-testid="add-to-cart-button"]').should('be.visible');
    });

    it('should be accessible', () => {
      cy.visit('/products');
      cy.waitForPageLoad();
      cy.checkA11y();
    });
  });

  describe('Shopping Cart', () => {
    it('should add products to cart', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Add first product to cart
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();

      // Should show success message
      cy.get('[data-testid="success-message"]')
        .should('be.visible')
        .and('contain', 'Added to cart');

      // Cart icon should show item count
      cy.get('[data-testid="cart-badge"]').should('contain', '1');

      // Add another product
      cy.get('[data-testid^="product-"]:eq(1) [data-testid="add-to-cart-button"]').click();

      // Cart count should update
      cy.get('[data-testid="cart-badge"]').should('contain', '2');
    });

    it('should manage cart items', () => {
      // Add products to cart first
      cy.visit('/products');
      cy.waitForPageLoad();
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();

      // Open cart
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="cart-sidebar"]').should('be.visible');

      // Should show cart items
      cy.get('[data-testid^="cart-item-"]').should('have.length', 1);

      // Increase quantity
      cy.get('[data-testid="increase-quantity"]').click();
      cy.get('[data-testid="item-quantity"]').should('contain', '2');

      // Decrease quantity
      cy.get('[data-testid="decrease-quantity"]').click();
      cy.get('[data-testid="item-quantity"]').should('contain', '1');

      // Remove item
      cy.get('[data-testid="remove-item"]').click();
      cy.get('[data-testid="confirm-remove"]').click();

      // Cart should be empty
      cy.get('[data-testid="empty-cart-message"]').should('be.visible');
      cy.get('[data-testid="cart-badge"]').should('not.exist');
    });

    it('should navigate to cart page', () => {
      // Add product to cart
      cy.visit('/products');
      cy.waitForPageLoad();
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();

      // Navigate to cart page
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="view-cart-button"]').click();

      // Should show cart page
      cy.url().should('include', '/cart');
      cy.get('[data-testid="cart-page"]').should('be.visible');
      cy.get('[data-testid="cart-items"]').should('be.visible');
      cy.get('[data-testid="cart-total"]').should('be.visible');
      cy.get('[data-testid="checkout-button"]').should('be.visible');
    });

    it('should persist cart across sessions', () => {
      // Add product to cart
      cy.visit('/products');
      cy.waitForPageLoad();
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();

      // Refresh page
      cy.reload();

      // Cart should still have items
      cy.get('[data-testid="cart-badge"]').should('contain', '1');
    });
  });

  describe('Checkout Process', () => {
    beforeEach(() => {
      // Add product to cart
      cy.visit('/products');
      cy.waitForPageLoad();
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();
    });

    it('should complete full checkout process', () => {
      // Navigate to checkout
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="checkout-button"]').click();

      // Should show checkout page
      cy.url().should('include', '/checkout');
      cy.get('[data-testid="checkout-form"]').should('be.visible');

      // Fill checkout form
      cy.fillCheckoutForm({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        address: '123 Main St',
        city: 'Manama',
        postalCode: '12345',
        cardNumber: '4242424242424242',
        expiryDate: '12/25',
        cvv: '123',
      });

      // Place order
      cy.get('[data-testid="place-order-button"]').click();

      // Should show order confirmation
      cy.url().should('include', '/order-confirmation');
      cy.get('[data-testid="order-confirmation"]').should('be.visible');
      cy.get('[data-testid="order-number"]').should('be.visible');
      cy.get('[data-testid="order-total"]').should('be.visible');

      // Cart should be empty
      cy.get('[data-testid="cart-badge"]').should('not.exist');
    });

    it('should validate checkout form', () => {
      // Navigate to checkout
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="checkout-button"]').click();

      // Try to submit empty form
      cy.get('[data-testid="continue-to-payment"]').click();

      // Should show validation errors
      cy.get('[data-testid="first-name-error"]').should('contain', 'First name is required');
      cy.get('[data-testid="last-name-error"]').should('contain', 'Last name is required');
      cy.get('[data-testid="address-error"]').should('contain', 'Address is required');
      cy.get('[data-testid="city-error"]').should('contain', 'City is required');
    });

    it('should handle payment errors gracefully', () => {
      // Navigate to checkout
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="checkout-button"]').click();

      // Fill form with invalid card
      cy.fillCheckoutForm({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        address: '123 Main St',
        city: 'Manama',
        postalCode: '12345',
        cardNumber: '4000000000000002', // Declined card
        expiryDate: '12/25',
        cvv: '123',
      });

      // Place order
      cy.get('[data-testid="place-order-button"]').click();

      // Should show payment error
      cy.get('[data-testid="payment-error"]')
        .should('be.visible')
        .and('contain', 'Payment failed');

      // Should remain on checkout page
      cy.url().should('include', '/checkout');
    });

    it('should be accessible', () => {
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="checkout-button"]').click();
      cy.checkA11y();
    });
  });

  describe('Order History', () => {
    it('should display user order history', () => {
      cy.visit('/orders');
      cy.waitForPageLoad();

      // Should show orders page
      cy.get('[data-testid="orders-page"]').should('be.visible');
      cy.get('[data-testid="orders-list"]').should('be.visible');

      // Should show order details
      cy.get('[data-testid^="order-"]:first').within(() => {
        cy.get('[data-testid="order-number"]').should('be.visible');
        cy.get('[data-testid="order-date"]').should('be.visible');
        cy.get('[data-testid="order-status"]').should('be.visible');
        cy.get('[data-testid="order-total"]').should('be.visible');
      });
    });

    it('should show order details when clicked', () => {
      cy.visit('/orders');
      cy.waitForPageLoad();

      // Click on first order
      cy.get('[data-testid^="order-"]:first [data-testid="view-order-button"]').click();

      // Should show order detail page
      cy.url().should('include', '/orders/');
      cy.get('[data-testid="order-detail"]').should('be.visible');
      cy.get('[data-testid="order-items"]').should('be.visible');
      cy.get('[data-testid="shipping-address"]').should('be.visible');
      cy.get('[data-testid="billing-address"]').should('be.visible');
    });
  });

  describe('Product Search and Filtering', () => {
    it('should search products by keyword', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Search for specific keyword
      cy.get('[data-testid="search-input"]').type('learning kit');
      cy.get('[data-testid="search-button"]').click();

      // Should show search results
      cy.get('[data-testid="search-results"]').should('be.visible');
      cy.get('[data-testid^="product-"]').each(($product) => {
        cy.wrap($product).should('contain.text', 'learning');
      });

      // Should show search term in results header
      cy.get('[data-testid="search-term"]').should('contain', 'learning kit');
    });

    it('should filter products by multiple criteria', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Apply multiple filters
      cy.get('[data-testid="category-filter"]').select('toys');
      cy.get('[data-testid="price-range-filter"]').select('50-100');
      cy.get('[data-testid="availability-filter"]').check();
      cy.get('[data-testid="apply-filters"]').click();

      // Should show filtered results
      cy.get('[data-testid="active-filters"]').should('contain', 'toys');
      cy.get('[data-testid="active-filters"]').should('contain', '50-100');
      cy.get('[data-testid="active-filters"]').should('contain', 'In Stock');

      // Clear filters
      cy.get('[data-testid="clear-filters"]').click();

      // Should show all products
      cy.get('[data-testid="active-filters"]').should('not.exist');
    });

    it('should sort products by different criteria', () => {
      cy.visit('/products');
      cy.waitForPageLoad();

      // Sort by price (low to high)
      cy.get('[data-testid="sort-select"]').select('price-asc');

      // Should sort products by price
      cy.get('[data-testid^="product-"] [data-testid="product-price"]').then(($prices) => {
        const prices = Array.from($prices).map((el) => 
          parseFloat(el.textContent?.replace(/[^\d.]/g, '') || '0')
        );
        const sortedPrices = [...prices].sort((a, b) => a - b);
        expect(prices).to.deep.equal(sortedPrices);
      });

      // Sort by name (A-Z)
      cy.get('[data-testid="sort-select"]').select('name-asc');

      // Should sort products by name
      cy.get('[data-testid^="product-"] [data-testid="product-name"]').then(($names) => {
        const names = Array.from($names).map((el) => el.textContent || '');
        const sortedNames = [...names].sort();
        expect(names).to.deep.equal(sortedNames);
      });
    });
  });

  describe('Responsive Design', () => {
    it('should work on mobile devices', () => {
      cy.viewport('iphone-x');
      cy.visit('/products');
      cy.waitForPageLoad();

      // Should show mobile layout
      cy.get('[data-testid="mobile-menu-button"]').should('be.visible');
      cy.get('[data-testid="product-grid"]').should('have.class', 'mobile-grid');

      // Mobile cart should work
      cy.get('[data-testid^="product-"]:first [data-testid="add-to-cart-button"]').click();
      cy.get('[data-testid="cart-icon"]').click();
      cy.get('[data-testid="mobile-cart-drawer"]').should('be.visible');
    });

    it('should work on tablet devices', () => {
      cy.viewport('ipad-2');
      cy.visit('/products');
      cy.waitForPageLoad();

      // Should show tablet layout
      cy.get('[data-testid="product-grid"]').should('have.class', 'tablet-grid');
      cy.get('[data-testid="sidebar-filters"]').should('be.visible');
    });
  });
});