import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { MagnifyingGlassIcon, FunnelIcon, Squares2X2Icon, ListBulletIcon } from '@heroicons/react/24/outline';
import { useCourses } from '../../hooks/useCourses';
import { useLocalization } from '../../hooks/useLocalization';
import { CourseCard } from './CourseCard';
import { CourseFilters } from './CourseFilters';
import { Pagination } from '../ui/Pagination';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { EmptyState } from '../ui/EmptyState';

interface CourseCatalogProps {
  showFilters?: boolean;
  limit?: number;
  className?: string;
}

export const CourseCatalog: React.FC<CourseCatalogProps> = ({
  showFilters = true,
  limit = 12,
  className = '',
}) => {
  const { t } = useLocalization();
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showFiltersPanel, setShowFiltersPanel] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const { courses, loading, error, filters, pagination, updateFilters, changePage } = useCourses({
    limit,
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    updateFilters({ search: searchQuery });
  };

  const handleFilterChange = (newFilters: any) => {
    updateFilters(newFilters);
    setShowFiltersPanel(false);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="btn btn-primary"
        >
          {t('common.retry')}
        </button>
      </div>
    );
  }

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            {t('courses.title')}
          </h1>
          <p className="text-gray-600 dark:text-gray-300 mt-2">
            {t('courses.description')}
          </p>
        </div>

        {/* View Mode Toggle */}
        <div className="flex items-center gap-2">
          <button
            onClick={() => setViewMode('grid')}
            className={`p-2 rounded-lg ${
              viewMode === 'grid'
                ? 'bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-300'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
            }`}
            aria-label={t('products.gridView')}
          >
            <Squares2X2Icon className="w-5 h-5" />
          </button>
          <button
            onClick={() => setViewMode('list')}
            className={`p-2 rounded-lg ${
              viewMode === 'list'
                ? 'bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-300'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
            }`}
            aria-label={t('products.listView')}
          >
            <ListBulletIcon className="w-5 h-5" />
          </button>
        </div>
      </div>

      {/* Search and Filters */}
      {showFilters && (
        <div className="flex flex-col sm:flex-row gap-4">
          {/* Search */}
          <form onSubmit={handleSearch} className="flex-1">
            <div className="relative">
              <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder={t('courses.searchPlaceholder')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                aria-label={t('courses.searchLabel')}
              />
            </div>
          </form>

          {/* Filter Toggle */}
          <button
            onClick={() => setShowFiltersPanel(!showFiltersPanel)}
            className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 dark:border-gray-600 dark:hover:bg-gray-700 dark:text-white"
          >
            <FunnelIcon className="w-5 h-5" />
            {t('common.filter')}
          </button>
        </div>
      )}

      {/* Filters Panel */}
      {showFiltersPanel && (
        <CourseFilters
          filters={filters}
          onFiltersChange={handleFilterChange}
          onClose={() => setShowFiltersPanel(false)}
        />
      )}

      {/* Results Count */}
      {courses.length > 0 && (
        <div className="text-sm text-gray-600 dark:text-gray-300">
          {t('courses.showingResults', { count: pagination.total })}
        </div>
      )}

      {/* Course Grid/List */}
      {courses.length === 0 ? (
        <EmptyState
          icon={MagnifyingGlassIcon}
          title={t('courses.noCourses')}
          description={t('courses.noCoursesDescription')}
        />
      ) : (
        <>
          <div
            className={
              viewMode === 'grid'
                ? 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6'
                : 'space-y-4'
            }
          >
            {courses.map((course) => (
              <CourseCard
                key={course.id}
                course={course}
                viewMode={viewMode}
                className={viewMode === 'list' ? 'w-full' : ''}
              />
            ))}
          </div>

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="flex justify-center mt-8">
              <Pagination
                currentPage={pagination.page}
                totalPages={pagination.totalPages}
                onPageChange={changePage}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default CourseCatalog;