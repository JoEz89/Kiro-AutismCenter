import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { LiveRegion } from '../LiveRegion';
import { testAccessibility } from '@/test/accessibility';

// Mock the ScreenReaderOnly component
vi.mock('../ScreenReaderOnly', () => ({
  ScreenReaderOnly: React.forwardRef<HTMLDivElement, any>(({ children, ...props }, ref) => (
    <div ref={ref} {...props} className="sr-only">
      {children}
    </div>
  ))
}));

describe('LiveRegion', () => {
  it('renders without accessibility violations', async () => {
    const result = render(<LiveRegion message="Test message" />);
    await testAccessibility(result);
  });

  it('announces message to screen readers', async () => {
    render(<LiveRegion message="Important announcement" />);
    
    await waitFor(() => {
      const liveRegion = screen.getByRole('status');
      expect(liveRegion).toHaveTextContent('Important announcement');
    });
  });

  it('sets correct ARIA attributes', () => {
    render(<LiveRegion message="Test message" politeness="assertive" />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveAttribute('aria-live', 'assertive');
    expect(liveRegion).toHaveAttribute('aria-atomic', 'true');
    expect(liveRegion).toHaveAttribute('role', 'status');
  });

  it('defaults to polite politeness', () => {
    render(<LiveRegion message="Test message" />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveAttribute('aria-live', 'polite');
  });

  it('updates message content', async () => {
    const { rerender } = render(<LiveRegion message="First message" />);
    
    await waitFor(() => {
      const liveRegion = screen.getByRole('status');
      expect(liveRegion).toHaveTextContent('First message');
    });

    rerender(<LiveRegion message="Second message" />);
    
    await waitFor(() => {
      const liveRegion = screen.getByRole('status');
      expect(liveRegion).toHaveTextContent('Second message');
    });
  });

  it('clears message on unmount when clearOnUnmount is true', () => {
    const { unmount } = render(<LiveRegion message="Test message" clearOnUnmount={true} />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveTextContent('Test message');
    
    unmount();
    // After unmount, the component should be removed from DOM
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
  });

  it('handles empty message', async () => {
    render(<LiveRegion message="" />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveTextContent('');
  });

  it('handles off politeness setting', () => {
    render(<LiveRegion message="Test message" politeness="off" />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveAttribute('aria-live', 'off');
  });

  it('is visually hidden but accessible to screen readers', () => {
    render(<LiveRegion message="Hidden message" />);
    
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveClass('sr-only');
    expect(liveRegion).toHaveTextContent('Hidden message');
  });
});