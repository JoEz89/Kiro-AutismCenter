import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import type { Product } from '@/types';

interface CartItemProps {
  productId: string;
  product: Product;
  quantity: number;
  price: number;
  onUpdateQuantity: (productId: string, quantity: number) => void;
  onRemove: (productId: string) => void;
  isUpdating?: boolean;
  className?: string;
}

export const CartItem = ({
  productId,
  product,
  quantity,
  price,
  onUpdateQuantity,
  onRemove,
  isUpdating = false,
  className = '',
}: CartItemProps) => {
  const { t, language } = useLocalization();
  const [localQuantity, setLocalQuantity] = useState(quantity);
  const [imageError, setImageError] = useState(false);

  const productName = language === 'ar' ? product.nameAr : product.nameEn;
  const primaryImage = product.imageUrls?.[0];
  const isOutOfStock = product.stockQuantity <= 0;
  const maxQuantity = Math.min(product.stockQuantity, 10);

  const formatPrice = (amount: number, currency: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity >= 1 && newQuantity <= maxQuantity) {
      setLocalQuantity(newQuantity);
      onUpdateQuantity(productId, newQuantity);
    }
  };

  const handleRemove = () => {
    onRemove(productId);
  };

  const totalPrice = price * quantity;

  return (
    <div className={`flex items-center gap-4 p-4 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 ${className}`}>
      {/* Product Image */}
      <div className="flex-shrink-0 w-20 h-20 overflow-hidden rounded-md">
        <Link to={`/products/${productId}`}>
          {primaryImage && !imageError ? (
            <img
              src={primaryImage}
              alt={productName}
              className="w-full h-full object-cover hover:scale-105 transition-transform duration-200"
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="w-full h-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center">
              <svg
                className="w-8 h-8 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
          )}
        </Link>
      </div>

      {/* Product Details */}
      <div className="flex-1 min-w-0">
        <Link
          to={`/products/${productId}`}
          className="block text-lg font-medium text-gray-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 transition-colors truncate"
        >
          {productName}
        </Link>
        
        <div className="mt-1 flex items-center gap-4">
          <span className="text-lg font-semibold text-blue-600 dark:text-blue-400">
            {formatPrice(price, product.currency)}
          </span>
          
          {isOutOfStock && (
            <span className="text-sm text-red-600 dark:text-red-400 font-medium">
              {t('products.outOfStock')}
            </span>
          )}
        </div>
      </div>

      {/* Quantity Controls */}
      <div className="flex items-center gap-2">
        <label htmlFor={`quantity-${productId}`} className="sr-only">
          {t('cart.quantity')}
        </label>
        <div className="flex items-center border border-gray-300 dark:border-gray-600 rounded-md">
          <button
            onClick={() => handleQuantityChange(localQuantity - 1)}
            disabled={localQuantity <= 1 || isUpdating}
            className="px-3 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            aria-label={t('cart.decreaseQuantity')}
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
            </svg>
          </button>
          
          <input
            id={`quantity-${productId}`}
            type="number"
            min="1"
            max={maxQuantity}
            value={localQuantity}
            onChange={(e) => handleQuantityChange(parseInt(e.target.value) || 1)}
            disabled={isUpdating}
            className="w-16 px-3 py-2 text-center border-0 focus:ring-0 dark:bg-gray-700 dark:text-white disabled:opacity-50"
          />
          
          <button
            onClick={() => handleQuantityChange(localQuantity + 1)}
            disabled={localQuantity >= maxQuantity || isUpdating}
            className="px-3 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            aria-label={t('cart.increaseQuantity')}
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
          </button>
        </div>
      </div>

      {/* Total Price */}
      <div className="text-right min-w-0">
        <div className="text-lg font-semibold text-gray-900 dark:text-white">
          {formatPrice(totalPrice, product.currency)}
        </div>
        {quantity > 1 && (
          <div className="text-sm text-gray-500 dark:text-gray-400">
            {quantity} × {formatPrice(price, product.currency)}
          </div>
        )}
      </div>

      {/* Remove Button */}
      <button
        onClick={handleRemove}
        disabled={isUpdating}
        className="flex-shrink-0 p-2 text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        aria-label={`${t('cart.removeItem')} ${productName}`}
      >
        {isUpdating ? (
          <svg className="animate-spin w-5 h-5" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
        ) : (
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        )}
      </button>
    </div>
  );
};