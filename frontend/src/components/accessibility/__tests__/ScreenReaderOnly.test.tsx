import React from 'react';
import { render, screen } from '@testing-library/react';
import { ScreenReaderOnly } from '../ScreenReaderOnly';

// Mock the utils
vi.mock('@/lib/utils', () => ({
  cn: (...classes: any[]) => classes.filter(Boolean).join(' ')
}));

describe('ScreenReaderOnly', () => {
  it('renders children content', () => {
    render(
      <ScreenReaderOnly>
        Screen reader only content
      </ScreenReaderOnly>
    );

    expect(screen.getByText('Screen reader only content')).toBeInTheDocument();
  });

  it('applies screen reader only classes', () => {
    const { container } = render(
      <ScreenReaderOnly>
        Hidden content
      </ScreenReaderOnly>
    );

    const element = container.firstChild as HTMLElement;
    expect(element).toHaveClass('sr-only');
    expect(element).toHaveClass('absolute', 'w-px', 'h-px', 'p-0', '-m-px', 'overflow-hidden');
    expect(element).toHaveClass('whitespace-nowrap', 'border-0');
  });

  it('renders as span by default', () => {
    const { container } = render(
      <ScreenReaderOnly>
        Content
      </ScreenReaderOnly>
    );

    const element = container.firstChild as HTMLElement;
    expect(element.tagName).toBe('SPAN');
  });

  it('renders as custom element when specified', () => {
    const { container } = render(
      <ScreenReaderOnly as="div">
        Content
      </ScreenReaderOnly>
    );

    const element = container.firstChild as HTMLElement;
    expect(element.tagName).toBe('DIV');
  });

  it('applies custom className', () => {
    const { container } = render(
      <ScreenReaderOnly className="custom-class">
        Content
      </ScreenReaderOnly>
    );

    const element = container.firstChild as HTMLElement;
    expect(element).toHaveClass('custom-class');
  });

  it('is visually hidden but accessible to screen readers', () => {
    const { container } = render(
      <ScreenReaderOnly>
        Important announcement
      </ScreenReaderOnly>
    );

    const element = container.firstChild as HTMLElement;
    
    // Should have screen reader only styling
    expect(element).toHaveClass('sr-only');
    
    // Content should still be in the DOM for screen readers
    expect(screen.getByText('Important announcement')).toBeInTheDocument();
  });
});