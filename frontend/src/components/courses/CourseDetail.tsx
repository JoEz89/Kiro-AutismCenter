import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  ClockIcon,
  PlayIcon,
  CheckCircleIcon,
  CalendarIcon,
  UserIcon,
  LanguageIcon,
  ArrowLeftIcon,
} from '@heroicons/react/24/outline';
import { PlayIcon as PlaySolidIcon } from '@heroicons/react/24/solid';
import type { Course } from '../../types';
import { useLocalization } from '../../hooks/useLocalization';
import { useAuth } from '../../hooks/useAuth';
import { useEnrollments } from '../../hooks/useCourses';
import { formatDuration, formatDate } from '../../utils';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { Modal } from '../ui/Modal';

interface CourseDetailProps {
  course: Course;
  className?: string;
}

export const CourseDetail: React.FC<CourseDetailProps> = ({
  course,
  className = '',
}) => {
  const { t, language } = useLocalization();
  const { user, isAuthenticated } = useAuth();
  const { enrollments, enrollInCourse } = useEnrollments();
  const navigate = useNavigate();
  
  const [isEnrolling, setIsEnrolling] = useState(false);
  const [showPreview, setShowPreview] = useState(false);
  const [enrollmentError, setEnrollmentError] = useState<string | null>(null);

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const description = language === 'ar' ? course.descriptionAr : course.descriptionEn;

  // Check if user is enrolled
  const enrollment = enrollments.find(e => e.courseId === course.id);
  const isEnrolled = !!enrollment;
  const isExpired = enrollment && new Date(enrollment.expiryDate) < new Date();

  const handleEnroll = async () => {
    if (!isAuthenticated) {
      navigate('/login', { state: { returnTo: `/courses/${course.id}` } });
      return;
    }

    try {
      setIsEnrolling(true);
      setEnrollmentError(null);
      
      await enrollInCourse(course.id);
      
      // Navigate to course learning page
      navigate(`/courses/${course.id}/learn`);
    } catch (error) {
      setEnrollmentError(error instanceof Error ? error.message : t('courses.enrollmentError'));
    } finally {
      setIsEnrolling(false);
    }
  };

  const getActionButton = () => {
    if (!isAuthenticated) {
      return (
        <button
          onClick={handleEnroll}
          className="btn btn-primary btn-lg w-full sm:w-auto"
        >
          {t('courses.enroll')}
        </button>
      );
    }

    if (!isEnrolled) {
      return (
        <button
          onClick={handleEnroll}
          disabled={isEnrolling}
          className="btn btn-primary btn-lg w-full sm:w-auto"
        >
          {isEnrolling ? (
            <>
              <LoadingSpinner size="sm" className="mr-2" />
              {t('common.loading')}
            </>
          ) : (
            t('courses.enroll')
          )}
        </button>
      );
    }

    if (isExpired) {
      return (
        <button
          disabled
          className="btn btn-secondary btn-lg w-full sm:w-auto opacity-50 cursor-not-allowed"
        >
          {t('courses.enrollmentExpired')}
        </button>
      );
    }

    if (enrollment?.progress === 0) {
      return (
        <Link
          to={`/courses/${course.id}/learn`}
          className="btn btn-primary btn-lg w-full sm:w-auto"
        >
          {t('courses.startCourse')}
        </Link>
      );
    }

    return (
      <Link
        to={`/courses/${course.id}/learn`}
        className="btn btn-primary btn-lg w-full sm:w-auto"
      >
        {t('courses.continueCourse')}
      </Link>
    );
  };

  const getEnrollmentInfo = () => {
    if (!enrollment) return null;

    return (
      <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
        <div className="flex items-center gap-2 mb-2">
          <CheckCircleIcon className="w-5 h-5 text-blue-600 dark:text-blue-400" />
          <span className="font-medium text-blue-900 dark:text-blue-100">
            {t('courses.enrolled')}
          </span>
        </div>
        
        <div className="text-sm text-blue-800 dark:text-blue-200 space-y-1">
          <p>
            {t('courses.enrolledOn')}: {formatDate(enrollment.enrollmentDate)}
          </p>
          <p>
            {t('courses.enrollmentExpiresOn', { date: formatDate(enrollment.expiryDate) })}
          </p>
          {enrollment.progress > 0 && (
            <p>
              {t('courses.progress')}: {Math.round(enrollment.progress)}%
            </p>
          )}
          {enrollment.completionDate && (
            <p>
              {t('courses.completedOn')}: {formatDate(enrollment.completionDate)}
            </p>
          )}
        </div>

        {enrollment && !isExpired && enrollment.progress > 0 && (
          <div className="mt-3">
            <div className="w-full bg-blue-200 dark:bg-blue-800 rounded-full h-2">
              <div
                className="bg-blue-600 dark:bg-blue-400 h-2 rounded-full transition-all duration-300"
                style={{ width: `${enrollment.progress}%` }}
              />
            </div>
          </div>
        )}
      </div>
    );
  };

  return (
    <div className={`max-w-4xl mx-auto ${className}`}>
      {/* Back Button */}
      <div className="mb-6">
        <Link
          to="/courses"
          className="inline-flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200 transition-colors"
        >
          <ArrowLeftIcon className="w-4 h-4" />
          {t('common.back')}
        </Link>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Course Header */}
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              {title}
            </h1>
            <p className="text-lg text-gray-600 dark:text-gray-300 leading-relaxed">
              {description}
            </p>
          </div>

          {/* Course Stats */}
          <div className="flex flex-wrap items-center gap-6 text-sm text-gray-600 dark:text-gray-300">
            <div className="flex items-center gap-2">
              <ClockIcon className="w-5 h-5" />
              <span>{formatDuration(course.duration)}</span>
            </div>
            <div className="flex items-center gap-2">
              <PlayIcon className="w-5 h-5" />
              <span>{course.modules.length} {t('courses.modules')}</span>
            </div>
            <div className="flex items-center gap-2">
              <LanguageIcon className="w-5 h-5" />
              <span>{language === 'ar' ? 'العربية' : 'English'}</span>
            </div>
          </div>

          {/* Enrollment Status */}
          {getEnrollmentInfo()}

          {/* Error Message */}
          {enrollmentError && (
            <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
              <p className="text-red-800 dark:text-red-200">{enrollmentError}</p>
            </div>
          )}

          {/* Course Overview */}
          <div>
            <h2 className="text-2xl font-semibold text-gray-900 dark:text-white mb-4">
              {t('courses.courseOverview')}
            </h2>
            <div className="prose dark:prose-invert max-w-none">
              <p>{description}</p>
            </div>
          </div>

          {/* What You'll Learn */}
          <div>
            <h2 className="text-2xl font-semibold text-gray-900 dark:text-white mb-4">
              {t('courses.whatYouWillLearn')}
            </h2>
            <ul className="space-y-2">
              {course.modules.slice(0, 5).map((module, index) => (
                <li key={module.id} className="flex items-start gap-3">
                  <CheckCircleIcon className="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5 flex-shrink-0" />
                  <span className="text-gray-700 dark:text-gray-300">
                    {language === 'ar' ? module.titleAr : module.titleEn}
                  </span>
                </li>
              ))}
            </ul>
          </div>

          {/* Course Content */}
          <div>
            <h2 className="text-2xl font-semibold text-gray-900 dark:text-white mb-4">
              {t('courses.courseContent')}
            </h2>
            <div className="space-y-3">
              {course.modules.map((module, index) => (
                <div
                  key={module.id}
                  className="border border-gray-200 dark:border-gray-700 rounded-lg p-4"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="flex items-center justify-center w-8 h-8 bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400 rounded-full text-sm font-medium">
                        {index + 1}
                      </div>
                      <div>
                        <h3 className="font-medium text-gray-900 dark:text-white">
                          {language === 'ar' ? module.titleAr : module.titleEn}
                        </h3>
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {formatDuration(module.duration)}
                        </p>
                      </div>
                    </div>
                    {index === 0 && (
                      <button
                        onClick={() => setShowPreview(true)}
                        className="flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200 text-sm font-medium"
                      >
                        <PlaySolidIcon className="w-4 h-4" />
                        {t('courses.watchPreview')}
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Course Thumbnail */}
          <div className="relative">
            <img
              src={course.thumbnailUrl}
              alt={title}
              className="w-full h-48 object-cover rounded-lg"
            />
            <button
              onClick={() => setShowPreview(true)}
              className="absolute inset-0 flex items-center justify-center bg-black/50 hover:bg-black/60 transition-colors rounded-lg group"
            >
              <div className="flex items-center justify-center w-16 h-16 bg-white/20 rounded-full group-hover:bg-white/30 transition-colors">
                <PlaySolidIcon className="w-8 h-8 text-white ml-1" />
              </div>
            </button>
          </div>

          {/* Action Button */}
          <div>
            {getActionButton()}
          </div>

          {/* Course Includes */}
          <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-4">
            <h3 className="font-semibold text-gray-900 dark:text-white mb-3">
              {t('courses.courseIncludes')}
            </h3>
            <ul className="space-y-2 text-sm">
              <li className="flex items-center gap-2">
                <PlayIcon className="w-4 h-4 text-gray-500" />
                <span className="text-gray-700 dark:text-gray-300">
                  {t('courses.videoContent', { hours: Math.floor(course.duration / 60) })}
                </span>
              </li>
              <li className="flex items-center gap-2">
                <CheckCircleIcon className="w-4 h-4 text-gray-500" />
                <span className="text-gray-700 dark:text-gray-300">
                  {t('courses.certificateOfCompletion')}
                </span>
              </li>
              <li className="flex items-center gap-2">
                <CalendarIcon className="w-4 h-4 text-gray-500" />
                <span className="text-gray-700 dark:text-gray-300">
                  {t('courses.lifetimeAccess')}
                </span>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Preview Modal */}
      {showPreview && (
        <Modal
          isOpen={showPreview}
          onClose={() => setShowPreview(false)}
          title={t('courses.previewVideo')}
          size="lg"
        >
          <div className="aspect-video bg-gray-900 rounded-lg flex items-center justify-center">
            <div className="text-white text-center">
              <PlaySolidIcon className="w-16 h-16 mx-auto mb-4 opacity-50" />
              <p className="text-lg">{t('courses.previewVideo')}</p>
              <p className="text-sm opacity-75 mt-2">
                {course.modules[0] && (language === 'ar' ? course.modules[0].titleAr : course.modules[0].titleEn)}
              </p>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};

export default CourseDetail;