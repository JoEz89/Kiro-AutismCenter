import { Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { CartItem } from './CartItem';

interface CartPageProps {
  className?: string;
}

export const CartPage = ({ className = '' }: CartPageProps) => {
  const { t, language } = useLocalization();
  const { items, totalAmount, totalItems, currency, isLoading, error, updateCartItem, removeFromCart, clearCart } = useCart();

  const formatPrice = (amount: number, curr: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: curr,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const handleUpdateQuantity = async (productId: string, quantity: number) => {
    await updateCartItem(productId, quantity);
  };

  const handleRemoveItem = async (productId: string) => {
    await removeFromCart(productId);
  };

  const handleClearCart = async () => {
    if (window.confirm(t('cart.confirmClear', 'Are you sure you want to clear your cart?'))) {
      await clearCart();
    }
  };

  if (error) {
    return (
      <div className={`text-center py-12 ${className}`}>
        <div className="max-w-md mx-auto">
          <svg
            className="mx-auto h-16 w-16 text-red-400 mb-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            {t('errors.generic')}
          </h3>
          <p className="text-gray-600 dark:text-gray-300 mb-4">
            {error}
          </p>
          <Link
            to="/products"
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors"
          >
            {t('cart.continueShopping')}
          </Link>
        </div>
      </div>
    );
  }

  if (isLoading && items.length === 0) {
    return (
      <div className={className}>
        <div className="space-y-6">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="flex items-center gap-4 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
                <div className="w-20 h-20 bg-gray-200 dark:bg-gray-700 rounded"></div>
                <div className="flex-1">
                  <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded mb-2"></div>
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
                </div>
                <div className="w-32 h-10 bg-gray-200 dark:bg-gray-700 rounded"></div>
                <div className="w-24 h-6 bg-gray-200 dark:bg-gray-700 rounded"></div>
                <div className="w-10 h-10 bg-gray-200 dark:bg-gray-700 rounded"></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className={`text-center py-16 ${className}`}>
        <div className="max-w-md mx-auto">
          <svg
            className="mx-auto h-24 w-24 text-gray-400 mb-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"
            />
          </svg>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('cart.empty')}
          </h2>
          <p className="text-gray-600 dark:text-gray-300 mb-8">
            {t('cart.emptyDescription', 'Add some products to get started.')}
          </p>
          <Link
            to="/products"
            className="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 transition-colors"
          >
            {t('cart.continueShopping')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={className}>
      <div className="lg:grid lg:grid-cols-12 lg:gap-x-12 lg:items-start">
        {/* Cart Items */}
        <div className="lg:col-span-7">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
              {t('cart.items', 'Cart Items')} ({totalItems})
            </h2>
            
            {items.length > 0 && (
              <button
                onClick={handleClearCart}
                disabled={isLoading}
                className="text-sm text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {t('cart.clearCart', 'Clear Cart')}
              </button>
            )}
          </div>

          <div className="space-y-4">
            {items.map((item) => (
              <CartItem
                key={item.productId}
                productId={item.productId}
                product={item.product}
                quantity={item.quantity}
                price={item.price}
                onUpdateQuantity={handleUpdateQuantity}
                onRemove={handleRemoveItem}
                isUpdating={isLoading}
              />
            ))}
          </div>
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-5 mt-16 lg:mt-0">
          <div className="bg-gray-50 dark:bg-gray-800 rounded-lg px-6 py-8 sticky top-8">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-6">
              {t('cart.orderSummary', 'Order Summary')}
            </h3>

            <div className="space-y-4">
              <div className="flex justify-between text-base text-gray-900 dark:text-white">
                <p>{t('cart.subtotal')}</p>
                <p>{formatPrice(totalAmount, currency)}</p>
              </div>

              <div className="flex justify-between text-sm text-gray-600 dark:text-gray-300">
                <p>{t('cart.shipping', 'Shipping')}</p>
                <p>{t('cart.calculatedAtCheckout', 'Calculated at checkout')}</p>
              </div>

              <div className="flex justify-between text-sm text-gray-600 dark:text-gray-300">
                <p>{t('cart.taxes', 'Taxes')}</p>
                <p>{t('cart.calculatedAtCheckout', 'Calculated at checkout')}</p>
              </div>

              <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
                <div className="flex justify-between text-lg font-semibold text-gray-900 dark:text-white">
                  <p>{t('cart.total')}</p>
                  <p>{formatPrice(totalAmount, currency)}</p>
                </div>
              </div>
            </div>

            <div className="mt-8 space-y-4">
              <Link
                to="/checkout"
                className="w-full flex justify-center items-center px-6 py-3 border border-transparent rounded-md shadow-sm text-base font-medium text-white bg-blue-600 hover:bg-blue-700 transition-colors"
              >
                {t('cart.proceedToCheckout', 'Proceed to Checkout')}
              </Link>

              <Link
                to="/products"
                className="w-full flex justify-center items-center px-6 py-3 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm text-base font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors"
              >
                {t('cart.continueShopping')}
              </Link>
            </div>

            <div className="mt-6 text-center">
              <p className="text-sm text-gray-500 dark:text-gray-400">
                {t('cart.secureCheckout', 'Secure checkout powered by SSL encryption')}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};