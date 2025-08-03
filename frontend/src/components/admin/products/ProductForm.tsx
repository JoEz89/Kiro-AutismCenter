import React, { useState, useEffect } from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import type { Product, ProductCategory } from '@/types';

interface ProductFormProps {
  product?: Product | null;
  categories: ProductCategory[];
  onSave: (productData: any) => void;
  onCancel: () => void;
}

export const ProductForm: React.FC<ProductFormProps> = ({
  product,
  categories,
  onSave,
  onCancel,
}) => {
  const { t, isRTL } = useLocalization();
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  
  const [formData, setFormData] = useState({
    nameEn: '',
    nameAr: '',
    descriptionEn: '',
    descriptionAr: '',
    price: '',
    stockQuantity: '',
    categoryId: '',
    imageUrls: [''],
    isActive: true,
  });

  useEffect(() => {
    if (product) {
      setFormData({
        nameEn: product.nameEn,
        nameAr: product.nameAr,
        descriptionEn: product.descriptionEn,
        descriptionAr: product.descriptionAr,
        price: product.price.toString(),
        stockQuantity: product.stockQuantity.toString(),
        categoryId: product.categoryId,
        imageUrls: product.imageUrls.length > 0 ? product.imageUrls : [''],
        isActive: product.isActive,
      });
    }
  }, [product]);

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const handleImageUrlChange = (index: number, value: string) => {
    const newImageUrls = [...formData.imageUrls];
    newImageUrls[index] = value;
    setFormData(prev => ({ ...prev, imageUrls: newImageUrls }));
  };

  const addImageUrl = () => {
    setFormData(prev => ({
      ...prev,
      imageUrls: [...prev.imageUrls, '']
    }));
  };

  const removeImageUrl = (index: number) => {
    if (formData.imageUrls.length > 1) {
      const newImageUrls = formData.imageUrls.filter((_, i) => i !== index);
      setFormData(prev => ({ ...prev, imageUrls: newImageUrls }));
    }
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.nameEn.trim()) {
      newErrors.nameEn = t('admin.products.validation.nameEnRequired', 'English name is required');
    }
    if (!formData.nameAr.trim()) {
      newErrors.nameAr = t('admin.products.validation.nameArRequired', 'Arabic name is required');
    }
    if (!formData.descriptionEn.trim()) {
      newErrors.descriptionEn = t('admin.products.validation.descriptionEnRequired', 'English description is required');
    }
    if (!formData.descriptionAr.trim()) {
      newErrors.descriptionAr = t('admin.products.validation.descriptionArRequired', 'Arabic description is required');
    }
    if (!formData.price || parseFloat(formData.price) <= 0) {
      newErrors.price = t('admin.products.validation.priceRequired', 'Valid price is required');
    }
    if (!formData.stockQuantity || parseInt(formData.stockQuantity) < 0) {
      newErrors.stockQuantity = t('admin.products.validation.stockRequired', 'Valid stock quantity is required');
    }
    if (!formData.categoryId) {
      newErrors.categoryId = t('admin.products.validation.categoryRequired', 'Category is required');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    try {
      const productData = {
        ...formData,
        price: parseFloat(formData.price),
        stockQuantity: parseInt(formData.stockQuantity),
        imageUrls: formData.imageUrls.filter(url => url.trim() !== ''),
      };
      
      await onSave(productData);
    } catch (error) {
      console.error('Failed to save product:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-11/12 max-w-4xl shadow-lg rounded-md bg-white dark:bg-gray-800">
        <div className="mt-3">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-white">
              {product 
                ? t('admin.products.editProduct', 'Edit Product')
                : t('admin.products.addProduct', 'Add Product')
              }
            </h3>
            <button
              onClick={onCancel}
              className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* English Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.nameEn', 'Product Name (English)')} *
                </label>
                <input
                  type="text"
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.nameEn 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.nameEn}
                  onChange={(e) => handleInputChange('nameEn', e.target.value)}
                  placeholder={t('admin.products.nameEnPlaceholder', 'Enter product name in English')}
                />
                {errors.nameEn && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.nameEn}</p>
                )}
              </div>

              {/* Arabic Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.nameAr', 'Product Name (Arabic)')} *
                </label>
                <input
                  type="text"
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.nameAr 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.nameAr}
                  onChange={(e) => handleInputChange('nameAr', e.target.value)}
                  placeholder={t('admin.products.nameArPlaceholder', 'Enter product name in Arabic')}
                  dir="rtl"
                />
                {errors.nameAr && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.nameAr}</p>
                )}
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* English Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.descriptionEn', 'Description (English)')} *
                </label>
                <textarea
                  rows={4}
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.descriptionEn 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.descriptionEn}
                  onChange={(e) => handleInputChange('descriptionEn', e.target.value)}
                  placeholder={t('admin.products.descriptionEnPlaceholder', 'Enter product description in English')}
                />
                {errors.descriptionEn && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.descriptionEn}</p>
                )}
              </div>

              {/* Arabic Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.descriptionAr', 'Description (Arabic)')} *
                </label>
                <textarea
                  rows={4}
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.descriptionAr 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.descriptionAr}
                  onChange={(e) => handleInputChange('descriptionAr', e.target.value)}
                  placeholder={t('admin.products.descriptionArPlaceholder', 'Enter product description in Arabic')}
                  dir="rtl"
                />
                {errors.descriptionAr && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.descriptionAr}</p>
                )}
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {/* Price */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.price', 'Price (BHD)')} *
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.price 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.price}
                  onChange={(e) => handleInputChange('price', e.target.value)}
                  placeholder="0.00"
                />
                {errors.price && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.price}</p>
                )}
              </div>

              {/* Stock Quantity */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.stockQuantity', 'Stock Quantity')} *
                </label>
                <input
                  type="number"
                  min="0"
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.stockQuantity 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.stockQuantity}
                  onChange={(e) => handleInputChange('stockQuantity', e.target.value)}
                  placeholder="0"
                />
                {errors.stockQuantity && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.stockQuantity}</p>
                )}
              </div>

              {/* Category */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('admin.products.category', 'Category')} *
                </label>
                <select
                  className={cn(
                    "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                    "bg-white dark:bg-gray-700 text-gray-900 dark:text-white",
                    errors.categoryId 
                      ? "border-red-300 dark:border-red-600" 
                      : "border-gray-300 dark:border-gray-600"
                  )}
                  value={formData.categoryId}
                  onChange={(e) => handleInputChange('categoryId', e.target.value)}
                >
                  <option value="">{t('admin.products.selectCategory', 'Select a category')}</option>
                  {categories.map((category) => (
                    <option key={category.id} value={category.id}>
                      {isRTL ? category.nameAr : category.nameEn}
                    </option>
                  ))}
                </select>
                {errors.categoryId && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.categoryId}</p>
                )}
              </div>
            </div>

            {/* Image URLs */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('admin.products.imageUrls', 'Product Images')}
              </label>
              <div className="space-y-2">
                {formData.imageUrls.map((url, index) => (
                  <div key={index} className="flex items-center space-x-2 rtl:space-x-reverse">
                    <input
                      type="url"
                      className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      value={url}
                      onChange={(e) => handleImageUrlChange(index, e.target.value)}
                      placeholder={t('admin.products.imageUrlPlaceholder', 'Enter image URL')}
                    />
                    {formData.imageUrls.length > 1 && (
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => removeImageUrl(index)}
                        className="text-red-600 hover:text-red-700 border-red-300 hover:border-red-400"
                      >
                        {t('common.remove', 'Remove')}
                      </Button>
                    )}
                  </div>
                ))}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addImageUrl}
                >
                  {t('admin.products.addImage', 'Add Image')}
                </Button>
              </div>
            </div>

            {/* Active Status */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="isActive"
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                checked={formData.isActive}
                onChange={(e) => handleInputChange('isActive', e.target.checked)}
              />
              <label htmlFor="isActive" className="ml-2 rtl:ml-0 rtl:mr-2 block text-sm text-gray-900 dark:text-white">
                {t('admin.products.isActive', 'Product is active')}
              </label>
            </div>

            {/* Form Actions */}
            <div className="flex items-center justify-end space-x-3 rtl:space-x-reverse pt-6 border-t border-gray-200 dark:border-gray-700">
              <Button
                type="button"
                variant="outline"
                onClick={onCancel}
                disabled={isLoading}
              >
                {t('common.cancel', 'Cancel')}
              </Button>
              <Button
                type="submit"
                disabled={isLoading}
              >
                {isLoading ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    {t('common.saving', 'Saving...')}
                  </>
                ) : (
                  t('common.save', 'Save')
                )}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default ProductForm;