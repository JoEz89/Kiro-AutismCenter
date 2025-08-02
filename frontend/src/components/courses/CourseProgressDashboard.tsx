import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  ChartBarIcon,
  ClockIcon,
  BookmarkIcon,
  AcademicCapIcon,
  PlayIcon,
  CheckCircleIcon,
  CalendarIcon,
  ArrowRightIcon,
} from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkSolidIcon } from '@heroicons/react/24/solid';
import type { Enrollment, Course } from '../../types';
import { useLocalization } from '../../hooks/useLocalization';
import { useCourseProgress } from '../../hooks/useCourses';
import { formatDuration, formatDate } from '../../utils';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { ProgressBar } from '../ui/ProgressBar';

interface CourseProgressDashboardProps {
  enrollment: Enrollment;
  course: Course;
  className?: string;
}

export const CourseProgressDashboard: React.FC<CourseProgressDashboardProps> = ({
  enrollment,
  course,
  className = '',
}) => {
  const { t, language } = useLocalization();
  const { progress, loading, error } = useCourseProgress(enrollment.id);
  const [activeTab, setActiveTab] = useState<'overview' | 'modules' | 'bookmarks'>('overview');

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const isCompleted = enrollment.completionDate !== undefined;
  const isExpired = new Date(enrollment.expiryDate) < new Date();

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error || !progress) {
    return (
      <div className="text-center py-8">
        <p className="text-red-600 dark:text-red-400">{error || t('courses.progressLoadError')}</p>
      </div>
    );
  }

  const completedModules = progress.completedModules.length;
  const totalModules = progress.totalModules;
  const progressPercentage = Math.round(progress.progress);

  const tabs = [
    { id: 'overview', label: t('courses.overallProgress'), icon: ChartBarIcon },
    { id: 'modules', label: t('courses.modules'), icon: PlayIcon },
    { id: 'bookmarks', label: t('courses.myBookmarks'), icon: BookmarkIcon },
  ] as const;

  return (
    <div className={`max-w-6xl mx-auto ${className}`}>
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-4 mb-4">
          <img
            src={course.thumbnailUrl}
            alt={title}
            className="w-16 h-16 object-cover rounded-lg"
          />
          <div className="flex-1">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
              {title}
            </h1>
            <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-300">
              <span className="flex items-center gap-1">
                <CalendarIcon className="w-4 h-4" />
                {t('courses.enrolledOn')}: {formatDate(enrollment.enrollmentDate)}
              </span>
              {!isExpired && (
                <span className="flex items-center gap-1">
                  <ClockIcon className="w-4 h-4" />
                  {t('courses.enrollmentExpiresOn', { date: formatDate(enrollment.expiryDate) })}
                </span>
              )}
            </div>
          </div>
          {isCompleted && enrollment.certificateUrl && (
            <Link
              to={`/courses/${course.id}/certificate`}
              className="btn btn-primary"
            >
              <AcademicCapIcon className="w-5 h-5 mr-2" />
              {t('courses.viewCertificate')}
            </Link>
          )}
        </div>

        {/* Progress Overview */}
        <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600 dark:text-blue-400 mb-1">
                {progressPercentage}%
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-300">
                {t('courses.overallProgress')}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-green-600 dark:text-green-400 mb-1">
                {completedModules}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-300">
                {t('courses.completedLessons', { completed: completedModules, total: totalModules })}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-purple-600 dark:text-purple-400 mb-1">
                {Math.floor(progress.timeSpent / 60)}h
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-300">
                {t('courses.timeSpent')}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-orange-600 dark:text-orange-400 mb-1">
                {isCompleted ? t('courses.completed') : t('courses.inProgress')}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-300">
                {isCompleted && enrollment.completionDate
                  ? formatDate(enrollment.completionDate)
                  : formatDate(progress.lastAccessedAt)
                }
              </div>
            </div>
          </div>
          
          <div className="mt-6">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {t('courses.courseProgress')}
              </span>
              <span className="text-sm text-gray-600 dark:text-gray-400">
                {progressPercentage}%
              </span>
            </div>
            <ProgressBar progress={progressPercentage} className="h-3" />
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200 dark:border-gray-700 mb-6">
        <nav className="flex space-x-8">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-2 py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                  isActive
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
                }`}
              >
                <Icon className="w-5 h-5" />
                {tab.label}
              </button>
            );
          })}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="space-y-6">
        {activeTab === 'overview' && (
          <OverviewTab
            enrollment={enrollment}
            course={course}
            progress={progress}
          />
        )}
        
        {activeTab === 'modules' && (
          <ModulesTab
            enrollment={enrollment}
            course={course}
            progress={progress}
          />
        )}
        
        {activeTab === 'bookmarks' && (
          <BookmarksTab
            enrollment={enrollment}
            course={course}
          />
        )}
      </div>
    </div>
  );
};

// Overview Tab Component
const OverviewTab: React.FC<{
  enrollment: Enrollment;
  course: Course;
  progress: any;
}> = ({ enrollment, course, progress }) => {
  const { t, language } = useLocalization();
  const isCompleted = enrollment.completionDate !== undefined;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Course Statistics */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          {t('courses.courseDetails')}
        </h3>
        <div className="space-y-3">
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-300">{t('courses.duration')}:</span>
            <span className="font-medium text-gray-900 dark:text-white">
              {formatDuration(course.duration)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-300">{t('courses.modules')}:</span>
            <span className="font-medium text-gray-900 dark:text-white">
              {course.modules.length}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-300">{t('courses.timeSpent')}:</span>
            <span className="font-medium text-gray-900 dark:text-white">
              {formatDuration(progress.timeSpent)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-300">{t('courses.progress')}:</span>
            <span className="font-medium text-gray-900 dark:text-white">
              {Math.round(progress.progress)}%
            </span>
          </div>
        </div>
      </div>

      {/* Next Steps */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          {isCompleted ? t('courses.courseCompleted') : t('courses.nextSteps')}
        </h3>
        
        {isCompleted ? (
          <div className="space-y-4">
            <div className="flex items-center gap-3 text-green-600 dark:text-green-400">
              <CheckCircleIcon className="w-6 h-6" />
              <span className="font-medium">{t('courses.congratulations')}</span>
            </div>
            <p className="text-gray-600 dark:text-gray-300">
              {t('courses.courseCompletedMessage')}
            </p>
            {enrollment.certificateUrl && (
              <Link
                to={`/courses/${course.id}/certificate`}
                className="btn btn-primary w-full"
              >
                <AcademicCapIcon className="w-5 h-5 mr-2" />
                {t('courses.downloadCertificate')}
              </Link>
            )}
          </div>
        ) : (
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-300">
              {t('courses.continueProgressMessage')}
            </p>
            <Link
              to={`/courses/${course.id}/learn`}
              className="btn btn-primary w-full"
            >
              <PlayIcon className="w-5 h-5 mr-2" />
              {t('courses.continueCourse')}
              <ArrowRightIcon className="w-5 h-5 ml-2" />
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};

// Modules Tab Component
const ModulesTab: React.FC<{
  enrollment: Enrollment;
  course: Course;
  progress: any;
}> = ({ enrollment, course, progress }) => {
  const { t, language } = useLocalization();

  return (
    <div className="space-y-4">
      {course.modules.map((module, index) => {
        const isCompleted = progress.completedModules.includes(module.id);
        const moduleTitle = language === 'ar' ? module.titleAr : module.titleEn;

        return (
          <div
            key={module.id}
            className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700"
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <div className={`flex items-center justify-center w-10 h-10 rounded-full ${
                  isCompleted
                    ? 'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-400'
                    : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400'
                }`}>
                  {isCompleted ? (
                    <CheckCircleIcon className="w-6 h-6" />
                  ) : (
                    <span className="font-medium">{index + 1}</span>
                  )}
                </div>
                <div>
                  <h3 className="font-medium text-gray-900 dark:text-white">
                    {moduleTitle}
                  </h3>
                  <p className="text-sm text-gray-600 dark:text-gray-300">
                    {formatDuration(module.duration)}
                  </p>
                </div>
              </div>
              
              <div className="flex items-center gap-3">
                {isCompleted && (
                  <span className="text-sm text-green-600 dark:text-green-400 font-medium">
                    {t('courses.completed')}
                  </span>
                )}
                <Link
                  to={`/courses/${course.id}/learn?module=${module.id}`}
                  className="btn btn-sm btn-outline"
                >
                  {isCompleted ? t('courses.review') : t('courses.start')}
                </Link>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
};

// Bookmarks Tab Component
const BookmarksTab: React.FC<{
  enrollment: Enrollment;
  course: Course;
}> = ({ enrollment, course }) => {
  const { t } = useLocalization();
  const [bookmarks, setBookmarks] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // This would be implemented with actual bookmark service calls
  React.useEffect(() => {
    // Simulate loading bookmarks
    setTimeout(() => {
      setBookmarks([]);
      setLoading(false);
    }, 1000);
  }, [enrollment.id]);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (bookmarks.length === 0) {
    return (
      <div className="text-center py-12">
        <BookmarkIcon className="w-16 h-16 text-gray-400 dark:text-gray-600 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
          {t('courses.noBookmarks')}
        </h3>
        <p className="text-gray-600 dark:text-gray-300 mb-6">
          {t('courses.noBookmarksDescription')}
        </p>
        <Link
          to={`/courses/${course.id}/learn`}
          className="btn btn-primary"
        >
          {t('courses.startLearning')}
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {bookmarks.map((bookmark) => (
        <div
          key={bookmark.moduleId}
          className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <BookmarkSolidIcon className="w-6 h-6 text-blue-600 dark:text-blue-400" />
              <div>
                <h3 className="font-medium text-gray-900 dark:text-white">
                  {bookmark.moduleTitle}
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-300">
                  {t('courses.bookmarkedOn', { date: formatDate(bookmark.bookmarkedAt) })}
                </p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              <button className="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-200">
                {t('courses.removeBookmark')}
              </button>
              <Link
                to={`/courses/${course.id}/learn?module=${bookmark.moduleId}`}
                className="btn btn-sm btn-primary"
              >
                {t('courses.goToLesson')}
              </Link>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default CourseProgressDashboard;