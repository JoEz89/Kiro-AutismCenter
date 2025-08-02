import React from 'react';
import { Link } from 'react-router-dom';
import { ClockIcon, PlayIcon, BookmarkIcon } from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkSolidIcon } from '@heroicons/react/24/solid';
import type { Course } from '../../types';
import { useLocalization } from '../../hooks/useLocalization';
import { useEnrollments } from '../../hooks/useCourses';
import { formatDuration } from '../../utils';

interface CourseCardProps {
  course: Course;
  viewMode?: 'grid' | 'list';
  className?: string;
  showBookmark?: boolean;
  isBookmarked?: boolean;
  onBookmarkToggle?: () => void;
}

export const CourseCard: React.FC<CourseCardProps> = ({
  course,
  viewMode = 'grid',
  className = '',
  showBookmark = false,
  isBookmarked = false,
  onBookmarkToggle,
}) => {
  const { t, language } = useLocalization();
  const { enrollments } = useEnrollments();

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const description = language === 'ar' ? course.descriptionAr : course.descriptionEn;

  // Check if user is enrolled in this course
  const enrollment = enrollments.find(e => e.courseId === course.id);
  const isEnrolled = !!enrollment;
  const isExpired = enrollment && new Date(enrollment.expiryDate) < new Date();

  const getEnrollmentStatus = () => {
    if (!enrollment) return null;
    
    if (isExpired) {
      return {
        text: t('courses.enrollmentExpired'),
        className: 'text-red-600 bg-red-50 dark:bg-red-900/20 dark:text-red-400',
      };
    }

    if (enrollment.completionDate) {
      return {
        text: t('courses.completed'),
        className: 'text-green-600 bg-green-50 dark:bg-green-900/20 dark:text-green-400',
      };
    }

    if (enrollment.progress > 0) {
      return {
        text: t('courses.inProgress'),
        className: 'text-blue-600 bg-blue-50 dark:bg-blue-900/20 dark:text-blue-400',
      };
    }

    return {
      text: t('courses.enrolled'),
      className: 'text-blue-600 bg-blue-50 dark:bg-blue-900/20 dark:text-blue-400',
    };
  };

  const enrollmentStatus = getEnrollmentStatus();

  const getActionButton = () => {
    if (!isEnrolled) {
      return (
        <Link
          to={`/courses/${course.id}`}
          className="btn btn-primary w-full"
        >
          {t('courses.enroll')}
        </Link>
      );
    }

    if (isExpired) {
      return (
        <button
          disabled
          className="btn btn-secondary w-full opacity-50 cursor-not-allowed"
        >
          {t('courses.enrollmentExpired')}
        </button>
      );
    }

    if (enrollment?.progress === 0) {
      return (
        <Link
          to={`/courses/${course.id}/learn`}
          className="btn btn-primary w-full"
        >
          {t('courses.startCourse')}
        </Link>
      );
    }

    return (
      <Link
        to={`/courses/${course.id}/learn`}
        className="btn btn-primary w-full"
      >
        {t('courses.continueCourse')}
      </Link>
    );
  };

  if (viewMode === 'list') {
    return (
      <div className={`bg-white dark:bg-gray-800 rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200 ${className}`}>
        <div className="flex flex-col sm:flex-row">
          {/* Thumbnail */}
          <div className="relative sm:w-64 sm:flex-shrink-0">
            <img
              src={course.thumbnailUrl}
              alt={title}
              className="w-full h-48 sm:h-full object-cover rounded-t-lg sm:rounded-l-lg sm:rounded-t-none"
            />
            {showBookmark && (
              <button
                onClick={onBookmarkToggle}
                className="absolute top-2 right-2 p-2 bg-white/80 hover:bg-white rounded-full transition-colors"
                aria-label={isBookmarked ? t('courses.removeBookmark') : t('courses.bookmark')}
              >
                {isBookmarked ? (
                  <BookmarkSolidIcon className="w-5 h-5 text-blue-600" />
                ) : (
                  <BookmarkIcon className="w-5 h-5 text-gray-600" />
                )}
              </button>
            )}
          </div>

          {/* Content */}
          <div className="flex-1 p-6">
            <div className="flex flex-col h-full">
              {/* Header */}
              <div className="flex-1">
                <div className="flex items-start justify-between mb-2">
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white line-clamp-2">
                    <Link
                      to={`/courses/${course.id}`}
                      className="hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                    >
                      {title}
                    </Link>
                  </h3>
                  {enrollmentStatus && (
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${enrollmentStatus.className}`}>
                      {enrollmentStatus.text}
                    </span>
                  )}
                </div>

                <p className="text-gray-600 dark:text-gray-300 line-clamp-3 mb-4">
                  {description}
                </p>

                {/* Course Info */}
                <div className="flex items-center gap-4 text-sm text-gray-500 dark:text-gray-400 mb-4">
                  <div className="flex items-center gap-1">
                    <ClockIcon className="w-4 h-4" />
                    <span>{formatDuration(course.duration)}</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <PlayIcon className="w-4 h-4" />
                    <span>{course.modules.length} {t('courses.modules')}</span>
                  </div>
                </div>

                {/* Progress Bar (if enrolled) */}
                {enrollment && !isExpired && (
                  <div className="mb-4">
                    <div className="flex items-center justify-between text-sm text-gray-600 dark:text-gray-300 mb-1">
                      <span>{t('courses.progress')}</span>
                      <span>{Math.round(enrollment.progress)}%</span>
                    </div>
                    <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                      <div
                        className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                        style={{ width: `${enrollment.progress}%` }}
                      />
                    </div>
                  </div>
                )}
              </div>

              {/* Action Button */}
              <div className="mt-auto">
                {getActionButton()}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Grid view
  return (
    <div className={`bg-white dark:bg-gray-800 rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200 overflow-hidden ${className}`}>
      {/* Thumbnail */}
      <div className="relative">
        <img
          src={course.thumbnailUrl}
          alt={title}
          className="w-full h-48 object-cover"
        />
        {showBookmark && (
          <button
            onClick={onBookmarkToggle}
            className="absolute top-2 right-2 p-2 bg-white/80 hover:bg-white rounded-full transition-colors"
            aria-label={isBookmarked ? t('courses.removeBookmark') : t('courses.bookmark')}
          >
            {isBookmarked ? (
              <BookmarkSolidIcon className="w-5 h-5 text-blue-600" />
            ) : (
              <BookmarkIcon className="w-5 h-5 text-gray-600" />
            )}
          </button>
        )}
        {enrollmentStatus && (
          <div className="absolute top-2 left-2">
            <span className={`px-2 py-1 text-xs font-medium rounded-full ${enrollmentStatus.className}`}>
              {enrollmentStatus.text}
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div className="p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2 line-clamp-2">
          <Link
            to={`/courses/${course.id}`}
            className="hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
          >
            {title}
          </Link>
        </h3>

        <p className="text-gray-600 dark:text-gray-300 text-sm line-clamp-3 mb-4">
          {description}
        </p>

        {/* Course Info */}
        <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400 mb-4">
          <div className="flex items-center gap-1">
            <ClockIcon className="w-4 h-4" />
            <span>{formatDuration(course.duration)}</span>
          </div>
          <div className="flex items-center gap-1">
            <PlayIcon className="w-4 h-4" />
            <span>{course.modules.length}</span>
          </div>
        </div>

        {/* Progress Bar (if enrolled) */}
        {enrollment && !isExpired && (
          <div className="mb-4">
            <div className="flex items-center justify-between text-sm text-gray-600 dark:text-gray-300 mb-1">
              <span>{t('courses.progress')}</span>
              <span>{Math.round(enrollment.progress)}%</span>
            </div>
            <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${enrollment.progress}%` }}
              />
            </div>
          </div>
        )}

        {/* Action Button */}
        {getActionButton()}
      </div>
    </div>
  );
};

export default CourseCard;