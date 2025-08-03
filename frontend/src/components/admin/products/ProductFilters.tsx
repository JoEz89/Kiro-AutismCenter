import React from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import type { ProductCategory } from '@/types';

interface ProductFiltersProps {
  filters: {
    search: string;
    category: string;
    inStock: boolean | undefined;
    isActive: boolean | undefined;
  };
  categories: ProductCategory[];
  onFiltersChange: (filters: any) => void;
}

export const ProductFilters: React.FC<ProductFiltersProps> = ({
  filters,
  categories,
  onFiltersChange,
}) => {
  const { t, isRTL } = useLocalization();

  const handleFilterChange = (key: string, value: any) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    });
  };

  const clearFilters = () => {
    onFiltersChange({
      search: '',
      category: '',
      inStock: undefined,
      isActive: undefined,
    });
  };

  const hasActiveFilters = filters.search || filters.category || 
    filters.inStock !== undefined || filters.isActive !== undefined;

  return (
    <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Search */}
        <div>
          <label htmlFor="search" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('admin.products.search', 'Search Products')}
          </label>
          <div className="relative">
            <input
              type="text"
              id="search"
              className="block w-full pl-10 rtl:pl-3 rtl:pr-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md leading-5 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              placeholder={t('admin.products.searchPlaceholder', 'Search by name or description...')}
              value={filters.search}
              onChange={(e) => handleFilterChange('search', e.target.value)}
            />
            <div className="absolute inset-y-0 left-0 rtl:left-auto rtl:right-0 pl-3 rtl:pl-0 rtl:pr-3 flex items-center pointer-events-none">
              <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Category */}
        <div>
          <label htmlFor="category" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('admin.products.category', 'Category')}
          </label>
          <select
            id="category"
            className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            value={filters.category}
            onChange={(e) => handleFilterChange('category', e.target.value)}
          >
            <option value="">{t('admin.products.allCategories', 'All Categories')}</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {isRTL ? category.nameAr : category.nameEn}
              </option>
            ))}
          </select>
        </div>

        {/* Stock Status */}
        <div>
          <label htmlFor="stock" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('admin.products.stockStatus', 'Stock Status')}
          </label>
          <select
            id="stock"
            className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            value={filters.inStock === undefined ? '' : filters.inStock.toString()}
            onChange={(e) => handleFilterChange('inStock', e.target.value === '' ? undefined : e.target.value === 'true')}
          >
            <option value="">{t('admin.products.allStock', 'All Stock Levels')}</option>
            <option value="true">{t('admin.products.inStock', 'In Stock')}</option>
            <option value="false">{t('admin.products.outOfStock', 'Out of Stock')}</option>
          </select>
        </div>

        {/* Active Status */}
        <div>
          <label htmlFor="active" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('admin.products.status', 'Status')}
          </label>
          <select
            id="active"
            className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            value={filters.isActive === undefined ? '' : filters.isActive.toString()}
            onChange={(e) => handleFilterChange('isActive', e.target.value === '' ? undefined : e.target.value === 'true')}
          >
            <option value="">{t('admin.products.allStatuses', 'All Statuses')}</option>
            <option value="true">{t('admin.products.active', 'Active')}</option>
            <option value="false">{t('admin.products.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      {/* Clear filters */}
      {hasActiveFilters && (
        <div className="mt-4 flex justify-end">
          <Button
            variant="outline"
            size="sm"
            onClick={clearFilters}
          >
            {t('admin.products.clearFilters', 'Clear Filters')}
          </Button>
        </div>
      )}
    </div>
  );
};

export default ProductFilters;