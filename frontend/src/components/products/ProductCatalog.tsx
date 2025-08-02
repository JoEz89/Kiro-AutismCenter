import { useState } from 'react';
import { useProducts } from '@/hooks/useProducts';
import { useLocalization } from '@/hooks';
import { ProductGrid } from './ProductGrid';
import { ProductFilters } from './ProductFilters';
import { ProductSort } from './ProductSort';
import { ProductSearch } from './ProductSearch';
import { Pagination } from './Pagination';
import type { ProductFilters as IProductFilters, ProductSortOptions } from '@/services/productService';

interface ProductCatalogProps {
  onAddToCart?: (productId: string) => void;
  className?: string;
}

export const ProductCatalog = ({ onAddToCart, className = '' }: ProductCatalogProps) => {
  const { t } = useLocalization();
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  
  const {
    products,
    totalCount,
    totalPages,
    currentPage,
    pageSize,
    loading,
    error,
    filters,
    sort,
    updateFilters,
    updateSort,
    goToPage,
    refresh,
  } = useProducts();

  const handleSearchSubmit = (query: string) => {
    updateFilters({ ...filters, search: query });
  };

  const handleFiltersChange = (newFilters: IProductFilters) => {
    updateFilters(newFilters);
  };

  const handleSortChange = (newSort: ProductSortOptions) => {
    updateSort(newSort);
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
          <button
            onClick={refresh}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors"
          >
            {t('common.retry', 'Try Again')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={className}>
      <div className="lg:grid lg:grid-cols-4 lg:gap-8">
        {/* Filters Sidebar */}
        <div className="lg:col-span-1">
          <ProductFilters
            filters={filters || {}}
            onFiltersChange={handleFiltersChange}
          />
        </div>

        {/* Main Content */}
        <div className="lg:col-span-3">
          {/* Search and Controls */}
          <div className="mb-6">
            <div className="flex flex-col sm:flex-row gap-4 mb-4">
              <div className="flex-1">
                <ProductSearch onSearchSubmit={handleSearchSubmit} />
              </div>
            </div>

            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
              {/* Results Count */}
              <div className="text-sm text-gray-600 dark:text-gray-300">
                {loading ? (
                  <div className="h-5 w-32 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                ) : (
                  <span>
                    {t('products.showingResults', 'Showing {{count}} products', { 
                      count: totalCount.toLocaleString() 
                    })}
                  </span>
                )}
              </div>

              <div className="flex items-center gap-4">
                {/* View Mode Toggle */}
                <div className="flex items-center border border-gray-300 dark:border-gray-600 rounded-md">
                  <button
                    onClick={() => setViewMode('grid')}
                    className={`p-2 ${
                      viewMode === 'grid'
                        ? 'bg-blue-600 text-white'
                        : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                    }`}
                    aria-label={t('products.gridView', 'Grid view')}
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
                    </svg>
                  </button>
                  <button
                    onClick={() => setViewMode('list')}
                    className={`p-2 ${
                      viewMode === 'list'
                        ? 'bg-blue-600 text-white'
                        : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                    }`}
                    aria-label={t('products.listView', 'List view')}
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
                    </svg>
                  </button>
                </div>

                {/* Sort */}
                <ProductSort
                  sort={sort}
                  onSortChange={handleSortChange}
                />
              </div>
            </div>
          </div>

          {/* Products Grid */}
          <ProductGrid
            products={products}
            loading={loading}
            onAddToCart={onAddToCart}
            className="mb-8"
          />

          {/* Pagination */}
          {!loading && totalPages > 1 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={goToPage}
            />
          )}
        </div>
      </div>
    </div>
  );
};