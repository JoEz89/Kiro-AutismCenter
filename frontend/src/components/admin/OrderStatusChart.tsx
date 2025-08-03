import React from 'react';
import { useLocalization } from '@/hooks';

interface OrderStatusData {
  status: string;
  count: number;
  percentage: number;
}

interface OrderStatusChartProps {
  data: OrderStatusData[];
}

const statusColors: Record<string, string> = {
  completed: 'bg-green-500',
  processing: 'bg-blue-500',
  shipped: 'bg-yellow-500',
  pending: 'bg-gray-500',
  cancelled: 'bg-red-500',
  refunded: 'bg-purple-500',
};

export const OrderStatusChart: React.FC<OrderStatusChartProps> = ({ data }) => {
  const { t } = useLocalization();

  if (!data || data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500 dark:text-gray-400">
        {t('admin.dashboard.noData', 'No data available')}
      </div>
    );
  }

  const totalOrders = data.reduce((sum, item) => sum + item.count, 0);

  return (
    <div className="space-y-6">
      {/* Donut chart representation using progress bars */}
      <div className="space-y-4">
        {data.map((item, index) => (
          <div key={index} className="space-y-2">
            <div className="flex justify-between items-center">
              <div className="flex items-center space-x-2 rtl:space-x-reverse">
                <div className={`w-3 h-3 rounded-full ${statusColors[item.status] || 'bg-gray-500'}`} />
                <span className="text-sm font-medium text-gray-900 dark:text-white capitalize">
                  {t(`order.status.${item.status}`, item.status)}
                </span>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                {item.count} ({item.percentage}%)
              </div>
            </div>
            <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                className={`h-2 rounded-full transition-all duration-300 ${statusColors[item.status] || 'bg-gray-500'}`}
                style={{ width: `${item.percentage}%` }}
              />
            </div>
          </div>
        ))}
      </div>

      {/* Summary */}
      <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
        <div className="text-center">
          <div className="text-2xl font-bold text-gray-900 dark:text-white">
            {totalOrders.toLocaleString()}
          </div>
          <div className="text-sm text-gray-600 dark:text-gray-400">
            {t('admin.dashboard.totalOrders', 'Total Orders')}
          </div>
        </div>
      </div>

      {/* Status breakdown */}
      <div className="grid grid-cols-2 gap-4 text-sm">
        {data.map((item, index) => (
          <div key={index} className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400 capitalize">
              {t(`order.status.${item.status}`, item.status)}:
            </span>
            <span className="font-medium text-gray-900 dark:text-white">
              {item.count}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default OrderStatusChart;