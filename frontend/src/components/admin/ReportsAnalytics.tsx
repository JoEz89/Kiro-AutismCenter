import React, { useState, useEffect } from 'react';
import { useLocalization } from '@/hooks';
import { LoadingSpinner } from '@/components/ui';
import { cn } from '@/lib/utils';

interface ReportsAnalyticsProps {
  className?: string;
}

interface AnalyticsData {
  revenue: {
    total: number;
    growth: number;
    monthlyData: Array<{ month: string; amount: number }>;
  };
  orders: {
    total: number;
    growth: number;
    statusBreakdown: Array<{ status: string; count: number; percentage: number }>;
  };
  users: {
    total: number;
    growth: number;
    registrationData: Array<{ month: string; count: number }>;
  };
  courses: {
    totalEnrollments: number;
    completionRate: number;
    popularCourses: Array<{ name: string; enrollments: number }>;
  };
  appointments: {
    total: number;
    completionRate: number;
    monthlyData: Array<{ month: string; count: number }>;
  };
}

export const ReportsAnalytics: React.FC<ReportsAnalyticsProps> = ({ className }) => {
  const { t, isRTL } = useLocalization();
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedPeriod, setSelectedPeriod] = useState<'week' | 'month' | 'quarter' | 'year'>('month');
  const [exportingData, setExportingData] = useState(false);

  useEffect(() => {
    loadAnalyticsData();
  }, [selectedPeriod]);

  const loadAnalyticsData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Mock data - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 1500));

      const mockData: AnalyticsData = {
        revenue: {
          total: 125000,
          growth: 12.5,
          monthlyData: [
            { month: 'Jan', amount: 8500 },
            { month: 'Feb', amount: 9200 },
            { month: 'Mar', amount: 10100 },
            { month: 'Apr', amount: 11500 },
            { month: 'May', amount: 12800 },
            { month: 'Jun', amount: 14200 },
          ],
        },
        orders: {
          total: 1250,
          growth: 8.3,
          statusBreakdown: [
            { status: 'completed', count: 850, percentage: 68 },
            { status: 'processing', count: 200, percentage: 16 },
            { status: 'shipped', count: 150, percentage: 12 },
            { status: 'pending', count: 50, percentage: 4 },
          ],
        },
        users: {
          total: 3500,
          growth: 15.2,
          registrationData: [
            { month: 'Jan', count: 120 },
            { month: 'Feb', count: 145 },
            { month: 'Mar', count: 180 },
            { month: 'Apr', count: 210 },
            { month: 'May', count: 250 },
            { month: 'Jun', count: 290 },
          ],
        },
        courses: {
          totalEnrollments: 2800,
          completionRate: 72,
          popularCourses: [
            { name: 'Understanding Autism Basics', enrollments: 450 },
            { name: 'Communication Strategies', enrollments: 380 },
            { name: 'Behavioral Support', enrollments: 320 },
            { name: 'Family Support Guide', enrollments: 280 },
            { name: 'Educational Planning', enrollments: 240 },
          ],
        },
        appointments: {
          total: 450,
          completionRate: 85,
          monthlyData: [
            { month: 'Jan', count: 65 },
            { month: 'Feb', count: 72 },
            { month: 'Mar', count: 78 },
            { month: 'Apr', count: 85 },
            { month: 'May', count: 92 },
            { month: 'Jun', count: 98 },
          ],
        },
      };

      setData(mockData);
    } catch (err) {
      setError(t('errors.generic', 'An error occurred. Please try again.'));
      console.error('Failed to load analytics data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleExportData = async (format: 'csv' | 'pdf') => {
    try {
      setExportingData(true);
      
      // Mock export - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // Create mock download
      const filename = `autism-center-report-${new Date().toISOString().split('T')[0]}.${format}`;
      const mockData = format === 'csv' 
        ? 'Date,Revenue,Orders,Users,Appointments\n2024-01-01,8500,85,120,65\n2024-02-01,9200,92,145,72'
        : 'Mock PDF content';
      
      const blob = new Blob([mockData], { 
        type: format === 'csv' ? 'text/csv' : 'application/pdf' 
      });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(t('errors.generic', 'Failed to export data'));
    } finally {
      setExportingData(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return `${amount.toLocaleString()} ${t('currency.bhd', 'BHD')}`;
  };

  const formatPercentage = (value: number) => {
    return `${value > 0 ? '+' : ''}${value.toFixed(1)}%`;
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
          {t('admin.reports.loadError', 'Failed to load analytics data')}
        </h3>
        <p className="text-gray-600 dark:text-gray-400 mb-4">{error}</p>
        <button
          onClick={loadAnalyticsData}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          {t('common.retry', 'Try Again')}
        </button>
      </div>
    );
  }

  if (!data) {
    return null;
  }

  return (
    <div className={cn('space-y-6', className)}>
      {/* Header with Controls */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t('admin.reports.title', 'Reports & Analytics')}
          </h2>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            {t('admin.reports.subtitle', 'Comprehensive insights into your autism center\'s performance')}
          </p>
        </div>
        
        <div className="mt-4 sm:mt-0 flex flex-col sm:flex-row gap-3">
          {/* Period Selector */}
          <select
            value={selectedPeriod}
            onChange={(e) => setSelectedPeriod(e.target.value as any)}
            className="block w-full sm:w-auto rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
          >
            <option value="week">{t('admin.reports.thisWeek', 'This Week')}</option>
            <option value="month">{t('admin.reports.thisMonth', 'This Month')}</option>
            <option value="quarter">{t('admin.reports.thisQuarter', 'This Quarter')}</option>
            <option value="year">{t('admin.reports.thisYear', 'This Year')}</option>
          </select>

          {/* Export Buttons */}
          <div className="flex gap-2">
            <button
              onClick={() => handleExportData('csv')}
              disabled={exportingData}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
            >
              {exportingData ? (
                <LoadingSpinner size="sm" className="mr-2 rtl:mr-0 rtl:ml-2" />
              ) : (
                <svg className="h-4 w-4 mr-2 rtl:mr-0 rtl:ml-2" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
                </svg>
              )}
              {t('admin.reports.exportCSV', 'Export CSV')}
            </button>
            <button
              onClick={() => handleExportData('pdf')}
              disabled={exportingData}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
            >
              {exportingData ? (
                <LoadingSpinner size="sm" className="mr-2 rtl:mr-0 rtl:ml-2" />
              ) : (
                <svg className="h-4 w-4 mr-2 rtl:mr-0 rtl:ml-2" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z" />
                </svg>
              )}
              {t('admin.reports.exportPDF', 'Export PDF')}
            </button>
          </div>
        </div>
      </div>

      {/* Key Metrics Overview */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {/* Revenue */}
        <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-green-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v12m-3-2.818l.879.659c1.171.879 3.07.879 4.242 0 1.172-.879 1.172-2.303 0-3.182C13.536 12.219 12.768 12 12 12c-.725 0-1.45-.22-2.003-.659-1.106-.879-1.106-2.303 0-3.182s2.9-.879 4.006 0l.415.33M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                <dl>
                  <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                    {t('admin.reports.totalRevenue', 'Total Revenue')}
                  </dt>
                  <dd className="flex items-baseline">
                    <div className="text-2xl font-semibold text-gray-900 dark:text-white">
                      {formatCurrency(data.revenue.total)}
                    </div>
                    <div className={cn(
                      'ml-2 flex items-baseline text-sm font-semibold',
                      isRTL && 'mr-2 ml-0',
                      data.revenue.growth >= 0 ? 'text-green-600' : 'text-red-600'
                    )}>
                      {formatPercentage(data.revenue.growth)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Orders */}
        <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-blue-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h4.125m0-15.75v15.75" />
                </svg>
              </div>
              <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                <dl>
                  <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                    {t('admin.reports.totalOrders', 'Total Orders')}
                  </dt>
                  <dd className="flex items-baseline">
                    <div className="text-2xl font-semibold text-gray-900 dark:text-white">
                      {data.orders.total.toLocaleString()}
                    </div>
                    <div className={cn(
                      'ml-2 flex items-baseline text-sm font-semibold',
                      isRTL && 'mr-2 ml-0',
                      data.orders.growth >= 0 ? 'text-green-600' : 'text-red-600'
                    )}>
                      {formatPercentage(data.orders.growth)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Users */}
        <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-purple-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" />
                </svg>
              </div>
              <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                <dl>
                  <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                    {t('admin.reports.totalUsers', 'Total Users')}
                  </dt>
                  <dd className="flex items-baseline">
                    <div className="text-2xl font-semibold text-gray-900 dark:text-white">
                      {data.users.total.toLocaleString()}
                    </div>
                    <div className={cn(
                      'ml-2 flex items-baseline text-sm font-semibold',
                      isRTL && 'mr-2 ml-0',
                      data.users.growth >= 0 ? 'text-green-600' : 'text-red-600'
                    )}>
                      {formatPercentage(data.users.growth)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Appointments */}
        <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-orange-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5a2.25 2.25 0 002.25-2.25m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5a2.25 2.25 0 012.25 2.25v7.5M9 12.75h6m-6 3h6" />
                </svg>
              </div>
              <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                <dl>
                  <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                    {t('admin.reports.totalAppointments', 'Total Appointments')}
                  </dt>
                  <dd className="flex items-baseline">
                    <div className="text-2xl font-semibold text-gray-900 dark:text-white">
                      {data.appointments.total.toLocaleString()}
                    </div>
                    <div className="ml-2 text-sm text-gray-500 dark:text-gray-400">
                      {data.appointments.completionRate}% {t('admin.reports.completed', 'completed')}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Charts and Detailed Analytics */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Revenue Chart */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.reports.revenueOverTime', 'Revenue Over Time')}
          </h3>
          <div className="h-64 flex items-center justify-center text-gray-500 dark:text-gray-400">
            {/* Placeholder for chart - replace with actual chart component */}
            <div className="text-center">
              <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 013 19.875v-6.75zM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V8.625zM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V4.125z" />
              </svg>
              <p>{t('admin.reports.chartPlaceholder', 'Chart visualization would appear here')}</p>
            </div>
          </div>
        </div>

        {/* Order Status Breakdown */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.reports.orderStatusBreakdown', 'Order Status Breakdown')}
          </h3>
          <div className="space-y-3">
            {data.orders.statusBreakdown.map((item) => (
              <div key={item.status} className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className={cn(
                    'w-3 h-3 rounded-full mr-3 rtl:mr-0 rtl:ml-3',
                    item.status === 'completed' && 'bg-green-400',
                    item.status === 'processing' && 'bg-blue-400',
                    item.status === 'shipped' && 'bg-purple-400',
                    item.status === 'pending' && 'bg-yellow-400'
                  )} />
                  <span className="text-sm text-gray-700 dark:text-gray-300 capitalize">
                    {t(`order.status.${item.status}`, item.status)}
                  </span>
                </div>
                <div className="flex items-center space-x-2 rtl:space-x-reverse">
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    {item.count}
                  </span>
                  <span className="text-sm text-gray-500 dark:text-gray-400">
                    ({item.percentage}%)
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Popular Courses */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.reports.popularCourses', 'Popular Courses')}
          </h3>
          <div className="space-y-3">
            {data.courses.popularCourses.map((course, index) => (
              <div key={course.name} className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className="flex-shrink-0 w-6 h-6 bg-blue-100 dark:bg-blue-900/20 rounded-full flex items-center justify-center">
                    <span className="text-xs font-medium text-blue-600 dark:text-blue-400">
                      {index + 1}
                    </span>
                  </div>
                  <span className={cn('text-sm text-gray-700 dark:text-gray-300 ml-3', isRTL && 'mr-3 ml-0')}>
                    {course.name}
                  </span>
                </div>
                <span className="text-sm font-medium text-gray-900 dark:text-white">
                  {course.enrollments} {t('admin.reports.enrollments', 'enrollments')}
                </span>
              </div>
            ))}
          </div>
          <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {t('admin.reports.completionRate', 'Completion Rate')}
              </span>
              <span className="text-sm font-medium text-gray-900 dark:text-white">
                {data.courses.completionRate}%
              </span>
            </div>
          </div>
        </div>

        {/* User Registration Trend */}
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {t('admin.reports.userRegistrationTrend', 'User Registration Trend')}
          </h3>
          <div className="h-48 flex items-center justify-center text-gray-500 dark:text-gray-400">
            {/* Placeholder for chart - replace with actual chart component */}
            <div className="text-center">
              <svg className="mx-auto h-8 w-8 mb-2" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 18L9 11.25l4.306 4.307a11.95 11.95 0 015.814-5.519l2.74-1.22m0 0l-5.94-2.28m5.94 2.28l-2.28 5.941" />
              </svg>
              <p className="text-sm">{t('admin.reports.trendChart', 'Trend chart visualization')}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Summary Report */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
          {t('admin.reports.summaryReport', 'Summary Report')}
        </h3>
        <div className="prose dark:prose-invert max-w-none">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.reports.summaryText', 'Based on the current period data, your autism center is showing strong performance across all key metrics. Revenue has grown by {{revenueGrowth}}% with {{totalOrders}} orders processed. User engagement remains high with {{totalUsers}} registered users and a {{completionRate}}% course completion rate.', {
              revenueGrowth: data.revenue.growth.toFixed(1),
              totalOrders: data.orders.total.toLocaleString(),
              totalUsers: data.users.total.toLocaleString(),
              completionRate: data.courses.completionRate
            })}
          </p>
        </div>
      </div>
    </div>
  );
};

export default ReportsAnalytics;