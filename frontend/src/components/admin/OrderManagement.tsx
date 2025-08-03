import React, { useState, useEffect } from 'react';
import { useLocalization } from '@/hooks';
import { LoadingSpinner, Modal, Pagination } from '@/components/ui';
import { Order, OrderStatus, PaymentStatus } from '@/types';
import { cn } from '@/lib/utils';

interface OrderManagementProps {
  className?: string;
}

interface OrderFilters {
  status: OrderStatus | 'all';
  paymentStatus: PaymentStatus | 'all';
  dateRange: 'all' | 'today' | 'week' | 'month';
  search: string;
}

interface OrderStats {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  totalRevenue: number;
}

export const OrderManagement: React.FC<OrderManagementProps> = ({ className }) => {
  const { t, isRTL } = useLocalization();
  const [orders, setOrders] = useState<Order[]>([]);
  const [stats, setStats] = useState<OrderStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [showOrderDetails, setShowOrderDetails] = useState(false);
  const [showRefundModal, setShowRefundModal] = useState(false);
  const [refundingOrder, setRefundingOrder] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState<OrderFilters>({
    status: 'all',
    paymentStatus: 'all',
    dateRange: 'all',
    search: '',
  });

  const itemsPerPage = 10;

  useEffect(() => {
    loadOrders();
    loadStats();
  }, [currentPage, filters]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      setError(null);

      // Mock data - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 1000));

      const mockOrders: Order[] = [
        {
          id: '1',
          orderNumber: 'ORD-2024-001234',
          userId: 'user1',
          items: [
            { productId: 'prod1', quantity: 2, price: 25.00 },
            { productId: 'prod2', quantity: 1, price: 15.00 }
          ],
          totalAmount: 65.00,
          currency: 'BHD',
          status: OrderStatus.PROCESSING,
          paymentId: 'pay_123',
          paymentStatus: PaymentStatus.COMPLETED,
          shippingAddress: {
            street: '123 Main St',
            city: 'Manama',
            state: 'Capital',
            postalCode: '12345',
            country: 'Bahrain'
          },
          billingAddress: {
            street: '123 Main St',
            city: 'Manama',
            state: 'Capital',
            postalCode: '12345',
            country: 'Bahrain'
          },
          createdAt: new Date('2024-01-15T10:30:00Z'),
          updatedAt: new Date('2024-01-15T11:00:00Z'),
        },
        {
          id: '2',
          orderNumber: 'ORD-2024-001235',
          userId: 'user2',
          items: [
            { productId: 'prod3', quantity: 1, price: 45.00 }
          ],
          totalAmount: 45.00,
          currency: 'BHD',
          status: OrderStatus.SHIPPED,
          paymentId: 'pay_124',
          paymentStatus: PaymentStatus.COMPLETED,
          shippingAddress: {
            street: '456 Oak Ave',
            city: 'Riffa',
            state: 'Southern',
            postalCode: '54321',
            country: 'Bahrain'
          },
          billingAddress: {
            street: '456 Oak Ave',
            city: 'Riffa',
            state: 'Southern',
            postalCode: '54321',
            country: 'Bahrain'
          },
          createdAt: new Date('2024-01-14T14:20:00Z'),
          updatedAt: new Date('2024-01-15T09:15:00Z'),
          shippedAt: new Date('2024-01-15T09:15:00Z'),
        },
      ];

      setOrders(mockOrders);
      setTotalPages(Math.ceil(mockOrders.length / itemsPerPage));
    } catch (err) {
      setError(t('errors.generic', 'An error occurred. Please try again.'));
      console.error('Failed to load orders:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      // Mock stats - replace with actual API call
      const mockStats: OrderStats = {
        totalOrders: 1250,
        pendingOrders: 45,
        completedOrders: 1150,
        totalRevenue: 125000,
      };

      setStats(mockStats);
    } catch (err) {
      console.error('Failed to load order stats:', err);
    }
  };

  const handleStatusUpdate = async (orderId: string, newStatus: OrderStatus) => {
    try {
      // Mock API call - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 500));

      setOrders(prev => prev.map(order => 
        order.id === orderId 
          ? { ...order, status: newStatus, updatedAt: new Date() }
          : order
      ));

      if (selectedOrder?.id === orderId) {
        setSelectedOrder(prev => prev ? { ...prev, status: newStatus } : null);
      }
    } catch (err) {
      setError(t('errors.generic', 'Failed to update order status'));
    }
  };

  const handleRefund = async (orderId: string) => {
    try {
      setRefundingOrder(orderId);
      
      // Mock API call - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 2000));

      setOrders(prev => prev.map(order => 
        order.id === orderId 
          ? { 
              ...order, 
              status: OrderStatus.REFUNDED,
              paymentStatus: PaymentStatus.REFUNDED,
              updatedAt: new Date() 
            }
          : order
      ));

      setShowRefundModal(false);
      setSelectedOrder(null);
    } catch (err) {
      setError(t('errors.generic', 'Failed to process refund'));
    } finally {
      setRefundingOrder(null);
    }
  };

  const getStatusColor = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.PENDING:
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400';
      case OrderStatus.CONFIRMED:
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400';
      case OrderStatus.PROCESSING:
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900/20 dark:text-purple-400';
      case OrderStatus.SHIPPED:
        return 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900/20 dark:text-indigo-400';
      case OrderStatus.DELIVERED:
        return 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400';
      case OrderStatus.CANCELLED:
        return 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400';
      case OrderStatus.REFUNDED:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400';
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

  if (loading && !orders.length) {
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
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <div className="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h4.125m0-15.75v15.75" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.orders.totalOrders', 'Total Orders')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.totalOrders.toLocaleString()}
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
                  <svg className="h-6 w-6 text-yellow-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.orders.pendingOrders', 'Pending Orders')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.pendingOrders.toLocaleString()}
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
                      {t('admin.orders.completedOrders', 'Completed Orders')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.completedOrders.toLocaleString()}
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
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v12m-3-2.818l.879.659c1.171.879 3.07.879 4.242 0 1.172-.879 1.172-2.303 0-3.182C13.536 12.219 12.768 12 12 12c-.725 0-1.45-.22-2.003-.659-1.106-.879-1.106-2.303 0-3.182s2.9-.879 4.006 0l.415.33M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div className={cn('ml-5 w-0 flex-1', isRTL && 'mr-5 ml-0')}>
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {t('admin.orders.totalRevenue', 'Total Revenue')}
                    </dt>
                    <dd className="text-lg font-medium text-gray-900 dark:text-white">
                      {stats.totalRevenue.toLocaleString()} {t('currency.bhd', 'BHD')}
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
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.orders.search', 'Search Orders')}
            </label>
            <input
              type="text"
              value={filters.search}
              onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))}
              placeholder={t('admin.orders.searchPlaceholder', 'Search by order number or customer...')}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.orders.orderStatus', 'Order Status')}
            </label>
            <select
              value={filters.status}
              onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value as OrderStatus | 'all' }))}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="all">{t('admin.orders.allStatuses', 'All Statuses')}</option>
              <option value={OrderStatus.PENDING}>{t('order.status.pending', 'Pending')}</option>
              <option value={OrderStatus.CONFIRMED}>{t('order.status.confirmed', 'Confirmed')}</option>
              <option value={OrderStatus.PROCESSING}>{t('order.status.processing', 'Processing')}</option>
              <option value={OrderStatus.SHIPPED}>{t('order.status.shipped', 'Shipped')}</option>
              <option value={OrderStatus.DELIVERED}>{t('order.status.delivered', 'Delivered')}</option>
              <option value={OrderStatus.CANCELLED}>{t('order.status.cancelled', 'Cancelled')}</option>
              <option value={OrderStatus.REFUNDED}>{t('order.status.refunded', 'Refunded')}</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.orders.paymentStatus', 'Payment Status')}
            </label>
            <select
              value={filters.paymentStatus}
              onChange={(e) => setFilters(prev => ({ ...prev, paymentStatus: e.target.value as PaymentStatus | 'all' }))}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="all">{t('admin.orders.allPaymentStatuses', 'All Payment Statuses')}</option>
              <option value={PaymentStatus.PENDING}>{t('admin.orders.paymentPending', 'Pending')}</option>
              <option value={PaymentStatus.COMPLETED}>{t('admin.orders.paymentCompleted', 'Completed')}</option>
              <option value={PaymentStatus.FAILED}>{t('admin.orders.paymentFailed', 'Failed')}</option>
              <option value={PaymentStatus.REFUNDED}>{t('admin.orders.paymentRefunded', 'Refunded')}</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('admin.orders.dateRange', 'Date Range')}
            </label>
            <select
              value={filters.dateRange}
              onChange={(e) => setFilters(prev => ({ ...prev, dateRange: e.target.value as any }))}
              className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="all">{t('admin.orders.allDates', 'All Dates')}</option>
              <option value="today">{t('date.today', 'Today')}</option>
              <option value="week">{t('date.thisWeek', 'This Week')}</option>
              <option value="month">{t('date.thisMonth', 'This Month')}</option>
            </select>
          </div>
        </div>

        <div className="mt-4 flex justify-end">
          <button
            onClick={() => setFilters({
              status: 'all',
              paymentStatus: 'all',
              dateRange: 'all',
              search: '',
            })}
            className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            {t('common.clear', 'Clear Filters')}
          </button>
        </div>
      </div>

      {/* Orders Table */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">
            {t('admin.orders.orderList', 'Order List')}
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
                  {t('admin.orders.orderNumber', 'Order Number')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.orders.customer', 'Customer')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.orders.total', 'Total')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.orders.status', 'Status')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('admin.orders.date', 'Date')}
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                  {t('common.actions', 'Actions')}
                </th>
              </tr>
            </thead>
            <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
              {orders.map((order) => (
                <tr key={order.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                    {order.orderNumber}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                    {order.userId}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                    {order.totalAmount.toFixed(2)} {order.currency}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={cn(
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      getStatusColor(order.status)
                    )}>
                      {t(`order.status.${order.status}`, order.status)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                    {formatDate(order.createdAt)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => {
                        setSelectedOrder(order);
                        setShowOrderDetails(true);
                      }}
                      className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-3 rtl:mr-0 rtl:ml-3"
                    >
                      {t('common.view', 'View')}
                    </button>
                    {order.status !== OrderStatus.REFUNDED && order.paymentStatus === PaymentStatus.COMPLETED && (
                      <button
                        onClick={() => {
                          setSelectedOrder(order);
                          setShowRefundModal(true);
                        }}
                        className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300"
                      >
                        {t('admin.orders.refund', 'Refund')}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {orders.length === 0 && !loading && (
          <div className="text-center py-12">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h4.125m0-15.75v15.75" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900 dark:text-white">
              {t('admin.orders.noOrders', 'No orders found')}
            </h3>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {t('admin.orders.noOrdersDescription', 'Orders will appear here when customers make purchases.')}
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

      {/* Order Details Modal */}
      {showOrderDetails && selectedOrder && (
        <Modal
          isOpen={showOrderDetails}
          onClose={() => {
            setShowOrderDetails(false);
            setSelectedOrder(null);
          }}
          title={t('admin.orders.orderDetails', 'Order Details')}
          size="lg"
        >
          <div className="space-y-6">
            {/* Order Info */}
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.orderNumber', 'Order Number')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{selectedOrder.orderNumber}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.orderDate', 'Order Date')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">{formatDate(selectedOrder.createdAt)}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.status', 'Status')}
                </label>
                <div className="mt-1">
                  <select
                    value={selectedOrder.status}
                    onChange={(e) => handleStatusUpdate(selectedOrder.id, e.target.value as OrderStatus)}
                    className="block w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  >
                    <option value={OrderStatus.PENDING}>{t('order.status.pending', 'Pending')}</option>
                    <option value={OrderStatus.CONFIRMED}>{t('order.status.confirmed', 'Confirmed')}</option>
                    <option value={OrderStatus.PROCESSING}>{t('order.status.processing', 'Processing')}</option>
                    <option value={OrderStatus.SHIPPED}>{t('order.status.shipped', 'Shipped')}</option>
                    <option value={OrderStatus.DELIVERED}>{t('order.status.delivered', 'Delivered')}</option>
                    <option value={OrderStatus.CANCELLED}>{t('order.status.cancelled', 'Cancelled')}</option>
                  </select>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.total', 'Total')}
                </label>
                <p className="mt-1 text-sm text-gray-900 dark:text-white">
                  {selectedOrder.totalAmount.toFixed(2)} {selectedOrder.currency}
                </p>
              </div>
            </div>

            {/* Order Items */}
            <div>
              <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                {t('admin.orders.orderItems', 'Order Items')}
              </h4>
              <div className="border border-gray-200 dark:border-gray-600 rounded-md overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-600">
                  <thead className="bg-gray-50 dark:bg-gray-700">
                    <tr>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                        {t('admin.orders.product', 'Product')}
                      </th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                        {t('admin.orders.quantity', 'Quantity')}
                      </th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                        {t('admin.orders.price', 'Price')}
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-600">
                    {selectedOrder.items.map((item, index) => (
                      <tr key={index}>
                        <td className="px-4 py-2 text-sm text-gray-900 dark:text-white">
                          {item.productId}
                        </td>
                        <td className="px-4 py-2 text-sm text-gray-900 dark:text-white">
                          {item.quantity}
                        </td>
                        <td className="px-4 py-2 text-sm text-gray-900 dark:text-white">
                          {item.price.toFixed(2)} {selectedOrder.currency}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Addresses */}
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.orders.shippingAddress', 'Shipping Address')}
                </h4>
                <div className="text-sm text-gray-900 dark:text-white space-y-1">
                  <p>{selectedOrder.shippingAddress.street}</p>
                  <p>{selectedOrder.shippingAddress.city}, {selectedOrder.shippingAddress.state}</p>
                  <p>{selectedOrder.shippingAddress.postalCode}</p>
                  <p>{selectedOrder.shippingAddress.country}</p>
                </div>
              </div>
              <div>
                <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.orders.billingAddress', 'Billing Address')}
                </h4>
                <div className="text-sm text-gray-900 dark:text-white space-y-1">
                  <p>{selectedOrder.billingAddress.street}</p>
                  <p>{selectedOrder.billingAddress.city}, {selectedOrder.billingAddress.state}</p>
                  <p>{selectedOrder.billingAddress.postalCode}</p>
                  <p>{selectedOrder.billingAddress.country}</p>
                </div>
              </div>
            </div>
          </div>
        </Modal>
      )}

      {/* Refund Modal */}
      {showRefundModal && selectedOrder && (
        <Modal
          isOpen={showRefundModal}
          onClose={() => {
            setShowRefundModal(false);
            setSelectedOrder(null);
          }}
          title={t('admin.orders.processRefund', 'Process Refund')}
        >
          <div className="space-y-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              {t('admin.orders.refundConfirmation', 'Are you sure you want to process a refund for this order? This action cannot be undone.')}
            </p>
            
            <div className="bg-gray-50 dark:bg-gray-700 p-4 rounded-md">
              <div className="flex justify-between items-center">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.orderNumber', 'Order Number')}:
                </span>
                <span className="text-sm text-gray-900 dark:text-white">{selectedOrder.orderNumber}</span>
              </div>
              <div className="flex justify-between items-center mt-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t('admin.orders.refundAmount', 'Refund Amount')}:
                </span>
                <span className="text-sm text-gray-900 dark:text-white">
                  {selectedOrder.totalAmount.toFixed(2)} {selectedOrder.currency}
                </span>
              </div>
            </div>

            <div className="flex justify-end space-x-3 rtl:space-x-reverse">
              <button
                type="button"
                onClick={() => {
                  setShowRefundModal(false);
                  setSelectedOrder(null);
                }}
                className="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="button"
                onClick={() => handleRefund(selectedOrder.id)}
                disabled={refundingOrder === selectedOrder.id}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {refundingOrder === selectedOrder.id ? (
                  <>
                    <LoadingSpinner size="sm" className="mr-2 rtl:mr-0 rtl:ml-2" />
                    {t('admin.orders.processing', 'Processing...')}
                  </>
                ) : (
                  t('admin.orders.processRefund', 'Process Refund')
                )}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};

export default OrderManagement;