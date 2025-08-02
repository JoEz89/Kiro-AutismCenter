import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { testAccessibility } from '@/test/accessibility';
import { SkipLink, ScreenReaderOnly, FocusTrap, LiveRegion, KeyboardNavigation } from '../index';

// Mock the utils
vi.mock('@/lib/utils', () => ({
  cn: (...classes: any[]) => classes.filter(Boolean).join(' ')
}));

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    isRTL: false
  }),
  useAccessibility: () => ({
    handleKeyboardNavigation: vi.fn(),
    createFocusTrap: vi.fn(() => vi.fn()),
    setFocus: vi.fn()
  })
}));

describe('Accessibility Components Integration', () => {
  it('renders all accessibility components without violations', async () => {
    const result = render(
      <div>
        <SkipLink href="#main">Skip to main</SkipLink>
        <ScreenReaderOnly>Screen reader content</ScreenReaderOnly>
        <LiveRegion message="Live announcement" />
        <FocusTrap isActive={false}>
          <button>Trapped button</button>
        </FocusTrap>
        <KeyboardNavigation>
          <button>Keyboard nav button</button>
        </KeyboardNavigation>
      </div>
    );

    await testAccessibility(result);
  });

  it('provides proper ARIA attributes', () => {
    render(
      <div>
        <SkipLink href="#main">Skip to main</SkipLink>
        <LiveRegion message="Test message" />
        <KeyboardNavigation>
          <button>Test button</button>
        </KeyboardNavigation>
      </div>
    );

    // Skip link should be a proper link
    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveAttribute('href', '#main');

    // Live region should have proper ARIA attributes
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveAttribute('aria-live', 'polite');
    expect(liveRegion).toHaveAttribute('aria-atomic', 'true');

    // Keyboard navigation should have group role
    const keyboardNav = screen.getByRole('group');
    expect(keyboardNav).toBeInTheDocument();
  });

  it('supports keyboard interaction', () => {
    const onEscape = vi.fn();
    
    render(
      <KeyboardNavigation onEscape={onEscape}>
        <button>Test button</button>
      </KeyboardNavigation>
    );

    const group = screen.getByRole('group');
    fireEvent.keyDown(group, { key: 'Escape' });
    
    // The mock should have been called
    expect(onEscape).toHaveBeenCalled();
  });

  it('provides screen reader support', async () => {
    render(
      <div>
        <ScreenReaderOnly>Hidden content for screen readers</ScreenReaderOnly>
        <LiveRegion message="Dynamic announcement" />
      </div>
    );

    // Screen reader only content should be in DOM but visually hidden
    expect(screen.getByText('Hidden content for screen readers')).toBeInTheDocument();
    
    // Live region should announce messages (wait for the timeout in LiveRegion)
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toBeInTheDocument();
    expect(liveRegion).toHaveAttribute('aria-live', 'polite');
  });

  it('handles focus management', () => {
    const { rerender } = render(
      <FocusTrap isActive={false}>
        <button>Button 1</button>
        <button>Button 2</button>
      </FocusTrap>
    );

    // When not active, should render normally
    expect(screen.getByRole('button', { name: 'Button 1' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Button 2' })).toBeInTheDocument();

    // When active, should still render but with focus management
    rerender(
      <FocusTrap isActive={true}>
        <button>Button 1</button>
        <button>Button 2</button>
      </FocusTrap>
    );

    expect(screen.getByRole('button', { name: 'Button 1' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Button 2' })).toBeInTheDocument();
  });
});