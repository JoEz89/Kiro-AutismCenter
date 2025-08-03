import React, { useState, useEffect } from 'react';
import { useParams, useSearchParams, Navigate, Link } from 'react-router-dom';
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  BookmarkIcon,
  CheckCircleIcon,
  ClockIcon,
  ArrowLeftIcon,
  ListBulletIcon,
} from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkSolidIcon } from '@heroicons/react/24/solid';
import { VideoPlayer } from '../components/courses/VideoPlayer';
import { SEOHead } from '../components/seo/SEOHead';
import { LoadingSpinner } from '../components/ui/LoadingSpinner';
import { useCourse, useEnrollments, useCourseProgress } from '../hooks/useCourses';
import { useAuth } from '../hooks/useAuth';
import { useLocalization } from '../hooks/useLocalization';
import { useBookmarks } from '../hooks/useBookmarks';
import { courseService } from '../services/courseService';
import { formatDuration } from '../utils';
import type { CourseModule } from '../types';

export const CourseLearningPage: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const { t, language } = useLocalization();
  const { isAuthenticated } = useAuth();
  const { course, loading: courseLoading, error: courseError } = useCourse(courseId!);
  const { enrollments, loading: enrollmentsLoading } = useEnrollments();
  
  const [showModuleList, setShowModuleList] = useState(false);
  const [currentModuleIndex, setCurrentModuleIndex] = useState(0);
  const [videoProgress, setVideoProgress] = useState<Record<string, number>>({});

  // Find enrollment
  const enrollment = enrollments.find(e => e.courseId === courseId);
  const { progress, updateLessonProgress } = useCourseProgress(enrollment?.id || '');
  const { bookmarks, addBookmark, removeBookmark, isBookmarked } = useBookmarks(enrollment?.id || '');

  // Redirect if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ returnTo: `/courses/${courseId}/learn` }} replace />;
  }

  // Handle loading states
  if (courseLoading || enrollmentsLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  // Handle errors
  if (courseError || !course) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('courses.courseNotFound')}
          </h1>
          <Link to="/courses" className="btn btn-primary">
            {t('common.back')}
          </Link>
        </div>
      </div>
    );
  }

  // Check enrollment
  if (!enrollment) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('courses.enrollmentRequired')}
          </h1>
          <p className="text-gray-600 dark:text-gray-300 mb-6">
            {t('courses.accessDeniedDescription')}
          </p>
          <Link to={`/courses/${courseId}`} className="btn btn-primary">
            {t('courses.viewCourse')}
          </Link>
        </div>
      </div>
    );
  }

  // Check if enrollment is expired
  const isExpired = new Date(enrollment.expiryDate) < new Date();
  if (isExpired) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('courses.enrollmentExpired')}
          </h1>
          <p className="text-gray-600 dark:text-gray-300 mb-6">
            {t('courses.enrollmentExpiredDescription')}
          </p>
          <Link to={`/courses/${courseId}`} className="btn btn-primary">
            {t('courses.viewCourse')}
          </Link>
        </div>
      </div>
    );
  }

  // Initialize current module from URL params or default to first
  useEffect(() => {
    const moduleParam = searchParams.get('module');
    if (moduleParam && course) {
      const moduleIndex = course.modules.findIndex(m => m.id === moduleParam);
      if (moduleIndex !== -1) {
        setCurrentModuleIndex(moduleIndex);
      }
    }
  }, [searchParams, course]);

  // Update URL when module changes
  const handleModuleChange = (index: number) => {
    setCurrentModuleIndex(index);
    const module = course.modules[index];
    if (module) {
      setSearchParams({ module: module.id });
    }
  };

  const currentModule = course.modules[currentModuleIndex];
  const isFirstModule = currentModuleIndex === 0;
  const isLastModule = currentModuleIndex === course.modules.length - 1;

  const handleVideoProgress = async (progressPercent: number) => {
    if (!currentModule) return;

    setVideoProgress(prev => ({
      ...prev,
      [currentModule.id]: progressPercent,
    }));

    // Mark as completed when 95% watched
    if (progressPercent >= 95 && progress) {
      const isCompleted = progress.completedModules.includes(currentModule.id);
      if (!isCompleted) {
        try {
          await updateLessonProgress(currentModule.id, true);
        } catch (error) {
          console.error('Failed to update progress:', error);
        }
      }
    }
  };

  const handleVideoComplete = async () => {
    if (!currentModule || !progress) return;

    const isCompleted = progress.completedModules.includes(currentModule.id);
    if (!isCompleted) {
      try {
        await updateLessonProgress(currentModule.id, true);
      } catch (error) {
        console.error('Failed to mark as completed:', error);
      }
    }
  };

  const handleBookmarkToggle = async () => {
    if (!currentModule) return;

    try {
      if (isBookmarked(currentModule.id)) {
        await removeBookmark(currentModule.id);
      } else {
        await addBookmark(currentModule.id);
      }
    } catch (error) {
      console.error('Failed to toggle bookmark:', error);
    }
  };

  const getModuleStatus = (module: CourseModule) => {
    if (!progress) return 'not-started';
    return progress.completedModules.includes(module.id) ? 'completed' : 'in-progress';
  };

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const moduleTitle = currentModule ? (language === 'ar' ? currentModule.titleAr : currentModule.titleEn) : '';

  return (
    <>
      <SEOHead
        title={t('courses.learningTitle', { course: title, module: moduleTitle })}
        description={t('courses.learningDescription', { course: title })}
        keywords={`${title}, online learning, video course, autism education`}
      />

      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        {/* Header */}
        <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
          <div className="container mx-auto px-4 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <Link
                  to={`/courses/${courseId}`}
                  className="flex items-center gap-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors"
                >
                  <ArrowLeftIcon className="w-4 h-4" />
                  {t('common.back')}
                </Link>
                
                <div className="h-6 w-px bg-gray-300 dark:bg-gray-600" />
                
                <div>
                  <h1 className="text-lg font-semibold text-gray-900 dark:text-white truncate">
                    {title}
                  </h1>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    {t('courses.moduleProgress', { 
                      current: currentModuleIndex + 1, 
                      total: course.modules.length 
                    })}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                {/* Progress */}
                {progress && (
                  <div className="hidden sm:flex items-center gap-2 text-sm text-gray-600 dark:text-gray-300">
                    <span>{Math.round(progress.progress)}% {t('courses.complete')}</span>
                    <div className="w-24 h-2 bg-gray-200 dark:bg-gray-700 rounded-full">
                      <div
                        className="h-full bg-blue-500 rounded-full transition-all duration-300"
                        style={{ width: `${progress.progress}%` }}
                      />
                    </div>
                  </div>
                )}

                {/* Module List Toggle */}
                <button
                  onClick={() => setShowModuleList(!showModuleList)}
                  className="btn btn-outline btn-sm"
                  aria-label={t('courses.toggleModuleList')}
                >
                  <ListBulletIcon className="w-4 h-4" />
                  <span className="hidden sm:inline ml-2">{t('courses.modules')}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="flex">
          {/* Module List Sidebar */}
          <div className={`${
            showModuleList ? 'w-80' : 'w-0'
          } transition-all duration-300 overflow-hidden bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700`}>
            <div className="p-4">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                {t('courses.courseContent')}
              </h2>
              
              <div className="space-y-2">
                {course.modules.map((module, index) => {
                  const status = getModuleStatus(module);
                  const isCurrent = index === currentModuleIndex;
                  const moduleTitle = language === 'ar' ? module.titleAr : module.titleEn;
                  
                  return (
                    <button
                      key={module.id}
                      onClick={() => handleModuleChange(index)}
                      className={`w-full text-left p-3 rounded-lg border transition-colors ${
                        isCurrent
                          ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800'
                          : 'bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-600'
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <div className={`flex items-center justify-center w-6 h-6 rounded-full text-xs font-medium mt-0.5 ${
                          status === 'completed'
                            ? 'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-400'
                            : isCurrent
                            ? 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400'
                            : 'bg-gray-200 dark:bg-gray-600 text-gray-600 dark:text-gray-300'
                        }`}>
                          {status === 'completed' ? (
                            <CheckCircleIcon className="w-4 h-4" />
                          ) : (
                            index + 1
                          )}
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <h3 className={`font-medium text-sm truncate ${
                            isCurrent
                              ? 'text-blue-900 dark:text-blue-100'
                              : 'text-gray-900 dark:text-white'
                          }`}>
                            {moduleTitle}
                          </h3>
                          <div className="flex items-center gap-2 mt-1">
                            <ClockIcon className="w-3 h-3 text-gray-400" />
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              {formatDuration(module.duration)}
                            </span>
                            {isBookmarked(module.id) && (
                              <BookmarkSolidIcon className="w-3 h-3 text-yellow-500" />
                            )}
                          </div>
                        </div>
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Main Content */}
          <div className="flex-1 min-w-0">
            <div className="container mx-auto px-4 py-6">
              {currentModule && (
                <div className="max-w-4xl mx-auto">
                  {/* Module Header */}
                  <div className="mb-6">
                    <div className="flex items-start justify-between mb-4">
                      <div>
                        <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                          {moduleTitle}
                        </h2>
                        <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-300">
                          <div className="flex items-center gap-1">
                            <ClockIcon className="w-4 h-4" />
                            <span>{formatDuration(currentModule.duration)}</span>
                          </div>
                          <span>
                            {t('courses.moduleNumber', { number: currentModuleIndex + 1 })}
                          </span>
                        </div>
                      </div>

                      <button
                        onClick={handleBookmarkToggle}
                        className={`p-2 rounded-lg transition-colors ${
                          isBookmarked(currentModule.id)
                            ? 'bg-yellow-100 dark:bg-yellow-900/20 text-yellow-600 dark:text-yellow-400'
                            : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                        }`}
                        aria-label={
                          isBookmarked(currentModule.id)
                            ? t('courses.removeBookmark')
                            : t('courses.addBookmark')
                        }
                      >
                        {isBookmarked(currentModule.id) ? (
                          <BookmarkSolidIcon className="w-5 h-5" />
                        ) : (
                          <BookmarkIcon className="w-5 h-5" />
                        )}
                      </button>
                    </div>

                    {/* Video Progress */}
                    {videoProgress[currentModule.id] > 0 && (
                      <div className="mb-4">
                        <div className="flex items-center justify-between text-sm text-gray-600 dark:text-gray-300 mb-1">
                          <span>{t('courses.videoProgress')}</span>
                          <span>{Math.round(videoProgress[currentModule.id])}%</span>
                        </div>
                        <div className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-full">
                          <div
                            className="h-full bg-blue-500 rounded-full transition-all duration-300"
                            style={{ width: `${videoProgress[currentModule.id]}%` }}
                          />
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Video Player */}
                  <div className="mb-8">
                    <VideoPlayer
                      enrollmentId={enrollment.id}
                      moduleId={currentModule.id}
                      onProgress={handleVideoProgress}
                      onComplete={handleVideoComplete}
                      className="w-full"
                    />
                  </div>

                  {/* Navigation */}
                  <div className="flex items-center justify-between">
                    <button
                      onClick={() => handleModuleChange(currentModuleIndex - 1)}
                      disabled={isFirstModule}
                      className="btn btn-outline disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <ChevronLeftIcon className="w-4 h-4 mr-2" />
                      {t('courses.previousModule')}
                    </button>

                    <div className="text-center">
                      <p className="text-sm text-gray-600 dark:text-gray-300">
                        {t('courses.moduleProgress', { 
                          current: currentModuleIndex + 1, 
                          total: course.modules.length 
                        })}
                      </p>
                    </div>

                    <button
                      onClick={() => handleModuleChange(currentModuleIndex + 1)}
                      disabled={isLastModule}
                      className="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {t('courses.nextModule')}
                      <ChevronRightIcon className="w-4 h-4 ml-2" />
                    </button>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default CourseLearningPage;