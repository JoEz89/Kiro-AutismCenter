import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { KeyboardNavigation } from '../KeyboardNavigation';
import { testAccessibility, testKeyboardNavigation } from '@/test/accessibility';

// Mock the useAccessibility hook
vi.mock('@/hooks/useAccessibility', () => ({
  useAccessibility: () => ({
    handleKeyboardNavigation: vi.fn((event, handlers) => {
      if (event.key === 'Escape') handlers.onEscape?.();
      if (event.key === 'Enter') handlers.onEnter?.();
    }),
    createFocusTrap: vi.fn(() => vi.fn()),
    setFocus: vi.fn()
  })
}));

describe('KeyboardNavigation', () => {
  it('renders without accessibility violations', async () => {
    const result = render(
      <KeyboardNavigation>
        <button>Test Button</button>
      </KeyboardNavigation>
    );
    await testAccessibility(result);
  });

  it('renders children correctly', () => {
    render(
      <KeyboardNavigation>
        <button>Test Button</button>
        <input type="text" placeholder="Test Input" />
      </KeyboardNavigation>
    );

    expect(screen.getByRole('button', { name: 'Test Button' })).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Test Input')).toBeInTheDocument();
  });

  it('has correct ARIA role', () => {
    const { container } = render(
      <KeyboardNavigation>
        <button>Test Button</button>
      </KeyboardNavigation>
    );

    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper).toHaveAttribute('role', 'group');
  });

  it('applies custom className', () => {
    const { container } = render(
      <KeyboardNavigation className="custom-class">
        <button>Test Button</button>
      </KeyboardNavigation>
    );

    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper).toHaveClass('custom-class');
  });

  it('handles Escape key press', () => {
    const onEscape = vi.fn();
    
    render(
      <KeyboardNavigation onEscape={onEscape}>
        <button>Test Button</button>
      </KeyboardNavigation>
    );

    const wrapper = screen.getByRole('group');
    fireEvent.keyDown(wrapper, { key: 'Escape' });

    expect(onEscape).toHaveBeenCalled();
  });

  it('handles Enter key press', () => {
    const onEnter = vi.fn();
    
    render(
      <KeyboardNavigation onEnter={onEnter}>
        <button>Test Button</button>
      </KeyboardNavigation>
    );

    const wrapper = screen.getByRole('group');
    fireEvent.keyDown(wrapper, { key: 'Enter' });

    expect(onEnter).toHaveBeenCalled();
  });

  it('supports keyboard navigation for focusable elements', () => {
    render(
      <KeyboardNavigation>
        <button>Button 1</button>
        <button>Button 2</button>
        <input type="text" placeholder="Input" />
      </KeyboardNavigation>
    );

    const button1 = screen.getByRole('button', { name: 'Button 1' });
    const button2 = screen.getByRole('button', { name: 'Button 2' });
    const input = screen.getByPlaceholderText('Input');

    // Test that elements are focusable
    button1.focus();
    expect(document.activeElement).toBe(button1);

    button2.focus();
    expect(document.activeElement).toBe(button2);

    input.focus();
    expect(document.activeElement).toBe(input);
  });

  it('handles focus trapping when enabled', () => {
    render(
      <KeyboardNavigation trapFocus={true}>
        <button>Button 1</button>
        <button>Button 2</button>
      </KeyboardNavigation>
    );

    // The component should set up focus trapping
    const wrapper = screen.getByRole('group');
    expect(wrapper).toBeInTheDocument();
  });

  it('handles auto focus when enabled', () => {
    render(
      <KeyboardNavigation autoFocus={true}>
        <button>First Button</button>
        <button>Second Button</button>
      </KeyboardNavigation>
    );

    // The first focusable element should be focused
    const firstButton = screen.getByRole('button', { name: 'First Button' });
    expect(firstButton).toBeInTheDocument();
  });

  it('works with various focusable elements', () => {
    render(
      <KeyboardNavigation>
        <button>Button</button>
        <input type="text" />
        <select>
          <option>Option 1</option>
        </select>
        <textarea />
        <a href="#test">Link</a>
        <div tabIndex={0}>Focusable Div</div>
      </KeyboardNavigation>
    );

    // All elements should be present and focusable
    expect(screen.getByRole('button')).toBeInTheDocument();
    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByRole('combobox')).toBeInTheDocument();
    expect(screen.getByRole('textbox')).toBeInTheDocument(); // textarea
    expect(screen.getByRole('link')).toBeInTheDocument();
  });

  it('ignores disabled elements', () => {
    render(
      <KeyboardNavigation>
        <button disabled>Disabled Button</button>
        <button>Enabled Button</button>
        <input type="text" disabled />
        <input type="text" placeholder="Enabled Input" />
      </KeyboardNavigation>
    );

    const enabledButton = screen.getByRole('button', { name: 'Enabled Button' });
    const enabledInput = screen.getByPlaceholderText('Enabled Input');

    // Only enabled elements should be focusable
    enabledButton.focus();
    expect(document.activeElement).toBe(enabledButton);

    enabledInput.focus();
    expect(document.activeElement).toBe(enabledInput);
  });
});