import React, { useState, useEffect } from 'react';
import { useLocalization } from '@/hooks';
import { LoadingSpinner, Modal, Pagination } from '@/components/ui';
import { User, UserRole } from '@/types';
import { cn } from '@/lib/utils';

interface UserManagementProps {
  className?: string;
}

interface UserFilters {
  role: UserRole | 'all';
  status: 'all' | 'verified' | 'unverified';
  search: string;
}

interface UserStats {
  totalUsers: number;
  adminUsers: number;
  doctorUsers: number;
  regularUsers: number;
  verifiedUsers: number;
  unverifiedUsers: number;
}

export const UserManagement: React.FC<UserManagementProps> = ({ className }) => {
  const { t, isRTL } = useLocalization();
  const [users, setUsers] = useState<User[]>([]);
  const [stats, setStats] = useState<UserStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [showUserDetails, setShowUserDetails] = useState(false);
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [updatingRole, setUpdatingRole] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState<UserFilters>({
    role: 'all',
    status: 'all',
    search: '',
  });

  const itemsPerPage = 10;

  useEffect(() => {
    loadUsers();
    loadStats();
  }, [currentPage, filters]);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError(null);

      // Mock data - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 1000));

      const mockUsers: User[] = [
        {
          id: '1',
          email: 'admin@autismcenter.com',
          firstName: 'Admin',
          lastName: 'User',
          role: UserRole.ADMIN,
          preferredLanguage: 'en',
          isEmailVerified: true,
          createdAt: new Date('2024-01-01T10:00:00Z'),
        },
        {
          id: '2',
          email: 'dr.smith@autismcenter.com',
          firstName: 'Dr. Sarah',
          lastName: 'Smith',
          role: UserRole.DOCTOR,
          preferredLanguage: 'en',
          isEmailVerified: true,
          createdAt: new Date('2024-01-05T14:30:00Z'),
        },
        {
          id: '3',
          email: 'john.doe@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: UserRole.USER,
          preferredLanguage: 'en',
          isEmailVerified: true,
          createdAt: new Date('2024-01-10T09:15:00Z'),
        },
        {
          id: '4',
          email: 'ahmed.ali@example.com',
          firstName: 'Ahmed',
          lastName: 'Ali',
          role: UserRole.USER,
          preferredLanguage: 'ar',
          isEmailVerified: false,
          createdAt: new Date('2024-01-12T16:45:00Z'),
        },
        {
          id: '5',
          email: 'dr.hassan@autismcenter.com',
          firstName: 'Dr. Hassan',
          lastName: 'Al-Mahmoud',
          role: UserRole.DOCTOR,
          preferredLanguage: 'ar',
          isEmailVerified: true,
          createdAt: new Date('2024-01-08T11:20:00Z'),
        },
      ];

      setUsers(mockUsers);
      setTotalPages(Math.ceil(mockUsers.length / itemsPerPage));
    } catch (err) {
      setError(t('errors.generic', 'An error occurred. Please try again.'));
      console.error('Failed to load users:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      // Mock stats - replace with actual API call
      const mockStats: UserStats = {
        totalUsers: 3500,
        adminUsers: 5,
        doctorUsers: 12,
        regularUsers: 3483,
        verifiedUsers: 3200,
        unverifiedUsers: 300,
      };

      setStats(mockStats);
    } catch (err) {
      console.error('Failed to load user stats:', err);
    }
  };

  const handleRoleUpdate = async (userId: string, newRole: UserRole) => {
    try {
      setUpdatingRole(userId);
      
      // Mock API call - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 1000));

      setUsers(prev => prev.map(user => 
        user.id === userId 
          ? { ...user, role: newRole }
          : user
      ));

      if (selectedUser?.id === userId) {
        setSelectedUser(prev => prev ? { ...prev, role: newRole } : null);
      }

      setShowRoleModal(false);
    } catch (err) {
      setError(t('errors.generic', 'Failed to update user role'));
    } finally {
      setUpdatingRole(null);
    }
  };

  const handleEmailVerification = async (userId: string) => {
    try {
      // Mock API call - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 500));

      setUsers(prev => prev.map(user => 
        user.id === userId 
          ? { ...user, isEmailVerified: true }
          : user
      ));

      if (selectedUser?.id === userId) {
        setSelectedUser(prev => prev ? { ...prev, isEmailVerified: true } : null);
      }
    } catch (err) {
      setError(t('errors.generic', 'Failed to verify email'));
    }
  };

  const getRoleColor = (role: UserRole) => {
    switch (role) {
      case UserRole.ADMIN:
        return 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400';
      case UserRole.DOCTOR:
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400';
      case UserRole.USER:
        return 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400';
    }
  };

  const formatDate = (date: Date) => {
    return new Intl.DateTimeFormat(isRTL ? 'ar-BH' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  };

  if (loading && !users.length) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className={cn('space-y-6', className)}>
      {/* Stats Cards */}
      {stats && (
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.users.totalUsers', 'Total Users')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.totalUsers.toLocaleString()}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="h-6 w-6 text-green-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.users.verifiedUsers', 'Verified Users')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.verifiedUsers.toLocaleString()}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="h-6 w-6 text-blue-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M4.26 10.147a60.436 60.436 0 00-.491 6.347A48.627 48.627 0 0112 20.904a48.627 48.627 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.57 50.57 0 00-2.658-.813A59.905 59.905 0 0112 3.493a59.902 59.902 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.697 50.697 0 0112 13.489a50.702 50.702 0 017.74-3.342M6.75 15a.75.75 0 100-1.5.75.75 0 000 1.5zm0 0v-3.675A55.378 55.378 0 0112 8.443m-7.007 11.55A5.981 5.981 0 006.75 15.75v-1.5" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.users.doctorUsers', 'Doctors')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.doctorUsers.toLocaleString()}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.users.search', 'Search Users')}
            </label>
            <input
              type="text"
              value={filters.search}
              onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))}
              placeholder={t('admin.users.searchPlaceholder', 'Search by name or email...')}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.users.role', 'Role')}
            </label>
            <select
              value={filters.role}
              onChange={(e) => setFilters(prev => ({ ...prev, role: e.target.value as UserRole | 'all' }))}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="all">{t('admin.users.allRoles', 'All Roles')}</option>
              <option value={UserRole.ADMIN}>{t('admin.users.adminRole', 'Admin')}</option>
              <option value={UserRole.DOCTOR}>{t('admin.users.doctorRole', 'Doctor')}</option>
              <option value={UserRole.USER}>{t('admin.users.userRole', 'User')}</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.users.verificationStatus', 'Verification Status')}
            </label>
            <select
              value={filters.status}
              onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value as any }))}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="all">{t('admin.users.allStatuses', 'All Statuses')}</option>
              <option value="verified">{t('admin.users.verified', 'Verified')}</option>
              <option value="unverified">{t('admin.users.unverified', 'Unverified')}</option>
            </select>
          </div>
        </div>

        <div className="mt-4 flex justify-end">
          <button
            onClick={() => setFilters({
              role: 'all',
              status: 'all',
              search: '',
            })}
            className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            {t('common.clear', 'Clear Filters')}
          </button>
        </div>
      </div>

      {/* Users Table */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">
            {t('admin.users.userList', 'User List')}
          </h3>
        </div>

        {error && (
          <div className="p-4 bg-red-50 dark:bg-red-900/20 border-l-4 border-red-400">
            <p className="text-red-700 dark:text-red-400">{error}</p>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.users.name', 'Name')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.users.email', 'Email')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.users.role', 'Role')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.users.status', 'Status')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.users.joinDate', 'Join Date')}
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('common.actions', 'Actions')}
                </th>
              </tr>
            </thead>
            <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
              {users.map((user) => (
                <tr key={user.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="flex-shrink-0 h-10 w-10">
                        <div className="h-10 w-10 rounded-full bg-gray-300 dark:bg-gray-600 flex items-center justify-center">
                          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                            {user.firstName.charAt(0)}{user.lastName.charAt(0)}
                          </span>
                        </div>
                      </div>
                      <div className={cn('ml-4', isRTL && 'mr-4 ml-0')}>
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {user.firstName} {user.lastName}
                        </div>
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          {t(`language.${user.preferredLanguage}`, user.preferredLanguage)}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                    {user.email}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={cn(
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      getRoleColor(user.role)
                    )}>
                      {t(`admin.users.${user.role}Role`, user.role)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      {user.isEmailVerified ? (
                        <>
                          <svg className="h-4 w-4 text-green-400 mr-2 rtl:mr-0 rtl:ml-2" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                          <span className="text-sm text-green-600 dark:text-green-400">
                            {t('admin.users.verified', 'Verified')}
                          </span>
                        </>
                      ) : (
                        <>
                          <svg className="h-4 w-4 text-yellow-400 mr-2 rtl:mr-0 rtl:ml-2" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
                          </svg>
                          <span className="text-sm text-yellow-600 dark:text-yellow-400">
                            {t('admin.users.unverified', 'Unverified')}
                          </span>
                        </>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                    {formatDate(user.createdAt)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => {
                        setSelectedUser(user);
                        setShowUserDetails(true);
                      }}
                      className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-3 rtl:mr-0 rtl:ml-3"
                    >
                      {t('common.view', 'View')}
                    </button>
                    <button
                      onClick={() => {
                        setSelectedUser(user);
                        setShowRoleModal(true);
                      }}
                      className="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-3 rtl:mr-0 rtl:ml-3"
                    >
                      {t('admin.users.changeRole', 'Change Role')}
                    </button>
                    {!user.isEmailVerified && (
                      <button
                        onClick={() => handleEmailVerification(user.id)}
                        className="text-green-600 hover:text-green-900 dark:text-green-400 dark:hover:text-green-300"
                      >
                        {t('admin.users.verify', 'Verify')}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {users.length === 0 && !loading && (
          <div className="text-center py-12">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900 dark:text-white">
              {t('admin.users.noUsers', 'No users found')}
            </h3>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {t('admin.users.noUsersDescription', 'Users will appear here when they register.')}
            </p>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-700">
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={setCurrentPage}
            />
          </div>
        )}
      </div>

      {/* User Details Modal */}
      {showUserDetails && selectedUser && (
        <Modal
          isOpen={showUserDetails}
          onClose={() => {
            setShowUserDetails(false);
            setSelectedUser(null);
          }}
          title={t('admin.users.userDetails', 'User Details')}
          size="lg"
        >
          <div className="space-y-6">
            {/* User Info */}
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.firstName', 'First Name')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{selectedUser.firstName}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.lastName', 'Last Name')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{selectedUser.lastName}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.email', 'Email')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{selectedUser.email}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.role', 'Role')}
                </label>
                <p className="mt-1">
                  <span className={cn(
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                    getRoleColor(selectedUser.role)
                  )}>
                    {t(`admin.users.${selectedUser.role}Role`, selectedUser.role)}
                  </span>
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.preferredLanguage', 'Preferred Language')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">
                  {t(`language.${selectedUser.preferredLanguage}`, selectedUser.preferredLanguage)}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.emailVerification', 'Email Verification')}
                </label>
                <p className="mt-1">
                  {selectedUser.isEmailVerified ? (
                    <span className="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400">
                      <svg className="h-3 w-3 mr-1 rtl:mr-0 rtl:ml-1" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      {t('admin.users.verified', 'Verified')}
                    </span>
                  ) : (
                    <span className="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400">
                      <svg className="h-3 w-3 mr-1 rtl:mr-0 rtl:ml-1" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
                      </svg>
                      {t('admin.users.unverified', 'Unverified')}
                    </span>
                  )}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.joinDate', 'Join Date')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{formatDate(selectedUser.createdAt)}</p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex justify-end space-x-3 rtl:space-x-reverse pt-4 border-t border-gray-200 dark:border-gray-700">
              {!selectedUser.isEmailVerified && (
                <button
                  onClick={() => handleEmailVerification(selectedUser.id)}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                >
                  {t('admin.users.verifyEmail', 'Verify Email')}
                </button>
              )}
              <button
                onClick={() => {
                  setShowUserDetails(false);
                  setShowRoleModal(true);
                }}
                className="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                {t('admin.users.changeRole', 'Change Role')}
              </button>
            </div>
          </div>
        </Modal>
      )}

      {/* Role Change Modal */}
      {showRoleModal && selectedUser && (
        <Modal
          isOpen={showRoleModal}
          onClose={() => {
            setShowRoleModal(false);
            setSelectedUser(null);
          }}
          title={t('admin.users.changeUserRole', 'Change User Role')}
        >
          <div className="space-y-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              {t('admin.users.roleChangeConfirmation', 'Select a new role for this user. This will change their access permissions.')}
            </p>
            
            <div className="bg-gray-50 dark:bg-gray-700 p-4 rounded-md">
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.user', 'User')}:
                </span>
                <span className="text-sm text-gray-900 dark:text-white">
                  {selectedUser.firstName} {selectedUser.lastName}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.users.currentRole', 'Current Role')}:
                </span>
                <span className={cn(
                  'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                  getRoleColor(selectedUser.role)
                )}>
                  {t(`admin.users.${selectedUser.role}Role`, selectedUser.role)}
                </span>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('admin.users.newRole', 'New Role')}
              </label>
              <div className="space-y-2">
                {Object.values(UserRole).map((role) => (
                  <label key={role} className="flex items-center">
                    <input
                      type="radio"
                      name="role"
                      value={role}
                      defaultChecked={role === selectedUser.role}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 dark:border-gray-600"
                    />
                    <span className={cn('ml-2 text-sm', isRTL && 'mr-2 ml-0')}>
                      <span className={cn(
                        'inline-flex px-2 py-1 text-xs font-semibold rounded-full mr-2 rtl:mr-0 rtl:ml-2',
                        getRoleColor(role)
                      )}>
                        {t(`admin.users.${role}Role`, role)}
                      </span>
                      {role === UserRole.ADMIN && t('admin.users.adminDescription', 'Full system access')}
                      {role === UserRole.DOCTOR && t('admin.users.doctorDescription', 'Can manage appointments and courses')}
                      {role === UserRole.USER && t('admin.users.userDescription', 'Standard user access')}
                    </span>
                  </label>
                ))}
              </div>
            </div>

            <div className="flex justify-end space-x-3 rtl:space-x-reverse">
              <button
                type="button"
                onClick={() => {
                  setShowRoleModal(false);
                  setSelectedUser(null);
                }}
                className="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="button"
                onClick={(e) => {
                  const form = e.currentTarget.closest('form') || e.currentTarget.closest('.space-y-4');
                  const selectedRole = (form?.querySelector('input[name="role"]:checked') as HTMLInputElement)?.value as UserRole;
                  if (selectedRole && selectedRole !== selectedUser.role) {
                    handleRoleUpdate(selectedUser.id, selectedRole);
                  }
                }}
                disabled={updatingRole === selectedUser.id}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {updatingRole === selectedUser.id ? (
                  <>
                    <LoadingSpinner size="sm" className="mr-2 rtl:mr-0 rtl:ml-2" />
                    {t('admin.users.updating', 'Updating...')}
                  </>
                ) : (
                  t('admin.users.updateRole', 'Update Role')
                )}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};

export default UserManagement;