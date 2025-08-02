import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { checkoutService } from '@/services/checkoutService';
import { SEOHead } from '@/components/seo';
import { Navigation } from '@/components/layout';
import type { Order } from '@/types';

const OrderConfirmationPage = () => {
  const { t, language } = useLocalization();
  const { orderId } = useParams<{ orderId: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchOrder = async () => {
      if (!orderId) {
        setError(t('checkout.orderNotFound', 'Order not found'));
        setLoading(false);
        return;
      }

      try {
        const orderData = await checkoutService.getOrder(orderId);
        setOrder(orderData);
      } catch (err) {
        setError(err instanceof Error ? err.message : t('checkout.orderLoadFailed', 'Failed to load order'));
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [orderId, t]);

  const formatPrice = (amount: number, currency: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const formatDate = (date: Date) => {
    return new Intl.DateTimeFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(date));
  };

  if (loading) {
    return (
      <>
        <SEOHead
          title={t('checkout.orderConfirmation', 'Order Confirmation')}
          noIndex={true}
        />
        
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
          <Navigation />
          
          <main id="main-content" className="pt-16">
            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                <p className="text-gray-600 dark:text-gray-300">
                  {t('checkout.loadingOrder', 'Loading your order...')}
                </p>
              </div>
            </div>
          </main>
        </div>
      </>
    );
  }

  if (error || !order) {
    return (
      <>
        <SEOHead
          title={t('checkout.orderConfirmation', 'Order Confirmation')}
          noIndex={true}
        />
        
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
          <Navigation />
          
          <main id="main-content" className="pt-16">
            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
              <div className="text-center">
                <svg className="mx-auto h-16 w-16 text-red-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {t('checkout.orderNotFound', 'Order Not Found')}
                </h1>
                <p className="text-gray-600 dark:text-gray-300 mb-8">
                  {error || t('checkout.orderNotFoundDescription', 'The order you are looking for does not exist or has been removed.')}
                </p>
                <Link
                  to="/products"
                  className="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 transition-colors"
                >
                  {t('checkout.continueShopping', 'Continue Shopping')}
                </Link>
              </div>
            </div>
          </main>
        </div>
      </>
    );
  }

  return (
    <>
      <SEOHead
        title={`${t('checkout.orderConfirmation', 'Order Confirmation')} - ${order.orderNumber}`}
        description={t('checkout.orderConfirmationDescription', 'Your order has been confirmed')}
        noIndex={true}
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Navigation />
        
        <main id="main-content" className="pt-16">
          <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
            {/* Success Header */}
            <div className="text-center mb-12">
              <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100 dark:bg-green-900/20 mb-6">
                <svg className="h-8 w-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              </div>
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
                {t('checkout.orderConfirmed', 'Order Confirmed!')}
              </h1>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                {t('checkout.thankYou', 'Thank you for your purchase. Your order has been confirmed and will be processed shortly.')}
              </p>
            </div>

            {/* Order Details */}
            <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden mb-8">
              <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {t('checkout.orderDetails', 'Order Details')}
                </h2>
              </div>
              
              <div className="px-6 py-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      {t('checkout.orderNumber', 'Order Number')}
                    </h3>
                    <p className="text-lg font-mono text-gray-900 dark:text-white">
                      {order.orderNumber}
                    </p>
                  </div>
                  
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      {t('checkout.orderDate', 'Order Date')}
                    </h3>
                    <p className="text-lg text-gray-900 dark:text-white">
                      {formatDate(order.createdAt)}
                    </p>
                  </div>
                  
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      {t('checkout.orderStatus', 'Status')}
                    </h3>
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      order.status === 'confirmed' 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400'
                        : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400'
                    }`}>
                      {t(`order.status.${order.status}`, order.status)}
                    </span>
                  </div>
                  
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">
                      {t('checkout.total', 'Total')}
                    </h3>
                    <p className="text-lg font-semibold text-gray-900 dark:text-white">
                      {formatPrice(order.totalAmount, order.currency)}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Order Items */}
            <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden mb-8">
              <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {t('checkout.orderItems', 'Order Items')}
                </h2>
              </div>
              
              <div className="px-6 py-4">
                <div className="space-y-4">
                  {order.items.map((item) => (
                    <div key={item.productId} className="flex items-center justify-between">
                      <div className="flex items-center space-x-4 rtl:space-x-reverse">
                        <div className="w-16 h-16 bg-gray-200 dark:bg-gray-700 rounded-lg"></div>
                        <div>
                          <p className="font-medium text-gray-900 dark:text-white">
                            {/* Product name would come from the order items */}
                            Product #{item.productId}
                          </p>
                          <p className="text-sm text-gray-600 dark:text-gray-300">
                            {t('checkout.quantity', 'Qty')}: {item.quantity}
                          </p>
                        </div>
                      </div>
                      <p className="font-medium text-gray-900 dark:text-white">
                        {formatPrice(item.price * item.quantity, order.currency)}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Shipping Address */}
            <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden mb-8">
              <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {t('checkout.shippingAddress', 'Shipping Address')}
                </h2>
              </div>
              
              <div className="px-6 py-4">
                <div className="text-gray-900 dark:text-white">
                  <p>{order.shippingAddress.street}</p>
                  <p>{order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.postalCode}</p>
                  <p>{order.shippingAddress.country}</p>
                </div>
              </div>
            </div>

            {/* Next Steps */}
            <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-6 mb-8">
              <h3 className="text-lg font-semibold text-blue-900 dark:text-blue-100 mb-4">
                {t('checkout.nextSteps', 'What happens next?')}
              </h3>
              <ul className="space-y-2 text-blue-800 dark:text-blue-200">
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  {t('checkout.step1', 'You will receive an order confirmation email shortly')}
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  {t('checkout.step2', 'Your order will be processed within 1-2 business days')}
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  {t('checkout.step3', 'You will receive tracking information once your order ships')}
                </li>
              </ul>
            </div>

            {/* Action Buttons */}
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/products"
                className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 transition-colors"
              >
                {t('checkout.continueShopping', 'Continue Shopping')}
              </Link>
              
              <Link
                to="/orders"
                className="inline-flex items-center justify-center px-6 py-3 border border-gray-300 dark:border-gray-600 text-base font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
              >
                {t('checkout.viewOrders', 'View All Orders')}
              </Link>
            </div>
          </div>
        </main>
      </div>
    </>
  );
};

export default OrderConfirmationPage;