import React, { useState } from 'react';
import { XMarkIcon } from '@heroicons/react/24/outline';
import { useLocalization } from '../../hooks/useLocalization';
import type { CourseFilters as CourseFiltersType } from '../../services/courseService';

interface CourseFiltersProps {
  filters: CourseFiltersType;
  onFiltersChange: (filters: CourseFiltersType) => void;
  onClose: () => void;
}

export const CourseFilters: React.FC<CourseFiltersProps> = ({
  filters,
  onFiltersChange,
  onClose,
}) => {
  const { t } = useLocalization();
  const [localFilters, setLocalFilters] = useState<CourseFiltersType>(filters);

  const handleApplyFilters = () => {
    onFiltersChange(localFilters);
  };

  const handleClearFilters = () => {
    const clearedFilters: CourseFiltersType = {
      search: '',
      level: '',
      sortBy: undefined,
    };
    setLocalFilters(clearedFilters);
    onFiltersChange(clearedFilters);
  };

  const levelOptions = [
    { value: '', label: t('courses.allLevels') },
    { value: 'beginner', label: t('courses.beginner') },
    { value: 'intermediate', label: t('courses.intermediate') },
    { value: 'advanced', label: t('courses.advanced') },
  ];

  const sortOptions = [
    { value: 'title', label: t('courses.sortByTitle') },
    { value: 'duration', label: t('courses.sortByDuration') },
    { value: 'newest', label: t('courses.sortByNewest') },
    { value: 'oldest', label: t('courses.sortByOldest') },
  ];

  return (
    <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6 shadow-lg">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
          {t('common.filter')}
        </h3>
        <button
          onClick={onClose}
          className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
          aria-label={t('common.close')}
        >
          <XMarkIcon className="w-5 h-5" />
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* Level Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('courses.filterByLevel')}
          </label>
          <select
            value={localFilters.level || ''}
            onChange={(e) => setLocalFilters(prev => ({ ...prev, level: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
          >
            {levelOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        {/* Sort By */}
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('common.sort')}
          </label>
          <select
            value={localFilters.sortBy || ''}
            onChange={(e) => setLocalFilters(prev => ({ ...prev, sortBy: e.target.value as any }))}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
          >
            <option value="">{t('products.defaultSort')}</option>
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex items-center justify-end gap-3 mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
        <button
          onClick={handleClearFilters}
          className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white transition-colors"
        >
          {t('common.clear')}
        </button>
        <button
          onClick={handleApplyFilters}
          className="btn btn-primary"
        >
          {t('common.apply')}
        </button>
      </div>
    </div>
  );
};

export default CourseFilters;