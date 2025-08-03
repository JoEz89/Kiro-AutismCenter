import React, { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { useAuth, useLocalization } from '@/hooks';
import { AdminSidebar } from './AdminSidebar';
import { AdminHeader } from './AdminHeader';
import { cn } from '@/lib/utils';

interface AdminLayoutProps {
  children?: React.ReactNode;
}

export const AdminLayout: React.FC<AdminLayoutProps> = ({ children }) => {
  const { user } = useAuth();
  const { t, isRTL } = useLocalization();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Check if user has admin role
  if (!user || user.role !== 'admin') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('admin.accessDenied', 'Access Denied')}
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            {t('admin.adminAccessRequired', 'Administrator access is required to view this page.')}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('min-h-screen bg-gray-50 dark:bg-gray-900', isRTL && 'rtl')}>
      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        >
          <div className="fixed inset-0 bg-gray-600 bg-opacity-75" />
        </div>
      )}

      {/* Sidebar */}
      <AdminSidebar
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
        currentPath={location.pathname}
      />

      {/* Main content */}
      <div className={cn('lg:pl-64', isRTL && 'lg:pr-64 lg:pl-0')}>
        {/* Header */}
        <AdminHeader
          onMenuClick={() => setSidebarOpen(true)}
          title={getPageTitle(location.pathname, t)}
        />

        {/* Page content */}
        <main className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            {children || <Outlet />}
          </div>
        </main>
      </div>
    </div>
  );
};

// Helper function to get page title based on current path
function getPageTitle(pathname: string, t: (key: string, fallback?: string) => string): string {
  const pathMap: Record<string, string> = {
    '/admin': t('admin.dashboard', 'Dashboard'),
    '/admin/dashboard': t('admin.dashboard', 'Dashboard'),
    '/admin/products': t('admin.products', 'Product Management'),
    '/admin/orders': t('admin.orders', 'Order Management'),
    '/admin/users': t('admin.users', 'User Management'),
    '/admin/courses': t('admin.courses', 'Course Management'),
    '/admin/appointments': t('admin.appointments', 'Appointment Management'),
    '/admin/reports': t('admin.reports', 'Reports & Analytics'),
    '/admin/settings': t('admin.settings', 'Settings'),
  };

  return pathMap[pathname] || t('admin.dashboard', 'Dashboard');
}

export default AdminLayout;