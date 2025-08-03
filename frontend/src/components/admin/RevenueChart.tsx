import React from 'react';
import { useLocalization } from '@/hooks';

interface RevenueData {
  month: string;
  revenue: number;
  orders: number;
}

interface RevenueChartProps {
  data: RevenueData[];
}

export const RevenueChart: React.FC<RevenueChartProps> = ({ data }) => {
  const { t } = useLocalization();

  if (!data || data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500 dark:text-gray-400">
        {t('admin.dashboard.noData', 'No data available')}
      </div>
    );
  }

  // Calculate max values for scaling
  const maxRevenue = Math.max(...data.map(d => d.revenue));
  const maxOrders = Math.max(...data.map(d => d.orders));

  // Simple bar chart implementation
  return (
    <div className="space-y-4">
      {/* Legend */}
      <div className="flex items-center justify-center space-x-6 rtl:space-x-reverse">
        <div className="flex items-center">
          <div className="w-3 h-3 bg-blue-500 rounded-full mr-2 rtl:mr-0 rtl:ml-2"></div>
          <span className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.dashboard.revenue', 'Revenue')}
          </span>
        </div>
        <div className="flex items-center">
          <div className="w-3 h-3 bg-green-500 rounded-full mr-2 rtl:mr-0 rtl:ml-2"></div>
          <span className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.dashboard.orders', 'Orders')}
          </span>
        </div>
      </div>

      {/* Chart */}
      <div className="relative h-64">
        <div className="flex items-end justify-between h-full space-x-2 rtl:space-x-reverse">
          {data.map((item, index) => {
            const revenueHeight = (item.revenue / maxRevenue) * 100;
            const ordersHeight = (item.orders / maxOrders) * 100;

            return (
              <div key={index} className="flex-1 flex flex-col items-center">
                {/* Bars container */}
                <div className="flex items-end space-x-1 rtl:space-x-reverse h-48 mb-2">
                  {/* Revenue bar */}
                  <div className="relative group">
                    <div
                      className="w-6 bg-blue-500 rounded-t transition-all duration-300 hover:bg-blue-600"
                      style={{ height: `${revenueHeight}%` }}
                    />
                    {/* Tooltip */}
                    <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-2 py-1 bg-gray-900 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">
                      {item.revenue.toLocaleString()} {t('currency.bhd', 'BHD')}
                    </div>
                  </div>

                  {/* Orders bar */}
                  <div className="relative group">
                    <div
                      className="w-6 bg-green-500 rounded-t transition-all duration-300 hover:bg-green-600"
                      style={{ height: `${ordersHeight}%` }}
                    />
                    {/* Tooltip */}
                    <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-2 py-1 bg-gray-900 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">
                      {item.orders} {t('admin.dashboard.orders', 'orders')}
                    </div>
                  </div>
                </div>

                {/* Month label */}
                <span className="text-xs text-gray-600 dark:text-gray-400 font-medium">
                  {item.month}
                </span>
              </div>
            );
          })}
        </div>

        {/* Y-axis labels */}
        <div className="absolute left-0 rtl:left-auto rtl:right-0 top-0 h-48 flex flex-col justify-between text-xs text-gray-500 dark:text-gray-400 -ml-12 rtl:-ml-0 rtl:-mr-12">
          <span>{maxRevenue.toLocaleString()}</span>
          <span>{Math.round(maxRevenue * 0.75).toLocaleString()}</span>
          <span>{Math.round(maxRevenue * 0.5).toLocaleString()}</span>
          <span>{Math.round(maxRevenue * 0.25).toLocaleString()}</span>
          <span>0</span>
        </div>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-2 gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
        <div className="text-center">
          <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
            {data.reduce((sum, item) => sum + item.revenue, 0).toLocaleString()}
          </div>
          <div className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.dashboard.totalRevenue', 'Total Revenue')} ({t('currency.bhd', 'BHD')})
          </div>
        </div>
        <div className="text-center">
          <div className="text-2xl font-bold text-green-600 dark:text-green-400">
            {data.reduce((sum, item) => sum + item.orders, 0).toLocaleString()}
          </div>
          <div className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.dashboard.totalOrders', 'Total Orders')}
          </div>
        </div>
      </div>
    </div>
  );
};

export default RevenueChart;