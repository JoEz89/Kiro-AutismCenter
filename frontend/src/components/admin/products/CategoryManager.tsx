import React, { useState, useEffect } from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import { adminProductService } from '@/services/adminProductService';
import type { ProductCategory } from '@/types';

interface CategoryManagerProps {
  categories: ProductCategory[];
  onClose: () => void;
  onCategoriesChange: () => void;
}

export const CategoryManager: React.FC<CategoryManagerProps> = ({
  categories,
  onClose,
  onCategoriesChange,
}) => {
  const { t, isRTL } = useLocalization();
  const [isLoading, setIsLoading] = useState(false);
  const [editingCategory, setEditingCategory] = useState<ProductCategory | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  
  const [formData, setFormData] = useState({
    nameEn: '',
    nameAr: '',
    descriptionEn: '',
    descriptionAr: '',
    isActive: true,
  });

  const resetForm = () => {
    setFormData({
      nameEn: '',
      nameAr: '',
      descriptionEn: '',
      descriptionAr: '',
      isActive: true,
    });
    setErrors({});
    setEditingCategory(null);
  };

  const handleEdit = (category: ProductCategory) => {
    setEditingCategory(category);
    setFormData({
      nameEn: category.nameEn,
      nameAr: category.nameAr,
      descriptionEn: category.descriptionEn || '',
      descriptionAr: category.descriptionAr || '',
      isActive: category.isActive,
    });
    setShowForm(true);
  };

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.nameEn.trim()) {
      newErrors.nameEn = t('admin.categories.validation.nameEnRequired', 'English name is required');
    }
    if (!formData.nameAr.trim()) {
      newErrors.nameAr = t('admin.categories.validation.nameArRequired', 'Arabic name is required');
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
      if (editingCategory) {
        await adminProductService.updateCategory(editingCategory.id, formData);
      } else {
        await adminProductService.createCategory(formData);
      }
      
      await onCategoriesChange();
      setShowForm(false);
      resetForm();
    } catch (error) {
      console.error('Failed to save category:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (categoryId: string) => {
    if (window.confirm(t('admin.categories.confirmDelete', 'Are you sure you want to delete this category?'))) {
      setIsLoading(true);
      try {
        await adminProductService.deleteCategory(categoryId);
        await onCategoriesChange();
      } catch (error) {
        console.error('Failed to delete category:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-11/12 max-w-4xl shadow-lg rounded-md bg-white dark:bg-gray-800">
        <div className="mt-3">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-white">
              {t('admin.categories.title', 'Category Management')}
            </h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Add Category Button */}
          <div className="mb-6">
            <Button
              onClick={() => {
                resetForm();
                setShowForm(true);
              }}
              disabled={isLoading}
            >
              {t('admin.categories.addCategory', 'Add Category')}
            </Button>
          </div>

          {/* Categories List */}
          <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4 mb-6">
            <h4 className="text-md font-medium text-gray-900 dark:text-white mb-4">
              {t('admin.categories.existingCategories', 'Existing Categories')}
            </h4>
            
            {categories.length === 0 ? (
              <div className="text-center py-8">
                <div className="text-gray-500 dark:text-gray-400">
                  <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                  </svg>
                  <h3 className="text-lg font-medium mb-2">
                    {t('admin.categories.noCategories', 'No categories found')}
                  </h3>
                  <p className="text-sm">
                    {t('admin.categories.noCategoriesDescription', 'Create your first category to organize products.')}
                  </p>
                </div>
              </div>
            ) : (
              <div className="space-y-3">
                {categories.map((category) => (
                  <div key={category.id} className="flex items-center justify-between p-3 bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-600">
                    <div className="flex-1">
                      <div className="flex items-center space-x-2 rtl:space-x-reverse">
                        <h5 className="font-medium text-gray-900 dark:text-white">
                          {isRTL ? category.nameAr : category.nameEn}
                        </h5>
                        <span className={cn(
                          'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                          category.isActive
                            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                            : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
                        )}>
                          {category.isActive 
                            ? t('admin.categories.active', 'Active')
                            : t('admin.categories.inactive', 'Inactive')
                          }
                        </span>
                      </div>
                      {(category.descriptionEn || category.descriptionAr) && (
                        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                          {isRTL ? category.descriptionAr : category.descriptionEn}
                        </p>
                      )}
                    </div>
                    <div className="flex items-center space-x-2 rtl:space-x-reverse">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleEdit(category)}
                        disabled={isLoading}
                      >
                        {t('common.edit', 'Edit')}
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleDelete(category.id)}
                        disabled={isLoading}
                        className="text-red-600 hover:text-red-700 border-red-300 hover:border-red-400"
                      >
                        {t('common.delete', 'Delete')}
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Category Form */}
          {showForm && (
            <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-6">
              <h4 className="text-md font-medium text-gray-900 dark:text-white mb-4">
                {editingCategory 
                  ? t('admin.categories.editCategory', 'Edit Category')
                  : t('admin.categories.addCategory', 'Add Category')
                }
              </h4>
              
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* English Name */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      {t('admin.categories.nameEn', 'Category Name (English)')} *
                    </label>
                    <input
                      type="text"
                      className={cn(
                        "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                        "bg-white dark:bg-gray-600 text-gray-900 dark:text-white",
                        errors.nameEn 
                          ? "border-red-300 dark:border-red-600" 
                          : "border-gray-300 dark:border-gray-500"
                      )}
                      value={formData.nameEn}
                      onChange={(e) => handleInputChange('nameEn', e.target.value)}
                      placeholder={t('admin.categories.nameEnPlaceholder', 'Enter category name in English')}
                    />
                    {errors.nameEn && (
                      <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.nameEn}</p>
                    )}
                  </div>

                  {/* Arabic Name */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      {t('admin.categories.nameAr', 'Category Name (Arabic)')} *
                    </label>
                    <input
                      type="text"
                      className={cn(
                        "block w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500",
                        "bg-white dark:bg-gray-600 text-gray-900 dark:text-white",
                        errors.nameAr 
                          ? "border-red-300 dark:border-red-600" 
                          : "border-gray-300 dark:border-gray-500"
                      )}
                      value={formData.nameAr}
                      onChange={(e) => handleInputChange('nameAr', e.target.value)}
                      placeholder={t('admin.categories.nameArPlaceholder', 'Enter category name in Arabic')}
                      dir="rtl"
                    />
                    {errors.nameAr && (
                      <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.nameAr}</p>
                    )}
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* English Description */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      {t('admin.categories.descriptionEn', 'Description (English)')}
                    </label>
                    <textarea
                      rows={3}
                      className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 bg-white dark:bg-gray-600 text-gray-900 dark:text-white"
                      value={formData.descriptionEn}
                      onChange={(e) => handleInputChange('descriptionEn', e.target.value)}
                      placeholder={t('admin.categories.descriptionEnPlaceholder', 'Enter category description in English')}
                    />
                  </div>

                  {/* Arabic Description */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      {t('admin.categories.descriptionAr', 'Description (Arabic)')}
                    </label>
                    <textarea
                      rows={3}
                      className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 bg-white dark:bg-gray-600 text-gray-900 dark:text-white"
                      value={formData.descriptionAr}
                      onChange={(e) => handleInputChange('descriptionAr', e.target.value)}
                      placeholder={t('admin.categories.descriptionArPlaceholder', 'Enter category description in Arabic')}
                      dir="rtl"
                    />
                  </div>
                </div>

                {/* Active Status */}
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="categoryIsActive"
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    checked={formData.isActive}
                    onChange={(e) => handleInputChange('isActive', e.target.checked)}
                  />
                  <label htmlFor="categoryIsActive" className="ml-2 rtl:ml-0 rtl:mr-2 block text-sm text-gray-900 dark:text-white">
                    {t('admin.categories.isActive', 'Category is active')}
                  </label>
                </div>

                {/* Form Actions */}
                <div className="flex items-center justify-end space-x-3 rtl:space-x-reverse pt-4">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => {
                      setShowForm(false);
                      resetForm();
                    }}
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
          )}
        </div>
      </div>
    </div>
  );
};

export default CategoryManager;