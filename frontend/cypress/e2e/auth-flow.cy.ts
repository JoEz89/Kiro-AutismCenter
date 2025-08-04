describe('Authentication Flow', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  describe('User Registration', () => {
    it('should complete user registration successfully', () => {
      // Navigate to registration page
      cy.get('[data-testid="register-link"]').click();
      cy.url().should('include', '/register');

      // Fill registration form
      cy.get('[data-testid="first-name-input"]').type('John');
      cy.get('[data-testid="last-name-input"]').type('Doe');
      cy.get('[data-testid="email-input"]').type('john.doe@example.com');
      cy.get('[data-testid="password-input"]').type('SecurePass123!');
      cy.get('[data-testid="confirm-password-input"]').type('SecurePass123!');

      // Accept terms and conditions
      cy.get('[data-testid="terms-checkbox"]').check();

      // Submit form
      cy.get('[data-testid="register-button"]').click();

      // Should show success message
      cy.get('[data-testid="success-message"]')
        .should('be.visible')
        .and('contain', 'Registration successful');

      // Should show email verification notice
      cy.get('[data-testid="verification-notice"]')
        .should('be.visible')
        .and('contain', 'Please check your email');
    });

    it('should validate registration form fields', () => {
      cy.visit('/register');

      // Try to submit empty form
      cy.get('[data-testid="register-button"]').click();

      // Should show validation errors
      cy.get('[data-testid="first-name-error"]').should('contain', 'First name is required');
      cy.get('[data-testid="last-name-error"]').should('contain', 'Last name is required');
      cy.get('[data-testid="email-error"]').should('contain', 'Email is required');
      cy.get('[data-testid="password-error"]').should('contain', 'Password is required');

      // Test invalid email format
      cy.get('[data-testid="email-input"]').type('invalid-email');
      cy.get('[data-testid="register-button"]').click();
      cy.get('[data-testid="email-error"]').should('contain', 'Please enter a valid email');

      // Test password mismatch
      cy.get('[data-testid="password-input"]').type('password123');
      cy.get('[data-testid="confirm-password-input"]').type('different123');
      cy.get('[data-testid="register-button"]').click();
      cy.get('[data-testid="confirm-password-error"]').should('contain', 'Passwords do not match');
    });

    it('should be accessible', () => {
      cy.visit('/register');
      cy.checkA11y();
    });
  });

  describe('User Login', () => {
    it('should login successfully with valid credentials', () => {
      cy.visit('/login');

      // Fill login form
      cy.get('[data-testid="email-input"]').type('user@example.com');
      cy.get('[data-testid="password-input"]').type('UserPass123!');

      // Submit form
      cy.get('[data-testid="login-button"]').click();

      // Should redirect to home page
      cy.url().should('not.include', '/login');
      cy.get('[data-testid="user-menu"]').should('be.visible');
      cy.get('[data-testid="user-name"]').should('contain', 'John Doe');
    });

    it('should show error for invalid credentials', () => {
      cy.visit('/login');

      // Fill with invalid credentials
      cy.get('[data-testid="email-input"]').type('user@example.com');
      cy.get('[data-testid="password-input"]').type('wrongpassword');

      // Submit form
      cy.get('[data-testid="login-button"]').click();

      // Should show error message
      cy.get('[data-testid="error-message"]')
        .should('be.visible')
        .and('contain', 'Invalid credentials');

      // Should remain on login page
      cy.url().should('include', '/login');
    });

    it('should support Google OAuth login', () => {
      cy.visit('/login');

      // Mock Google OAuth response
      cy.window().then((win) => {
        cy.stub(win, 'open').as('googleLogin');
      });

      cy.get('[data-testid="google-login-button"]').click();
      cy.get('@googleLogin').should('have.been.called');
    });

    it('should validate login form fields', () => {
      cy.visit('/login');

      // Try to submit empty form
      cy.get('[data-testid="login-button"]').click();

      // Should show validation errors
      cy.get('[data-testid="email-error"]').should('contain', 'Email is required');
      cy.get('[data-testid="password-error"]').should('contain', 'Password is required');
    });

    it('should be accessible', () => {
      cy.visit('/login');
      cy.checkA11y();
    });
  });

  describe('Password Reset', () => {
    it('should send password reset email', () => {
      cy.visit('/login');

      // Click forgot password link
      cy.get('[data-testid="forgot-password-link"]').click();
      cy.url().should('include', '/forgot-password');

      // Fill email
      cy.get('[data-testid="email-input"]').type('user@example.com');

      // Submit form
      cy.get('[data-testid="send-reset-email-button"]').click();

      // Should show success message
      cy.get('[data-testid="success-message"]')
        .should('be.visible')
        .and('contain', 'Password reset email sent');
    });

    it('should reset password with valid token', () => {
      // Visit reset password page with token
      cy.visit('/reset-password?token=valid-reset-token');

      // Fill new password
      cy.get('[data-testid="new-password-input"]').type('NewSecurePass123!');
      cy.get('[data-testid="confirm-password-input"]').type('NewSecurePass123!');

      // Submit form
      cy.get('[data-testid="reset-password-button"]').click();

      // Should show success message
      cy.get('[data-testid="success-message"]')
        .should('be.visible')
        .and('contain', 'Password reset successfully');

      // Should redirect to login
      cy.url().should('include', '/login');
    });
  });

  describe('Protected Routes', () => {
    it('should redirect unauthenticated users to login', () => {
      // Try to access protected route
      cy.visit('/courses');

      // Should redirect to login
      cy.url().should('include', '/login');
      cy.get('[data-testid="login-form"]').should('be.visible');
    });

    it('should allow authenticated users to access protected routes', () => {
      // Login first
      cy.loginAsUser();

      // Visit protected route
      cy.visit('/courses');

      // Should show courses page
      cy.url().should('include', '/courses');
      cy.get('[data-testid="courses-page"]').should('be.visible');
    });
  });

  describe('Logout', () => {
    it('should logout user successfully', () => {
      // Login first
      cy.loginAsUser();
      cy.visit('/');

      // Open user menu and logout
      cy.get('[data-testid="user-menu"]').click();
      cy.get('[data-testid="logout-button"]').click();

      // Should redirect to home page
      cy.url().should('eq', Cypress.config().baseUrl + '/');

      // Should show login button instead of user menu
      cy.get('[data-testid="login-link"]').should('be.visible');
      cy.get('[data-testid="user-menu"]').should('not.exist');

      // Local storage should be cleared
      cy.window().its('localStorage.authToken').should('not.exist');
    });
  });

  describe('Session Management', () => {
    it('should maintain session across page refreshes', () => {
      // Login
      cy.loginAsUser();
      cy.visit('/');

      // Verify user is logged in
      cy.get('[data-testid="user-menu"]').should('be.visible');

      // Refresh page
      cy.reload();

      // Should still be logged in
      cy.get('[data-testid="user-menu"]').should('be.visible');
    });

    it('should handle expired tokens gracefully', () => {
      // Set expired token
      cy.window().then((win) => {
        win.localStorage.setItem('authToken', 'expired-token');
      });

      cy.visit('/courses');

      // Should redirect to login
      cy.url().should('include', '/login');
      cy.get('[data-testid="error-message"]')
        .should('be.visible')
        .and('contain', 'Session expired');
    });
  });
});