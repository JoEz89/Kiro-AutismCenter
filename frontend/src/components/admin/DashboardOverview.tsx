import React, { useEffect, useState } from 'react';
import { useLocalization } from '@/hooks';
import { LoadingSpinner } from '@/components/ui';
import { MetricCard } from './MetricCard';
import { RevenueChart } from './RevenueChart';
import { OrderStatusChart } from './OrderStatusChart';
import { RecentActivity } from './RecentActivity';
import { cn } from '@/lib/utils';

interface DashboardMetrics {
  totalRevenue: number;
  totalOrders: number;
  totalUsers: number;
  totalCourses: number;
  totalAppointments: number;
  revenueGrowth: number;
  ordersGrowth: number;
  usersGrowth: number;
  appointmentsGrowth: number;
}

interface RevenueData {
  month: string;
  revenue: number;
  orders: number;
}

interface OrderStatusData {
  status: string;
  count: number;
  percentage: number;
}

export const DashboardOverview: React.FC = () => {
  const { t, isRTL } = useLocalization();
  const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
  const [revenueData, setRevenueData] = useState<RevenueData[]>([]);
  const [orderStatusData, setOrderStatusData] = useState<OrderStatusData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Mock data for now - replace with actual API calls
      await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate API delay

      const mockMetrics: DashboardMetrics = {
        totalRevenue: 125000,
        totalOrders: 1250,
        totalUsers: 3500,
        totalCourses: 25,
        totalAppointments: 450,
        revenueGrowth: 12.5,
        ordersGrowth: 8.3,
        usersGrowth: 15.2,
        appointmentsGrowth: 22.1,
      };

      const mockRevenueData: RevenueData[] = [
        { month: 'Jan', revenue: 8500, orders: 85 },
        { month: 'Feb', revenue: 9200, orders: 92 },
        { month: 'Mar', revenue: 10100, orders: 101 },
        { month: 'Apr', revenue: 11500, orders: 115 },
        { month: 'May', revenue: 12800, orders: 128 },
        { month: 'Jun', revenue: 14200, orders: 142 },
      ];

      const mockOrderStatusData: OrderStatusData[] = [
        { status: 'completed', count: 850, percentage: 68 },
        { status: 'processing', count: 200, percentage: 16 },
        { status: 'shipped', count: 150, percentage: 12 },
        { status: 'pending', count: 50, percentage: 4 },
      ];

      setMetrics(mockMetrics);
      setRevenueData(mockRevenueData);
      setOrderStatusData(mockOrderStatusData);
    } catch (err) {
      setError(t('errors.generic', 'An error occurred. Please try again.'));
      console.error('Failed to load dashboard data:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 dark:text-red-400 mb-4">
          <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
          {t('admin.dashboard.loadError', 'Failed to load dashboard data')}
        </h3>
        <p className="text-gray-600 dark:text-gray-400 mb-4">{error}</p>
        <button
          onClick={loadDashboardData}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          {t('common.retry', 'Try Again')}
        </button>
      </div>
    );
  }

  if (!metrics) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Welcome message */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
          {t('admin.dashboard.welcome', 'Welcome to Admin Dashboard')}
        </h2>
        <p className="text-gray-600 dark:text-gray-400">
          {t('admin.dashboard.welcomeMessage', 'Here\'s an overview of your autism center\'s performance.')}
        </p>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-5">
        <MetricCard
          title={t('admin.dashboard.totalRevenue', 'Total Revenue')}
          value={`${metrics.totalRevenue.toLocaleString()} ${t('currency.bhd', 'BHD')}`}
          change={metrics.revenueGrowth}
          changeType="increase"
          icon="currency"
        />
        <MetricCard
          title={t('admin.dashboard.totalOrders', 'Total Orders')}
          value={metrics.totalOrders.toLocaleString()}
          change={metrics.ordersGrowth}
          changeType="increase"
          icon="orders"
        />
        <MetricCard
          title={t('admin.dashboard.totalUsers', 'Total Users')}
          value={metrics.totalUsers.toLocaleString()}
          change={metrics.usersGrowth}
          changeType="increase"
          icon="users"
        />
        <MetricCard
          title={t('admin.dashboard.totalCourses', 'Total Courses')}
          value={metrics.totalCourses.toString()}
          change={0}
          changeType="neutral"
          icon="courses"
        />
        <MetricCard
          title={t('admin.dashboard.totalAppointments', 'Total Appointments')}
          value={metrics.totalAppointments.toLocaleString()}
          change={metrics.appointmentsGrowth}
          changeType="increase"
          icon="appointments"
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Revenue Chart */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.dashboard.revenueOverTime', 'Revenue Over Time')}
          </h3>
          <RevenueChart data={revenueData} />
        </div>

        {/* Order Status Chart */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.dashboard.orderStatus', 'Order Status Distribution')}
          </h3>
          <OrderStatusChart data={orderStatusData} />
        </div>
      </div>

      {/* Recent Activity */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">
            {t('admin.dashboard.recentActivity', 'Recent Activity')}
          </h3>
        </div>
        <RecentActivity />
      </div>
    </div>
  );
};

export default DashboardOverview;