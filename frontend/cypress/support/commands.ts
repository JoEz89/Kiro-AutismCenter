/// <reference types="cypress" />

// Custom commands for authentication
Cypress.Commands.add('login', (email: string, password: string) => {
  cy.session([email, password], () => {
    cy.visit('/login');
    cy.get('[data-testid="email-input"]').type(email);
    cy.get('[data-testid="password-input"]').type(password);
    cy.get('[data-testid="login-button"]').click();
    cy.url().should('not.include', '/login');
    cy.window().its('localStorage.authToken').should('exist');
  });
});

Cypress.Commands.add('loginAsAdmin', () => {
  cy.login('admin@autismcenter.com', 'AdminPass123!');
});

Cypress.Commands.add('loginAsUser', () => {
  cy.login('user@example.com', 'UserPass123!');
});

// Custom commands for API interactions
Cypress.Commands.add('apiLogin', (email: string, password: string) => {
  return cy.request({
    method: 'POST',
    url: `${Cypress.env('apiUrl')}/auth/login`,
    body: {
      email,
      password,
    },
  }).then((response) => {
    expect(response.status).to.eq(200);
    const { token, user } = response.body.data;
    window.localStorage.setItem('authToken', token);
    window.localStorage.setItem('user', JSON.stringify(user));
    return response;
  });
});

// Custom commands for cart operations
Cypress.Commands.add('addToCart', (productId: string, quantity: number = 1) => {
  cy.get(`[data-testid="product-${productId}"] [data-testid="add-to-cart-button"]`).click();
  if (quantity > 1) {
    for (let i = 1; i < quantity; i++) {
      cy.get(`[data-testid="product-${productId}"] [data-testid="increase-quantity"]`).click();
    }
  }
});

Cypress.Commands.add('clearCart', () => {
  cy.get('[data-testid="cart-icon"]').click();
  cy.get('[data-testid="clear-cart-button"]').click();
  cy.get('[data-testid="confirm-clear-cart"]').click();
});

// Custom commands for accessibility testing
Cypress.Commands.add('checkA11y', (context?: string, options?: any) => {
  cy.checkA11y(context, options, (violations) => {
    violations.forEach((violation) => {
      cy.log(`Accessibility violation: ${violation.description}`);
      cy.log(`Help: ${violation.helpUrl}`);
      violation.nodes.forEach((node) => {
        cy.log(`Target: ${node.target.join(', ')}`);
        cy.log(`HTML: ${node.html}`);
      });
    });
  });
});

// Custom commands for language switching
Cypress.Commands.add('switchLanguage', (language: 'en' | 'ar') => {
  cy.get('[data-testid="language-switcher"]').click();
  cy.get(`[data-testid="language-option-${language}"]`).click();
  cy.get('html').should('have.attr', 'lang', language);
  if (language === 'ar') {
    cy.get('html').should('have.attr', 'dir', 'rtl');
  } else {
    cy.get('html').should('have.attr', 'dir', 'ltr');
  }
});

// Custom commands for form interactions
Cypress.Commands.add('fillCheckoutForm', (formData: {
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  city: string;
  postalCode: string;
  cardNumber: string;
  expiryDate: string;
  cvv: string;
}) => {
  // Shipping information
  cy.get('[data-testid="first-name-input"]').type(formData.firstName);
  cy.get('[data-testid="last-name-input"]').type(formData.lastName);
  cy.get('[data-testid="email-input"]').type(formData.email);
  cy.get('[data-testid="address-input"]').type(formData.address);
  cy.get('[data-testid="city-input"]').type(formData.city);
  cy.get('[data-testid="postal-code-input"]').type(formData.postalCode);
  
  // Continue to payment
  cy.get('[data-testid="continue-to-payment"]').click();
  
  // Payment information
  cy.get('[data-testid="card-number-input"]').type(formData.cardNumber);
  cy.get('[data-testid="expiry-date-input"]').type(formData.expiryDate);
  cy.get('[data-testid="cvv-input"]').type(formData.cvv);
});

// Custom commands for waiting
Cypress.Commands.add('waitForPageLoad', () => {
  cy.get('[data-testid="loading-spinner"]', { timeout: 10000 }).should('not.exist');
});

Cypress.Commands.add('waitForApiCall', (alias: string) => {
  cy.wait(alias).its('response.statusCode').should('eq', 200);
});

// Declare custom commands for TypeScript
declare global {
  namespace Cypress {
    interface Chainable {
      login(email: string, password: string): Chainable<void>;
      loginAsAdmin(): Chainable<void>;
      loginAsUser(): Chainable<void>;
      apiLogin(email: string, password: string): Chainable<Response<any>>;
      addToCart(productId: string, quantity?: number): Chainable<void>;
      clearCart(): Chainable<void>;
      checkA11y(context?: string, options?: any): Chainable<void>;
      switchLanguage(language: 'en' | 'ar'): Chainable<void>;
      fillCheckoutForm(formData: {
        firstName: string;
        lastName: string;
        email: string;
        address: string;
        city: string;
        postalCode: string;
        cardNumber: string;
        expiryDate: string;
        cvv: string;
      }): Chainable<void>;
      waitForPageLoad(): Chainable<void>;
      waitForApiCall(alias: string): Chainable<void>;
    }
  }
}