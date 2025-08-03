import React from 'react';
import { useAuth, useLocalization } from '@/hooks';
import { LanguageSwitcher, ThemeSwitcher } from '@/components/ui';
import { cn } from '@/lib/utils';

interface AdminHeaderProps {
  onMenuClick: () => void;
  title: string;
}

export const AdminHeader: React.FC<AdminHeaderProps> = ({ onMenuClick, title }) => {
  const { user, logout } = useAuth();
  const { t, isRTL } = useLocalization();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return (
    <div className="sticky top-0 z-40 flex h-16 shrink-0 items-center gap-x-4 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 shadow-sm sm:gap-x-6 sm:px-6 lg:px-8">
      {/* Mobile menu button */}
      <button
        type="button"
        className="-m-2.5 p-2.5 text-gray-700 dark:text-gray-300 lg:hidden"
        onClick={onMenuClick}
      >
        <span className="sr-only">{t('accessibility.openMenu', 'Open sidebar')}</span>
        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
        </svg>
      </button>

      {/* Separator */}
      <div className="h-6 w-px bg-gray-200 dark:bg-gray-700 lg:hidden" />

      {/* Page title */}
      <div className="flex flex-1 gap-x-4 self-stretch lg:gap-x-6">
        <div className="flex items-center">
          <h1 className="text-xl font-semibold text-gray-900 dark:text-white">
            {title}
          </h1>
        </div>

        {/* Right side */}
        <div className={cn('flex items-center gap-x-4 lg:gap-x-6', isRTL ? 'mr-auto' : 'ml-auto')}>
          {/* Search (placeholder for future implementation) */}
          <div className="hidden lg:block lg:max-w-md lg:flex-1">
            <div className="relative">
              <div className={cn(
                'pointer-events-none absolute inset-y-0 flex items-center',
                isRTL ? 'right-0 pr-3' : 'left-0 pl-3'
              )}>
                <svg className="h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z" clipRule="evenodd" />
                </svg>
              </div>
              <input
                type="search"
                placeholder={t('common.search', 'Search...')}
                className={cn(
                  'block w-full rounded-md border-0 bg-gray-50 dark:bg-gray-700 py-1.5 text-gray-900 dark:text-white placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-blue-600 sm:text-sm sm:leading-6',
                  isRTL ? 'pr-10 pl-3' : 'pl-10 pr-3'
                )}
              />
            </div>
          </div>

          {/* Theme switcher */}
          <ThemeSwitcher />

          {/* Language switcher */}
          <LanguageSwitcher variant="dropdown" />

          {/* Separator */}
          <div className="hidden lg:block lg:h-6 lg:w-px lg:bg-gray-200 dark:lg:bg-gray-700" />

          {/* Profile dropdown */}
          <div className="relative">
            <div className="flex items-center gap-x-3">
              {/* User avatar */}
              <div className="h-8 w-8 rounded-full bg-blue-600 flex items-center justify-center">
                <span className="text-sm font-medium text-white">
                  {user?.firstName?.charAt(0)?.toUpperCase() || 'A'}
                </span>
              </div>

              {/* User info */}
              <div className="hidden lg:flex lg:items-center lg:gap-x-2">
                <span className="text-sm font-semibold text-gray-900 dark:text-white">
                  {user?.firstName} {user?.lastName}
                </span>
                <span className="text-xs text-gray-500 dark:text-gray-400">
                  {t('admin.role', 'Administrator')}
                </span>
              </div>

              {/* Logout button */}
              <button
                type="button"
                onClick={handleLogout}
                className="flex items-center gap-x-2 rounded-md bg-gray-50 dark:bg-gray-700 px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                title={t('auth.logout', 'Logout')}
              >
                <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15M12 9l-3 3m0 0l3 3m-3-3h12.75" />
                </svg>
                <span className="hidden sm:block">{t('auth.logout', 'Logout')}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminHeader;