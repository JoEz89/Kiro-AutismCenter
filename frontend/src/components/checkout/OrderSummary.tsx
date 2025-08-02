import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import type { ShippingOption } from '@/services/checkoutService';

interface OrderSummaryProps {
  shippingOption?: ShippingOption;
  taxAmount?: number;
  className?: string;
}

export const OrderSummary = ({ shippingOption, taxAmount = 0, className = '' }: OrderSummaryProps) => {
  const { t, language } = useLocalization();
  const { items, totalAmount, currency } = useCart();

  const formatPrice = (amount: number, curr: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: curr,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const shippingCost = shippingOption?.price || 0;
  const finalTotal = totalAmount + shippingCost + taxAmount;

  return (
    <div className={`bg-gray-50 dark:bg-gray-800 rounded-lg p-6 ${className}`}>
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-6">
        {t('checkout.orderSummary', 'Order Summary')}
      </h3>

      {/* Order Items */}
      <div className="space-y-4 mb-6">
        {items.map((item) => {
          const productName = language === 'ar' ? item.product.nameAr : item.product.nameEn;
          const itemTotal = item.price * item.quantity;

          return (
            <div key={item.productId} className="flex items-center justify-between">
              <div className="flex items-center space-x-3 rtl:space-x-reverse">
                {item.product.imageUrls?.[0] && (
                  <img
                    src={item.product.imageUrls[0]}
                    alt={productName}
                    className="w-12 h-12 object-cover rounded"
                  />
                )}
                <div>
                  <p className="text-sm font-medium text-gray-900 dark:text-white">
                    {productName}
                  </p>
                  <p className="text-sm text-gray-600 dark:text-gray-300">
                    {t('checkout.quantity', 'Qty')}: {item.quantity}
                  </p>
                </div>
              </div>
              <p className="text-sm font-medium text-gray-900 dark:text-white">
                {formatPrice(itemTotal, currency)}
              </p>
            </div>
          );
        })}
      </div>

      {/* Order Totals */}
      <div className="space-y-3 border-t border-gray-200 dark:border-gray-700 pt-4">
        <div className="flex justify-between text-sm">
          <p className="text-gray-600 dark:text-gray-300">
            {t('checkout.subtotal', 'Subtotal')}
          </p>
          <p className="text-gray-900 dark:text-white">
            {formatPrice(totalAmount, currency)}
          </p>
        </div>

        {shippingOption && (
          <div className="flex justify-between text-sm">
            <div>
              <p className="text-gray-600 dark:text-gray-300">
                {t('checkout.shipping', 'Shipping')}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400">
                {shippingOption.name} ({shippingOption.estimatedDays} {t('checkout.days', 'days')})
              </p>
            </div>
            <p className="text-gray-900 dark:text-white">
              {shippingCost === 0 ? t('checkout.free', 'Free') : formatPrice(shippingCost, currency)}
            </p>
          </div>
        )}

        {taxAmount > 0 && (
          <div className="flex justify-between text-sm">
            <p className="text-gray-600 dark:text-gray-300">
              {t('checkout.tax', 'Tax')}
            </p>
            <p className="text-gray-900 dark:text-white">
              {formatPrice(taxAmount, currency)}
            </p>
          </div>
        )}

        <div className="flex justify-between text-lg font-semibold border-t border-gray-200 dark:border-gray-700 pt-3">
          <p className="text-gray-900 dark:text-white">
            {t('checkout.total', 'Total')}
          </p>
          <p className="text-gray-900 dark:text-white">
            {formatPrice(finalTotal, currency)}
          </p>
        </div>
      </div>

      {/* Estimated Delivery */}
      {shippingOption && (
        <div className="mt-6 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
          <div className="flex items-center text-sm">
            <svg className="w-4 h-4 text-blue-600 dark:text-blue-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3a1 1 0 011-1h6a1 1 0 011 1v4h3a1 1 0 011 1v8a1 1 0 01-1 1H5a1 1 0 01-1-1V8a1 1 0 011-1h3z" />
            </svg>
            <span className="text-blue-900 dark:text-blue-100">
              {t('checkout.estimatedDelivery', 'Estimated delivery')}: {shippingOption.estimatedDays} {t('checkout.businessDays', 'business days')}
            </span>
          </div>
        </div>
      )}

      {/* Security Badge */}
      <div className="mt-6 flex items-center justify-center text-xs text-gray-500 dark:text-gray-400">
        <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
        </svg>
        {t('checkout.secureCheckout', 'Secure 256-bit SSL encryption')}
      </div>
    </div>
  );
};