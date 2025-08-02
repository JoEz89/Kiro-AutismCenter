import React, { useState } from 'react';
import { BookmarkIcon } from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkSolidIcon } from '@heroicons/react/24/solid';
import { useLocalization } from '../../hooks/useLocalization';
import { courseService } from '../../services/courseService';
import { LoadingSpinner } from '../ui/LoadingSpinner';

interface BookmarkButtonProps {
  enrollmentId: string;
  moduleId: string;
  isBookmarked: boolean;
  onBookmarkChange?: (isBookmarked: boolean) => void;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
  showLabel?: boolean;
}

export const BookmarkButton: React.FC<BookmarkButtonProps> = ({
  enrollmentId,
  moduleId,
  isBookmarked: initialBookmarked,
  onBookmarkChange,
  className = '',
  size = 'md',
  showLabel = true,
}) => {
  const { t } = useLocalization();
  const [isBookmarked, setIsBookmarked] = useState(initialBookmarked);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
  };

  const buttonSizeClasses = {
    sm: 'px-2 py-1 text-xs',
    md: 'px-3 py-2 text-sm',
    lg: 'px-4 py-2 text-base',
  };

  const handleToggleBookmark = async () => {
    if (isLoading) return;

    setIsLoading(true);
    setError(null);

    try {
      if (isBookmarked) {
        await courseService.removeBookmark(enrollmentId, moduleId);
        setIsBookmarked(false);
        onBookmarkChange?.(false);
      } else {
        await courseService.addBookmark(enrollmentId, moduleId);
        setIsBookmarked(true);
        onBookmarkChange?.(true);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : t('courses.bookmarkError'));
      // Revert the optimistic update on error
      console.error('Bookmark error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const Icon = isBookmarked ? BookmarkSolidIcon : BookmarkIcon;
  const label = isBookmarked ? t('courses.removeBookmark') : t('courses.bookmark');

  if (showLabel) {
    return (
      <button
        onClick={handleToggleBookmark}
        disabled={isLoading}
        className={`inline-flex items-center gap-2 transition-colors ${buttonSizeClasses[size]} ${
          isBookmarked
            ? 'text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200'
            : 'text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400'
        } ${className}`}
        title={label}
        aria-label={label}
      >
        {isLoading ? (
          <LoadingSpinner size="sm" />
        ) : (
          <Icon className={sizeClasses[size]} />
        )}
        {showLabel && <span>{label}</span>}
      </button>
    );
  }

  return (
    <button
      onClick={handleToggleBookmark}
      disabled={isLoading}
      className={`inline-flex items-center justify-center p-2 rounded-full transition-colors ${
        isBookmarked
          ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20 hover:bg-blue-100 dark:hover:bg-blue-900/30'
          : 'text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-gray-800 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/20'
      } ${className}`}
      title={label}
      aria-label={label}
    >
      {isLoading ? (
        <LoadingSpinner size="sm" />
      ) : (
        <Icon className={sizeClasses[size]} />
      )}
    </button>
  );
};

export default BookmarkButton;