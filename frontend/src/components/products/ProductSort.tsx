import { useLocalization } from '@/hooks';
import type { ProductSortOptions } from '@/services/productService';

interface ProductSortProps {
  sort?: ProductSortOptions;
  onSortChange: (sort: ProductSortOptions) => void;
  className?: string;
}

export const ProductSort = ({ sort, onSortChange, className = '' }: ProductSortProps) => {
  const { t } = useLocalization();

  const sortOptions = [
    { value: 'name-asc', label: t('products.sortNameAsc', 'Name (A-Z)'), field: 'name' as const, direction: 'asc' as const },
    { value: 'name-desc', label: t('products.sortNameDesc', 'Name (Z-A)'), field: 'name' as const, direction: 'desc' as const },
    { value: 'price-asc', label: t('products.sortPriceAsc', 'Price (Low to High)'), field: 'price' as const, direction: 'asc' as const },
    { value: 'price-desc', label: t('products.sortPriceDesc', 'Price (High to Low)'), field: 'price' as const, direction: 'desc' as const },
    { value: 'newest', label: t('products.sortNewest', 'Newest First'), field: 'createdAt' as const, direction: 'desc' as const },
    { value: 'oldest', label: t('products.sortOldest', 'Oldest First'), field: 'createdAt' as const, direction: 'asc' as const },
  ];

  const currentValue = sort ? `${sort.field}-${sort.direction}` : '';

  const handleSortChange = (value: string) => {
    const option = sortOptions.find(opt => opt.value === value);
    if (option) {
      onSortChange({
        field: option.field,
        direction: option.direction,
      });
    }
  };

  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <label htmlFor="product-sort" className="text-sm font-medium text-gray-700 dark:text-gray-300 whitespace-nowrap">
        {t('common.sort')}:
      </label>
      <select
        id="product-sort"
        value={currentValue}
        onChange={(e) => handleSortChange(e.target.value)}
        className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white text-sm"
      >
        <option value="">{t('products.defaultSort', 'Default')}</option>
        {sortOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
};