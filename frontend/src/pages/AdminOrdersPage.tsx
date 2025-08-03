import React from 'react';
import { AdminLayout, OrderManagement } from '@/components/admin';
import { useLocalization } from '@/hooks';

export const AdminOrdersPage: React.FC = () => {
  const { t } = useLocalization();

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Page header */}
        <div className="md:flex md:items-center md:justify-between">
          <div className="min-w-0 flex-1">
            <h2 className="text-2xl font-bold leading-7 text-gray-900 dark:text-white sm:truncate sm:text-3xl sm:tracking-tight">
              {t('admin.orders.title', 'Order Management')}
            </h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {t('admin.orders.subtitle', 'Manage customer orders, process refunds, and track order status.')}
            </p>
          </div>
        </div>

        {/* Order management content */}
        <OrderManagement />
      </div>
    </AdminLayout>
  );
};

export default AdminOrdersPage;