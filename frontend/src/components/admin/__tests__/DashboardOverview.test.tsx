import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { DashboardOverview } from '../DashboardOverview';

// Mock the hooks
jest.mock('@/hooks', () => ({
  useLocalization: () => ({
    t: (key: string, fallback?: string) => fallback || key,
    isRTL: false,
  }),
}));

// Mock the child components
jest.mock('../MetricCard', () => ({
  MetricCard: ({ title, value }: { title: string; value: string }) => (
    <div data-testid="metric-card">
      <div>{title}</div>
      <div>{value}</div>
    </div>
  ),
}));

jest.mock('../RevenueChart', () => ({
  RevenueChart: () => <div data-testid="revenue-chart">Revenue Chart</div>,
}));

jest.mock('../OrderStatusChart', () => ({
  OrderStatusChart: () => <div data-testid="order-status-chart">Order Status Chart</div>,
}));

jest.mock('../RecentActivity', () => ({
  RecentActivity: () => <div data-testid="recent-activity">Recent Activity</div>,
}));

describe('DashboardOverview', () => {
  it('renders loading state initially', () => {
    render(<DashboardOverview />);
    
    expect(screen.getByRole('status')).toBeInTheDocument(); // LoadingSpinner
  });

  it('renders dashboard content after loading', async () => {
    render(<DashboardOverview />);

    await waitFor(() => {
      expect(screen.getByText('Welcome to Admin Dashboard')).toBeInTheDocument();
    });

    // Check for metric cards
    expect(screen.getAllByTestId('metric-card')).toHaveLength(5);
    
    // Check for charts
    expect(screen.getByTestId('revenue-chart')).toBeInTheDocument();
    expect(screen.getByTestId('order-status-chart')).toBeInTheDocument();
    
    // Check for recent activity
    expect(screen.getByTestId('recent-activity')).toBeInTheDocument();
  });

  it('displays welcome message and description', async () => {
    render(<DashboardOverview />);

    await waitFor(() => {
      expect(screen.getByText('Welcome to Admin Dashboard')).toBeInTheDocument();
      expect(screen.getByText("Here's an overview of your autism center's performance.")).toBeInTheDocument();
    });
  });

  it('displays metric cards with correct titles', async () => {
    render(<DashboardOverview />);

    await waitFor(() => {
      expect(screen.getByText('Total Revenue')).toBeInTheDocument();
      expect(screen.getByText('Total Orders')).toBeInTheDocument();
      expect(screen.getByText('Total Users')).toBeInTheDocument();
      expect(screen.getByText('Total Courses')).toBeInTheDocument();
      expect(screen.getByText('Total Appointments')).toBeInTheDocument();
    });
  });

  it('displays chart sections', async () => {
    render(<DashboardOverview />);

    await waitFor(() => {
      expect(screen.getByText('Revenue Over Time')).toBeInTheDocument();
      expect(screen.getByText('Order Status Distribution')).toBeInTheDocument();
      expect(screen.getByText('Recent Activity')).toBeInTheDocument();
    });
  });
});