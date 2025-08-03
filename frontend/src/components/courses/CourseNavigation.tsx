import React from 'react';
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  CheckCircleIcon,
  ClockIcon,
  BookmarkIcon,
  PlayIcon,
} from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkSolidIcon } from '@heroicons/react/24/solid';
import { useLocalization } from '../../hooks/useLocalization';
import { formatDuration } from '../../utils';
import type { CourseModule } from '../../types';

interface CourseNavigationProps {
  modules: CourseModule[];
  currentModuleIndex: number;
  completedModules: string[];
  bookmarkedModules: string[];
  onModuleChange: (index: number) => void;
  onBookmarkToggle: (moduleId: string) => void;
  className?: string;
}

export const CourseNavigation: React.FC<CourseNavigationProps> = ({
  modules,
  currentModuleIndex,
  completedModules,
  bookmarkedModules,
  onModuleChange,
  onBookmarkToggle,
  className = '',
}) => {
  const { t, language } = useLocalization();

  const getModuleStatus = (module: CourseModule) => {
    if (completedModules.includes(module.id)) return 'completed';
    return 'not-started';
  };

  const isBookmarked = (moduleId: string) => bookmarkedModules.includes(moduleId);

  const currentModule = modules[currentModuleIndex];
  const isFirstModule = currentModuleIndex === 0;
  const isLastModule = currentModuleIndex === modules.length - 1;

  return (
    <div className={`bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 ${className}`}>
      {/* Header */}
      <div className="p-4 border-b border-gray-200 dark:border-gray-700">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
          {t('courses.courseContent')}
        </h3>
        <p className="text-sm text-gray-600 dark:text-gray-300 mt-1">
          {t('courses.moduleProgress', { 
            current: currentModuleIndex + 1, 
            total: modules.length 
          })}
        </p>
      </div>

      {/* Module List */}
      <div className="p-4 space-y-2 max-h-96 overflow-y-auto">
        {modules.map((module, index) => {
          const status = getModuleStatus(module);
          const isCurrent = index === currentModuleIndex;
          const moduleTitle = language === 'ar' ? module.titleAr : module.titleEn;
          
          return (
            <div
              key={module.id}
              className={`relative group rounded-lg border transition-all duration-200 ${
                isCurrent
                  ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 shadow-sm'
                  : 'bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-600'
              }`}
            >
              <button
                onClick={() => onModuleChange(index)}
                className="w-full text-left p-3 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-inset rounded-lg"
                aria-label={t('courses.goToModule', { module: moduleTitle })}
              >
                <div className="flex items-start gap-3">
                  {/* Module Number/Status */}
                  <div className={`flex items-center justify-center w-8 h-8 rounded-full text-sm font-medium flex-shrink-0 ${
                    status === 'completed'
                      ? 'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-400'
                      : isCurrent
                      ? 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400'
                      : 'bg-gray-200 dark:bg-gray-600 text-gray-600 dark:text-gray-300'
                  }`}>
                    {status === 'completed' ? (
                      <CheckCircleIcon className="w-5 h-5" />
                    ) : isCurrent ? (
                      <PlayIcon className="w-4 h-4" />
                    ) : (
                      index + 1
                    )}
                  </div>
                  
                  {/* Module Info */}
                  <div className="flex-1 min-w-0">
                    <h4 className={`font-medium text-sm truncate ${
                      isCurrent
                        ? 'text-blue-900 dark:text-blue-100'
                        : 'text-gray-900 dark:text-white'
                    }`}>
                      {moduleTitle}
                    </h4>
                    <div className="flex items-center gap-3 mt-1">
                      <div className="flex items-center gap-1">
                        <ClockIcon className="w-3 h-3 text-gray-400" />
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          {formatDuration(module.duration)}
                        </span>
                      </div>
                      {isBookmarked(module.id) && (
                        <BookmarkSolidIcon className="w-3 h-3 text-yellow-500" />
                      )}
                      {status === 'completed' && (
                        <span className="text-xs text-green-600 dark:text-green-400 font-medium">
                          {t('courses.completed')}
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Bookmark Button */}
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onBookmarkToggle(module.id);
                    }}
                    className={`p-1 rounded opacity-0 group-hover:opacity-100 transition-opacity ${
                      isBookmarked(module.id)
                        ? 'text-yellow-500 hover:text-yellow-600'
                        : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300'
                    }`}
                    aria-label={
                      isBookmarked(module.id)
                        ? t('courses.removeBookmark')
                        : t('courses.addBookmark')
                    }
                  >
                    {isBookmarked(module.id) ? (
                      <BookmarkSolidIcon className="w-4 h-4" />
                    ) : (
                      <BookmarkIcon className="w-4 h-4" />
                    )}
                  </button>
                </div>
              </button>
            </div>
          );
        })}
      </div>

      {/* Navigation Controls */}
      <div className="p-4 border-t border-gray-200 dark:border-gray-700">
        <div className="flex items-center justify-between">
          <button
            onClick={() => onModuleChange(currentModuleIndex - 1)}
            disabled={isFirstModule}
            className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeftIcon className="w-4 h-4" />
            {t('courses.previousModule')}
          </button>

          <div className="text-center">
            <p className="text-xs text-gray-500 dark:text-gray-400">
              {currentModule && (language === 'ar' ? currentModule.titleAr : currentModule.titleEn)}
            </p>
          </div>

          <button
            onClick={() => onModuleChange(currentModuleIndex + 1)}
            disabled={isLastModule}
            className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 rounded-lg disabled:cursor-not-allowed transition-colors"
          >
            {t('courses.nextModule')}
            <ChevronRightIcon className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
};

export default CourseNavigation;