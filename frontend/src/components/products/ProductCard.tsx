import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import type { Product } from '@/types';

interface ProductCardProps {
  product: Product;
  onAddToCart?: (productId: string) => void;
  className?: string;
}

export const ProductCard = ({ product, onAddToCart, className = '' }: ProductCardProps) => {
  const { t, language } = useLocalization();
  const [imageError, setImageError] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const productName = language === 'ar' ? product.nameAr : product.nameEn;
  const productDescription = language === 'ar' ? product.descriptionAr : product.descriptionEn;
  const primaryImage = product.imageUrls?.[0];
  const isOutOfStock = product.stockQuantity <= 0;

  const handleAddToCart = async () => {
    if (!onAddToCart || isOutOfStock) return;
    
    setIsLoading(true);
    try {
      await onAddToCart(product.id);
    } finally {
      setIsLoading(false);
    }
  };

  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div className={`bg-white dark:bg-gray-800 rounded-lg shadow-md hover:shadow-lg transition-shadow duration-300 overflow-hidden ${className}`}>
      {/* Product Image */}
      <div className="relative aspect-square overflow-hidden">
        <Link to={`/products/${product.id}`} className="block w-full h-full">
          {primaryImage && !imageError ? (
            <img
              src={primaryImage}
              alt={productName}
              className="w-full h-full object-cover hover:scale-105 transition-transform duration-300"
              onError={() => setImageError(true)}
              loading="lazy"
            />
          ) : (
            <div className="w-full h-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center">
              <svg
                className="w-16 h-16 text-gray-400"
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
        
        {/* Stock Status Badge */}
        {isOutOfStock && (
          <div className="absolute top-2 left-2 bg-red-500 text-white px-2 py-1 rounded text-sm font-medium">
            {t('products.outOfStock')}
          </div>
        )}
      </div>

      {/* Product Info */}
      <div className="p-4">
        <Link to={`/products/${product.id}`} className="block">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2 hover:text-blue-600 dark:hover:text-blue-400 transition-colors line-clamp-2">
            {productName}
          </h3>
        </Link>
        
        <p className="text-gray-600 dark:text-gray-300 text-sm mb-3 line-clamp-2">
          {productDescription}
        </p>

        {/* Price */}
        <div className="flex items-center justify-between mb-4">
          <span className="text-xl font-bold text-blue-600 dark:text-blue-400">
            {formatPrice(product.price, product.currency)}
          </span>
          
          {product.stockQuantity > 0 && product.stockQuantity <= 5 && (
            <span className="text-sm text-orange-600 dark:text-orange-400">
              {t('products.lowStock', `Only ${product.stockQuantity} left`)}
            </span>
          )}
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          <Link
            to={`/products/${product.id}`}
            className="flex-1 bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white px-4 py-2 rounded-md text-center text-sm font-medium hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
          >
            {t('products.viewDetails')}
          </Link>
          
          {onAddToCart && (
            <button
              onClick={handleAddToCart}
              disabled={isOutOfStock || isLoading}
              className={`flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                isOutOfStock
                  ? 'bg-gray-300 dark:bg-gray-600 text-gray-500 dark:text-gray-400 cursor-not-allowed'
                  : 'bg-blue-600 hover:bg-blue-700 text-white'
              }`}
              aria-label={`${t('products.addToCart')} ${productName}`}
            >
              {isLoading ? (
                <div className="flex items-center justify-center">
                  <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-current" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  {t('common.loading')}
                </div>
              ) : (
                t('products.addToCart')
              )}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};