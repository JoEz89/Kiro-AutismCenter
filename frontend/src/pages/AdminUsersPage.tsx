import React from 'react';
import { AdminLayout, UserManagement } from '@/components/admin';
import { useLocalization } from '@/hooks';

export const AdminUsersPage: React.FC = () => {
  const { t } = useLocalization();

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Page header */}
        <div className="md:flex md:items-center md:justify-between">
          <div className="min-w-0 flex-1">
            <h2 className="text-2xl font-bold leading-7 text-gray-900 dark:text-white sm:truncate sm:text-3xl sm:tracking-tight">
              {t('admin.users.title', 'User Management')}
            </h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {t('admin.users.subtitle', 'Manage user accounts, roles, and permissions.')}
            </p>
          </div>
        </div>

        {/* User management content */}
        <UserManagement />
      </div>
    </AdminLayout>
  );
};

export default AdminUsersPage;