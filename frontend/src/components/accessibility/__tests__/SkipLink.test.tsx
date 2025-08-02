import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { SkipLink } from '../SkipLink';

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    isRTL: false
  })
}));

// Mock the utils
vi.mock('@/lib/utils', () => ({
  cn: (...classes: any[]) => classes.filter(Boolean).join(' ')
}));

describe('SkipLink', () => {
  beforeEach(() => {
    // Create a target element for the skip link
    const targetElement = document.createElement('main');
    targetElement.id = 'main-content';
    targetElement.setAttribute('tabindex', '-1');
    document.body.appendChild(targetElement);
  });

  afterEach(() => {
    // Clean up
    const targetElement = document.getElementById('main-content');
    if (targetElement) {
      document.body.removeChild(targetElement);
    }
  });

  it('renders skip link with correct href', () => {
    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveAttribute('href', '#main-content');
    expect(skipLink).toHaveTextContent('Skip to main content');
  });

  it('is positioned off-screen by default', () => {
    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveClass('-top-40');
  });

  it('becomes visible when focused', () => {
    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveClass('focus:top-6');
  });

  it('handles Enter key press', () => {
    const scrollIntoViewMock = vi.fn();
    const focusMock = vi.fn();
    
    // Mock the target element
    const targetElement = document.getElementById('main-content');
    if (targetElement) {
      targetElement.scrollIntoView = scrollIntoViewMock;
      targetElement.focus = focusMock;
    }

    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    fireEvent.keyDown(skipLink, { key: 'Enter' });

    expect(scrollIntoViewMock).toHaveBeenCalledWith({ behavior: 'smooth' });
    expect(focusMock).toHaveBeenCalled();
  });

  it('handles Space key press', () => {
    const scrollIntoViewMock = vi.fn();
    const focusMock = vi.fn();
    
    // Mock the target element
    const targetElement = document.getElementById('main-content');
    if (targetElement) {
      targetElement.scrollIntoView = scrollIntoViewMock;
      targetElement.focus = focusMock;
    }

    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    fireEvent.keyDown(skipLink, { key: ' ' });

    expect(scrollIntoViewMock).toHaveBeenCalledWith({ behavior: 'smooth' });
    expect(focusMock).toHaveBeenCalled();
  });

  it('applies custom className', () => {
    render(
      <SkipLink href="#main-content" className="custom-class">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveClass('custom-class');
  });

  it('has proper accessibility attributes', () => {
    render(
      <SkipLink href="#main-content">
        Skip to main content
      </SkipLink>
    );

    const skipLink = screen.getByRole('link');
    expect(skipLink).toHaveClass('focus:outline-none', 'focus:ring-4', 'focus:ring-blue-300');
  });
});