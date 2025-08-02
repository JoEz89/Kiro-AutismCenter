import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { useProductSearch } from '@/hooks/useProducts';
import type { Product } from '@/types';

interface ProductSearchProps {
  onSearchSubmit?: (query: string) => void;
  className?: string;
}

export const ProductSearch = ({ onSearchSubmit, className = '' }: ProductSearchProps) => {
  const { t, language } = useLocalization();
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const { results, loading, search, clearResults } = useProductSearch();
  const searchRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (query.trim()) {
        search(query);
        setIsOpen(true);
      } else {
        clearResults();
        setIsOpen(false);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [query, search, clearResults]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim() && onSearchSubmit) {
      onSearchSubmit(query.trim());
      setIsOpen(false);
      inputRef.current?.blur();
    }
  };

  const handleResultClick = () => {
    setIsOpen(false);
    setQuery('');
    clearResults();
  };

  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div ref={searchRef} className={`relative ${className}`}>
      <form onSubmit={handleSubmit} className="relative">
        <div className="relative">
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={t('products.searchPlaceholder', 'Search products...')}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            aria-label={t('products.searchLabel', 'Search products')}
          />
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg
              className="h-5 w-5 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
          </div>
          {loading && (
            <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
              <svg
                className="animate-spin h-5 w-5 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
            </div>
          )}
        </div>
      </form>

      {/* Search Results Dropdown */}
      {isOpen && (query.trim() || results.length > 0) && (
        <div className="absolute z-50 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg max-h-96 overflow-y-auto">
          {results.length > 0 ? (
            <>
              <div className="px-4 py-2 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide border-b border-gray-200 dark:border-gray-700">
                {t('products.searchResults', 'Search Results')}
              </div>
              {results.map((product) => (
                <Link
                  key={product.id}
                  to={`/products/${product.id}`}
                  onClick={handleResultClick}
                  className="flex items-center px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700 last:border-b-0"
                >
                  <div className="flex-shrink-0 w-12 h-12 mr-3">
                    {product.imageUrls?.[0] ? (
                      <img
                        src={product.imageUrls[0]}
                        alt={language === 'ar' ? product.nameAr : product.nameEn}
                        className="w-full h-full object-cover rounded"
                      />
                    ) : (
                      <div className="w-full h-full bg-gray-200 dark:bg-gray-600 rounded flex items-center justify-center">
                        <svg className="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                      </div>
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                      {language === 'ar' ? product.nameAr : product.nameEn}
                    </p>
                    <p className="text-sm text-blue-600 dark:text-blue-400 font-medium">
                      {formatPrice(product.price, product.currency)}
                    </p>
                    {product.stockQuantity <= 0 && (
                      <p className="text-xs text-red-500">
                        {t('products.outOfStock')}
                      </p>
                    )}
                  </div>
                </Link>
              ))}
              {onSearchSubmit && (
                <button
                  onClick={() => {
                    onSearchSubmit(query.trim());
                    setIsOpen(false);
                  }}
                  className="w-full px-4 py-3 text-left text-sm text-blue-600 dark:text-blue-400 hover:bg-gray-50 dark:hover:bg-gray-700 border-t border-gray-200 dark:border-gray-700"
                >
                  {t('products.viewAllResults', 'View all results for "{{query}}"', { query })}
                </button>
              )}
            </>
          ) : query.trim() && !loading ? (
            <div className="px-4 py-8 text-center">
              <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                {t('products.noSearchResults', 'No products found for "{{query}}"', { query })}
              </p>
              {onSearchSubmit && (
                <button
                  onClick={() => {
                    onSearchSubmit(query.trim());
                    setIsOpen(false);
                  }}
                  className="mt-2 text-sm text-blue-600 dark:text-blue-400 hover:underline"
                >
                  {t('products.searchAnyway', 'Search anyway')}
                </button>
              )}
            </div>
          ) : null}
        </div>
      )}
    </div>
  );
};