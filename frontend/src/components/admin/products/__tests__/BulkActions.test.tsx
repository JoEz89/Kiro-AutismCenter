import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import BulkActions from '../BulkActions';
import { useLocalization } from '@/hooks';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
}));

// Mock the Button component
vi.mock('@/components/ui', () => ({
  Button: ({ children, onClick, disabled, ...props }: any) => (
    <button onClick={onClick} disabled={disabled} {...props}>
      {children}
    </button>
  ),
}));

const mockUseLocalization = useLocalization as vi.MockedFunction<typeof useLocalization>;

const defaultProps = {
  selectedCount: 3,
  selectedProductIds: ['1', '2', '3'],
  onAction: vi.fn(),
};

describe('BulkActions', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string, options?: any) => {
        if (key === 'admin.products.selectedItems' && options?.count) {
          return `${options.count} items selected`;
        }
        const translations: Record<string, string> = {
          'admin.products.activate': 'Activate',
          'admin.products.deactivate': 'Deactivate',
          'admin.products.delete': 'Delete',
        };
        return translations[key] || fallback || key;
      },
      isRTL: false,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn(),
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly with selected items count', () => {
    render(<BulkActions {...defaultProps} />);
    
    expect(screen.getByText('3 items selected')).toBeInTheDocument();
    expect(screen.getByText('admin.products.activate')).toBeInTheDocument();
    expect(screen.getByText('admin.products.deactivate')).toBeInTheDocument();
    expect(screen.getByText('admin.products.delete')).toBeInTheDocument();
  });

  it('handles activate action', async () => {
    const onAction = vi.fn().mockResolvedValue(undefined);
    render(<BulkActions {...defaultProps} onAction={onAction} />);
    
    const activateButton = screen.getByText('admin.products.activate');
    fireEvent.click(activateButton);
    
    await waitFor(() => {
      expect(onAction).toHaveBeenCalledWith('activate', ['1', '2', '3']);
    });
  });

  it('handles deactivate action', async () => {
    const onAction = vi.fn().mockResolvedValue(undefined);
    render(<BulkActions {...defaultProps} onAction={onAction} />);
    
    const deactivateButton = screen.getByText('admin.products.deactivate');
    fireEvent.click(deactivateButton);
    
    await waitFor(() => {
      expect(onAction).toHaveBeenCalledWith('deactivate', ['1', '2', '3']);
    });
  });

  it('handles delete action', async () => {
    const onAction = vi.fn().mockResolvedValue(undefined);
    render(<BulkActions {...defaultProps} onAction={onAction} />);
    
    const deleteButton = screen.getByText('admin.products.delete');
    fireEvent.click(deleteButton);
    
    await waitFor(() => {
      expect(onAction).toHaveBeenCalledWith('delete', ['1', '2', '3']);
    });
  });

  it('shows loading state during action execution', async () => {
    const onAction = vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
    render(<BulkActions {...defaultProps} onAction={onAction} />);
    
    const activateButton = screen.getByText('admin.products.activate');
    fireEvent.click(activateButton);
    
    // Check for loading spinner
    expect(screen.getByRole('generic')).toHaveClass('animate-spin');
    
    await waitFor(() => {
      expect(screen.queryByRole('generic')).not.toHaveClass('animate-spin');
    });
  });

  it('disables buttons during loading', async () => {
    const onAction = vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
    render(<BulkActions {...defaultProps} onAction={onAction} />);
    
    const activateButton = screen.getByText('admin.products.activate');
    const deactivateButton = screen.getByText('admin.products.deactivate');
    const deleteButton = screen.getByText('admin.products.delete');
    
    fireEvent.click(activateButton);
    
    expect(activateButton).toBeDisabled();
    expect(deactivateButton).toBeDisabled();
    expect(deleteButton).toBeDisabled();
    
    await waitFor(() => {
      expect(activateButton).not.toBeDisabled();
      expect(deactivateButton).not.toBeDisabled();
      expect(deleteButton).not.toBeDisabled();
    });
  });

  it('renders with correct styling classes', () => {
    render(<BulkActions {...defaultProps} />);
    
    const container = screen.getByText('3 items selected').closest('div');
    expect(container).toHaveClass('bg-blue-50', 'dark:bg-blue-900/20');
  });

  it('handles single item selection', () => {
    render(<BulkActions {...defaultProps} selectedCount={1} selectedProductIds={['1']} />);
    
    expect(screen.getByText('1 items selected')).toBeInTheDocument();
  });

  it('handles zero selection gracefully', () => {
    render(<BulkActions {...defaultProps} selectedCount={0} selectedProductIds={[]} />);
    
    expect(screen.getByText('0 items selected')).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<BulkActions {...defaultProps} />);
    
    const activateButton = screen.getByText('admin.products.activate');
    const deactivateButton = screen.getByText('admin.products.deactivate');
    const deleteButton = screen.getByText('admin.products.delete');
    
    expect(activateButton).toHaveAttribute('type', 'button');
    expect(deactivateButton).toHaveAttribute('type', 'button');
    expect(deleteButton).toHaveAttribute('type', 'button');
  });

  it('displays correct icons for each action', () => {
    render(<BulkActions {...defaultProps} />);
    
    // Check that SVG icons are present
    const svgElements = screen.getAllByRole('generic');
    const svgIcons = svgElements.filter(el => el.tagName === 'svg');
    
    // Should have icons for activate, deactivate, delete, and the check icon
    expect(svgIcons.length).toBeGreaterThan(0);
  });
});