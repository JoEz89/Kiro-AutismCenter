import React from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import type { Product, ProductCategory } from '@/types';

interface ProductListProps {
  products: Product[];
  categories: ProductCategory[];
  selectedProducts: string[];
  onSelectionChange: (selectedIds: string[]) => void;
  onEdit: (product: Product) => void;
  onDelete: (productId: string) => void;
  isLoading: boolean;
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export const ProductList: React.FC<ProductListProps> = ({
  products,
  categories,
  selectedProducts,
  onSelectionChange,
  onEdit,
  onDelete,
  isLoading,
  currentPage,
  totalPages,
  onPageChange,
}) => {
  const { t, isRTL } = useLocalization();

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      onSelectionChange(products.map(p => p.id));
    } else {
      onSelectionChange([]);
    }
  };

  const handleSelectProduct = (productId: string, checked: boolean) => {
    if (checked) {
      onSelectionChange([...selectedProducts, productId]);
    } else {
      onSelectionChange(selectedProducts.filter(id => id !== productId));
    }
  };

  const getCategoryName = (categoryId: string) => {
    const category = categories.find(c => c.id === categoryId);
    return category ? (isRTL ? category.nameAr : category.nameEn) : '-';
  };

  const getProductName = (product: Product) => {
    return isRTL ? product.nameAr : product.nameEn;
  };

  const getProductDescription = (product: Product) => {
    const desc = isRTL ? product.descriptionAr : product.descriptionEn;
    return desc.length > 100 ? `${desc.substring(0, 100)}...` : desc;
  };

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
        <div className="p-6">
          <div className="animate-pulse space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex items-center space-x-4 rtl:space-x-reverse">
                <div className="w-4 h-4 bg-gray-300 dark:bg-gray-600 rounded"></div>
                <div className="w-16 h-16 bg-gray-300 dark:bg-gray-600 rounded"></div>
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-gray-300 dark:bg-gray-600 rounded w-1/4"></div>
                  <div className="h-3 bg-gray-300 dark:bg-gray-600 rounded w-3/4"></div>
                </div>
                <div className="w-20 h-4 bg-gray-300 dark:bg-gray-600 rounded"></div>
                <div className="w-16 h-4 bg-gray-300 dark:bg-gray-600 rounded"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
      {/* Table header */}
      <div className="px-6 py-3 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700">
        <div className="flex items-center">
          <input
            type="checkbox"
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            checked={selectedProducts.length === products.length && products.length > 0}
            onChange={(e) => handleSelectAll(e.target.checked)}
          />
          <span className="ml-3 rtl:ml-0 rtl:mr-3 text-sm font-medium text-gray-900 dark:text-white">
            {t('admin.products.selectAll', 'Select All')}
          </span>
        </div>
      </div>

      {/* Product list */}
      <div className="divide-y divide-gray-200 dark:divide-gray-700">
        {products.length === 0 ? (
          <div className="p-6 text-center">
            <div className="text-gray-500 dark:text-gray-400">
              <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <h3 className="text-lg font-medium mb-2">
                {t('admin.products.noProducts', 'No products found')}
              </h3>
              <p className="text-sm">
                {t('admin.products.noProductsDescription', 'Get started by creating your first product.')}
              </p>
            </div>
          </div>
        ) : (
          products.map((product) => (
            <div key={product.id} className="p-6 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
              <div className="flex items-center space-x-4 rtl:space-x-reverse">
                {/* Checkbox */}
                <input
                  type="checkbox"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  checked={selectedProducts.includes(product.id)}
                  onChange={(e) => handleSelectProduct(product.id, e.target.checked)}
                />

                {/* Product image */}
                <div className="flex-shrink-0">
                  {product.imageUrls.length > 0 ? (
                    <img
                      className="h-16 w-16 rounded-lg object-cover"
                      src={product.imageUrls[0]}
                      alt={getProductName(product)}
                    />
                  ) : (
                    <div className="h-16 w-16 rounded-lg bg-gray-200 dark:bg-gray-600 flex items-center justify-center">
                      <svg className="h-8 w-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                  )}
                </div>

                {/* Product details */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center space-x-2 rtl:space-x-reverse">
                    <h3 className="text-lg font-medium text-gray-900 dark:text-white truncate">
                      {getProductName(product)}
                    </h3>
                    <span className={cn(
                      'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                      product.isActive
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
                    )}>
                      {product.isActive 
                        ? t('admin.products.active', 'Active')
                        : t('admin.products.inactive', 'Inactive')
                      }
                    </span>
                  </div>
                  <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                    {getProductDescription(product)}
                  </p>
                  <div className="flex items-center space-x-4 rtl:space-x-reverse mt-2 text-sm text-gray-500 dark:text-gray-400">
                    <span>
                      {t('admin.products.category', 'Category')}: {getCategoryName(product.categoryId)}
                    </span>
                    <span>
                      {t('admin.products.stock', 'Stock')}: {product.stockQuantity}
                    </span>
                  </div>
                </div>

                {/* Price */}
                <div className="text-right rtl:text-left">
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {product.price.toFixed(2)} {product.currency}
                  </div>
                  <div className={cn(
                    'text-sm',
                    product.stockQuantity > 0
                      ? 'text-green-600 dark:text-green-400'
                      : 'text-red-600 dark:text-red-400'
                  )}>
                    {product.stockQuantity > 0
                      ? t('admin.products.inStock', 'In Stock')
                      : t('admin.products.outOfStock', 'Out of Stock')
                    }
                  </div>
                </div>

                {/* Actions */}
                <div className="flex items-center space-x-2 rtl:space-x-reverse">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onEdit(product)}
                  >
                    {t('common.edit', 'Edit')}
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onDelete(product.id)}
                    className="text-red-600 hover:text-red-700 border-red-300 hover:border-red-400"
                  >
                    {t('common.delete', 'Delete')}
                  </Button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="px-6 py-3 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700">
          <div className="flex items-center justify-between">
            <div className="text-sm text-gray-700 dark:text-gray-300">
              {t('admin.products.page', 'Page')} {currentPage} {t('common.of', 'of')} {totalPages}
            </div>
            <div className="flex items-center space-x-2 rtl:space-x-reverse">
              <Button
                variant="outline"
                size="sm"
                onClick={() => onPageChange(currentPage - 1)}
                disabled={currentPage === 1}
              >
                {t('common.previous', 'Previous')}
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => onPageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
              >
                {t('common.next', 'Next')}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductList;