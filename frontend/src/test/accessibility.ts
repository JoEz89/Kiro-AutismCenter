import { configureAxe } from 'jest-axe';
import { RenderResult } from '@testing-library/react';

// Configure axe for our specific needs
export const axe = configureAxe({
  rules: {
    // Disable color-contrast rule for now as it requires actual rendering
    'color-contrast': { enabled: false },
    // Enable important accessibility rules
    'aria-allowed-attr': { enabled: true },
    'aria-required-attr': { enabled: true },
    'aria-valid-attr': { enabled: true },
    'aria-valid-attr-value': { enabled: true },
    'button-name': { enabled: true },
    'bypass': { enabled: true },
    'document-title': { enabled: true },
    'duplicate-id': { enabled: true },
    'form-field-multiple-labels': { enabled: true },
    'frame-title': { enabled: true },
    'html-has-lang': { enabled: true },
    'html-lang-valid': { enabled: true },
    'image-alt': { enabled: true },
    'input-image-alt': { enabled: true },
    'label': { enabled: true },
    'link-name': { enabled: true },
    'list': { enabled: true },
    'listitem': { enabled: true },
    'meta-refresh': { enabled: true },
    'meta-viewport': { enabled: true },
    'object-alt': { enabled: true },
    'role-img-alt': { enabled: true },
    'scrollable-region-focusable': { enabled: true },
    'select-name': { enabled: true },
    'server-side-image-map': { enabled: true },
    'svg-img-alt': { enabled: true },
    'td-headers-attr': { enabled: true },
    'th-has-data-cells': { enabled: true },
    'valid-lang': { enabled: true },
    'video-caption': { enabled: true }
  }
});

/**
 * Test accessibility of a rendered component
 */
export const testAccessibility = async (renderResult: RenderResult) => {
  const results = await axe(renderResult.container);
  expect(results).toHaveNoViolations();
};

/**
 * Test keyboard navigation for a component
 */
export const testKeyboardNavigation = (element: HTMLElement) => {
  // Test that element is focusable
  element.focus();
  expect(document.activeElement).toBe(element);
  
  // Test tab navigation
  const tabEvent = new KeyboardEvent('keydown', { key: 'Tab' });
  element.dispatchEvent(tabEvent);
  
  // Test enter key
  const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
  element.dispatchEvent(enterEvent);
  
  // Test escape key
  const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
  element.dispatchEvent(escapeEvent);
};

/**
 * Test screen reader compatibility
 */
export const testScreenReaderCompatibility = (element: HTMLElement) => {
  // Check for proper ARIA attributes
  const hasAriaLabel = element.hasAttribute('aria-label');
  const hasAriaLabelledBy = element.hasAttribute('aria-labelledby');
  const hasAriaDescribedBy = element.hasAttribute('aria-describedby');
  const hasRole = element.hasAttribute('role');
  
  // At least one of these should be present for screen reader compatibility
  const hasAccessibilityInfo = hasAriaLabel || hasAriaLabelledBy || hasAriaDescribedBy || hasRole;
  
  if (!hasAccessibilityInfo && element.tagName !== 'DIV' && element.tagName !== 'SPAN') {
    console.warn(`Element ${element.tagName} may not be accessible to screen readers`);
  }
  
  return {
    hasAriaLabel,
    hasAriaLabelledBy,
    hasAriaDescribedBy,
    hasRole,
    hasAccessibilityInfo
  };
};

/**
 * Test focus management
 */
export const testFocusManagement = (container: HTMLElement) => {
  const focusableElements = container.querySelectorAll(
    'button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href], [tabindex]:not([tabindex="-1"]), [contenteditable="true"]'
  );
  
  return {
    focusableCount: focusableElements.length,
    focusableElements: Array.from(focusableElements) as HTMLElement[]
  };
};

/**
 * Test color contrast (basic check)
 */
export const testColorContrast = (element: HTMLElement) => {
  const styles = window.getComputedStyle(element);
  const backgroundColor = styles.backgroundColor;
  const color = styles.color;
  
  return {
    backgroundColor,
    color,
    hasContrast: backgroundColor !== 'rgba(0, 0, 0, 0)' && color !== 'rgba(0, 0, 0, 0)'
  };
};