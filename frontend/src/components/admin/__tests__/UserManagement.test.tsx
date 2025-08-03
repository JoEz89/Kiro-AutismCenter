import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { UserManagement } from '../UserManagement';
import { useLocalization } from '@/hooks';
import { UserRole } from '@/types';

// Mock hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
}));

// Mock UI components
vi.mock('@/components/ui', () => ({
  LoadingSpinner: ({ size }: { size?: string }) => <div data-testid="loading-spinner" data-size={size} />,
  Modal: ({ isOpen, onClose, title, children }: any) => 
    isOpen ? (
      <div data-testid="modal">
        <div data-testid="modal-title">{title}</div>
        <button onClick={onClose} data-testid="modal-close">Close</button>
        {children}
      </div>
    ) : null,
  Pagination: ({ currentPage, totalPages, onPageChange }: any) => (
    <div data-testid="pagination">
      <button onClick={() => onPageChange(currentPage - 1)} disabled={currentPage === 1}>
        Previous
      </button>
      <span>{currentPage} of {totalPages}</span>
      <button onClick={() => onPageChange(currentPage + 1)} disabled={currentPage === totalPages}>
        Next
      </button>
    </div>
  ),
}));

// Mock utils
vi.mock('@/lib/utils', () => ({
  cn: (...classes: any[]) => classes.filter(Boolean).join(' '),
}));

const mockT = vi.fn((key: string, fallback?: string) => fallback || key);
const mockUseLocalization = useLocalization as any;

describe('UserManagement', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      isRTL: false,
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders loading spinner initially', () => {
    render(<UserManagement />);
    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
  });

  it('renders user management interface after loading', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Check for stats cards
    expect(screen.getByText('admin.users.totalUsers')).toBeInTheDocument();
    expect(screen.getByText('admin.users.verifiedUsers')).toBeInTheDocument();
    expect(screen.getByText('admin.users.doctorUsers')).toBeInTheDocument();
  });

  it('renders filters section', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    expect(screen.getByPlaceholderText('admin.users.searchPlaceholder')).toBeInTheDocument();
    expect(screen.getByDisplayValue('all')).toBeInTheDocument(); // Role filter
  });

  it('renders users table with data', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Check table headers
    expect(screen.getByText('admin.users.name')).toBeInTheDocument();
    expect(screen.getByText('admin.users.email')).toBeInTheDocument();
    expect(screen.getByText('admin.users.role')).toBeInTheDocument();
    expect(screen.getByText('admin.users.status')).toBeInTheDocument();
    expect(screen.getByText('admin.users.joinDate')).toBeInTheDocument();

    // Check for mock user data
    expect(screen.getByText('Admin User')).toBeInTheDocument();
    expect(screen.getByText('Dr. Sarah Smith')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });

  it('opens user details modal when view button is clicked', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const viewButtons = screen.getAllByText('common.view');
    fireEvent.click(viewButtons[0]);

    await waitFor(() => {
      expect(screen.getByTestId('modal')).toBeInTheDocument();
      expect(screen.getByTestId('modal-title')).toHaveTextContent('admin.users.userDetails');
    });
  });

  it('opens role change modal when change role button is clicked', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const changeRoleButtons = screen.getAllByText('admin.users.changeRole');
    fireEvent.click(changeRoleButtons[0]);

    await waitFor(() => {
      expect(screen.getByTestId('modal')).toBeInTheDocument();
      expect(screen.getByTestId('modal-title')).toHaveTextContent('admin.users.changeUserRole');
    });
  });

  it('filters users by search term', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('admin.users.searchPlaceholder');
    fireEvent.change(searchInput, { target: { value: 'John' } });

    expect(searchInput).toHaveValue('John');
  });

  it('filters users by role', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const roleSelects = screen.getAllByDisplayValue('all');
    const roleSelect = roleSelects[0]; // First select should be role filter
    fireEvent.change(roleSelect, { target: { value: UserRole.ADMIN } });

    expect(roleSelect).toHaveValue(UserRole.ADMIN);
  });

  it('filters users by verification status', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const statusSelects = screen.getAllByDisplayValue('all');
    const statusSelect = statusSelects[1]; // Second select should be status filter
    fireEvent.change(statusSelect, { target: { value: 'verified' } });

    expect(statusSelect).toHaveValue('verified');
  });

  it('clears filters when clear button is clicked', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Set some filters
    const searchInput = screen.getByPlaceholderText('admin.users.searchPlaceholder');
    fireEvent.change(searchInput, { target: { value: 'test' } });

    const roleSelects = screen.getAllByDisplayValue('all');
    fireEvent.change(roleSelects[0], { target: { value: UserRole.ADMIN } });

    // Clear filters
    const clearButton = screen.getByText('common.clear');
    fireEvent.click(clearButton);

    expect(searchInput).toHaveValue('');
    expect(roleSelects[0]).toHaveValue('all');
  });

  it('shows verify button for unverified users', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Ahmed Ali is unverified in mock data
    const verifyButtons = screen.getAllByText('admin.users.verify');
    expect(verifyButtons.length).toBeGreaterThan(0);
  });

  it('handles email verification', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    const verifyButtons = screen.getAllByText('admin.users.verify');
    if (verifyButtons.length > 0) {
      fireEvent.click(verifyButtons[0]);
      
      // The button should trigger the verification process
      // In a real test, you would mock the API call and verify it was called
    }
  });

  it('handles RTL layout correctly', async () => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      isRTL: true,
    });

    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Component should render without errors in RTL mode
    expect(screen.getByText('admin.users.totalUsers')).toBeInTheDocument();
  });

  it('displays user avatars with initials', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Check for user initials in avatars
    expect(screen.getByText('AU')).toBeInTheDocument(); // Admin User
    expect(screen.getByText('DS')).toBeInTheDocument(); // Dr. Sarah
    expect(screen.getByText('JD')).toBeInTheDocument(); // John Doe
  });

  it('shows correct role badges', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Check for role badges
    expect(screen.getByText('admin.users.adminRole')).toBeInTheDocument();
    expect(screen.getByText('admin.users.doctorRole')).toBeInTheDocument();
    expect(screen.getByText('admin.users.userRole')).toBeInTheDocument();
  });

  it('shows verification status indicators', async () => {
    render(<UserManagement />);
    
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    });

    // Check for verification status
    expect(screen.getAllByText('admin.users.verified').length).toBeGreaterThan(0);
    expect(screen.getAllByText('admin.users.unverified').length).toBeGreaterThan(0);
  });
});