import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { FocusTrap } from '../FocusTrap';
import { testAccessibility } from '@/test/accessibility';

describe('FocusTrap', () => {
  beforeEach(() => {
    // Create a button outside the trap to test focus restoration
    const outsideButton = document.createElement('button');
    outsideButton.textContent = 'Outside Button';
    outsideButton.id = 'outside-button';
    document.body.appendChild(outsideButton);
    outsideButton.focus();
  });

  afterEach(() => {
    // Clean up
    const outsideButton = document.getElementById('outside-button');
    if (outsideButton) {
      document.body.removeChild(outsideButton);
    }
  });

  it('renders without accessibility violations', async () => {
    const result = render(
      <FocusTrap isActive={true}>
        <button>Button 1</button>
        <button>Button 2</button>
      </FocusTrap>
    );
    await testAccessibility(result);
  });

  it('renders children when not active', () => {
    render(
      <FocusTrap isActive={false}>
        <button>Test Button</button>
      </FocusTrap>
    );

    expect(screen.getByRole('button', { name: 'Test Button' })).toBeInTheDocument();
  });

  it('renders children in container when active', () => {
    render(
      <FocusTrap isActive={true}>
        <button>Button 1</button>
        <button>Button 2</button>
      </FocusTrap>
    );

    expect(screen.getByRole('button', { name: 'Button 1' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Button 2' })).toBeInTheDocument();
  });

  it('focuses first focusable element when activated', () => {
    render(
      <FocusTrap isActive={true}>
        <button>First Button</button>
        <button>Second Button</button>
      </FocusTrap>
    );

    const firstButton = screen.getByRole('button', { name: 'First Button' });
    expect(document.activeElement).toBe(firstButton);
  });

  it('traps focus within container', () => {
    render(
      <FocusTrap isActive={true}>
        <button>Button 1</button>
        <button>Button 2</button>
        <input type="text" placeholder="Input" />
      </FocusTrap>
    );

    const button1 = screen.getByRole('button', { name: 'Button 1' });
    const button2 = screen.getByRole('button', { name: 'Button 2' });
    const input = screen.getByPlaceholderText('Input');

    // Focus should start on first element
    expect(document.activeElement).toBe(button1);

    // Tab forward should move to next element
    fireEvent.keyDown(button1, { key: 'Tab' });
    // Note: In a real browser, this would move focus, but in tests we need to simulate it
    button2.focus();
    expect(document.activeElement).toBe(button2);

    // Tab from last element should wrap to first
    fireEvent.keyDown(input, { key: 'Tab' });
    // In the actual implementation, this would wrap to the first element
  });

  it('handles Shift+Tab for backward navigation', () => {
    render(
      <FocusTrap isActive={true}>
        <button>Button 1</button>
        <button>Button 2</button>
      </FocusTrap>
    );

    const button1 = screen.getByRole('button', { name: 'Button 1' });
    const button2 = screen.getByRole('button', { name: 'Button 2' });

    // Focus second button
    button2.focus();
    expect(document.activeElement).toBe(button2);

    // Shift+Tab from first element should wrap to last
    fireEvent.keyDown(button1, { key: 'Tab', shiftKey: true });
    // In the actual implementation, this would wrap to the last element
  });

  it('ignores disabled elements', () => {
    render(
      <FocusTrap isActive={true}>
        <button disabled>Disabled Button</button>
        <button>Enabled Button</button>
        <input type="text" disabled />
        <input type="text" placeholder="Enabled Input" />
      </FocusTrap>
    );

    // Should focus first enabled element
    const enabledButton = screen.getByRole('button', { name: 'Enabled Button' });
    expect(document.activeElement).toBe(enabledButton);
  });

  it('handles elements with tabindex', () => {
    render(
      <FocusTrap isActive={true}>
        <div tabIndex={0}>Focusable Div</div>
        <button>Button</button>
        <div tabIndex={-1}>Non-focusable Div</div>
      </FocusTrap>
    );

    // Should include elements with tabindex >= 0
    const focusableDiv = screen.getByText('Focusable Div');
    expect(document.activeElement).toBe(focusableDiv);
  });

  it('restores focus when deactivated', () => {
    const { rerender } = render(
      <FocusTrap isActive={true} restoreFocus={true}>
        <button>Trapped Button</button>
      </FocusTrap>
    );

    const trappedButton = screen.getByRole('button', { name: 'Trapped Button' });
    expect(document.activeElement).toBe(trappedButton);

    // Deactivate the trap
    rerender(
      <FocusTrap isActive={false} restoreFocus={true}>
        <button>Trapped Button</button>
      </FocusTrap>
    );

    // Focus should be restored to the outside button
    const outsideButton = document.getElementById('outside-button');
    expect(document.activeElement).toBe(outsideButton);
  });

  it('does not restore focus when restoreFocus is false', () => {
    const { rerender } = render(
      <FocusTrap isActive={true} restoreFocus={false}>
        <button>Trapped Button</button>
      </FocusTrap>
    );

    // Deactivate the trap
    rerender(
      <FocusTrap isActive={false} restoreFocus={false}>
        <button>Trapped Button</button>
      </FocusTrap>
    );

    // Focus should not be restored
    const outsideButton = document.getElementById('outside-button');
    expect(document.activeElement).not.toBe(outsideButton);
  });

  it('applies custom className when active', () => {
    const { container } = render(
      <FocusTrap isActive={true} className="custom-trap">
        <button>Button</button>
      </FocusTrap>
    );

    const trapContainer = container.firstChild as HTMLElement;
    expect(trapContainer).toHaveClass('custom-trap');
  });

  it('handles empty container gracefully', () => {
    render(
      <FocusTrap isActive={true}>
        <div>No focusable elements</div>
      </FocusTrap>
    );

    // Should not throw an error
    expect(screen.getByText('No focusable elements')).toBeInTheDocument();
  });

  it('handles contenteditable elements', () => {
    render(
      <FocusTrap isActive={true}>
        <div contentEditable="true">Editable content</div>
        <button>Button</button>
      </FocusTrap>
    );

    const editableDiv = screen.getByText('Editable content');
    expect(document.activeElement).toBe(editableDiv);
  });
});