import React, { useState, useEffect } from 'react';
import {
  CheckCircleIcon,
  ClockIcon,
  AcademicCapIcon,
  TrophyIcon,
} from '@heroicons/react/24/outline';
import { CheckCircleIcon as CheckCircleSolidIcon } from '@heroicons/react/24/solid';
import type { Course, Enrollment } from '../../types';
import { useLocalization } from '../../hooks/useLocalization';
import { useCourseProgress } from '../../hooks/useCourses';
import { formatDuration, formatDate } from '../../utils';
import { ProgressBar } from '../ui/ProgressBar';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { courseService } from '../../services/courseService';

interface CourseCompletionTrackerProps {
  course: Course;
  enrollment: Enrollment;
  onCompletionChange?: (isCompleted: boolean) => void;
  className?: string;
}

export const CourseCompletionTracker: React.FC<CourseCompletionTrackerProps> = ({
  course,
  enrollment,
  onCompletionChange,
  className = '',
}) => {
  const { t, language } = useLocalization();
  const { progress, loading, error, updateLessonProgress } = useCourseProgress(enrollment.id);
  const [isGeneratingCertificate, setIsGeneratingCertificate] = useState(false);
  const [completionAnimation, setCompletionAnimation] = useState(false);

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const isCompleted = enrollment.completionDate !== undefined;
  const isExpired = new Date(enrollment.expiryDate) < new Date();

  // Check if course was just completed
  useEffect(() => {
    if (progress && progress.progress >= 100 && !isCompleted) {
      setCompletionAnimation(true);
      setTimeout(() => setCompletionAnimation(false), 3000);
      onCompletionChange?.(true);
    }
  }, [progress, isCompleted, onCompletionChange]);

  const handleGenerateCertificate = async () => {
    if (!isCompleted || enrollment.certificateUrl) return;

    setIsGeneratingCertificate(true);
    try {
      const response = await courseService.generateCertificate(enrollment.id);
      if (response.success) {
        // Update enrollment with certificate URL
        // This would typically trigger a refetch of enrollment data
        console.log('Certificate generated:', response.data.certificateUrl);
      }
    } catch (error) {
      console.error('Failed to generate certificate:', error);
    } finally {
      setIsGeneratingCertificate(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error || !progress) {
    return (
      <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <p className="text-red-800 dark:text-red-200">
          {error || t('courses.progressLoadError')}
        </p>
      </div>
    );
  }

  const progressPercentage = Math.round(progress.progress);
  const completedModules = progress.completedModules.length;
  const totalModules = progress.totalModules;

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Completion Animation */}
      {completionAnimation && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-8 text-center max-w-md mx-4 animate-bounce">
            <TrophyIcon className="w-16 h-16 text-yellow-500 mx-auto mb-4" />
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
              {t('courses.congratulations')}
            </h2>
            <p className="text-gray-600 dark:text-gray-300">
              {t('courses.courseCompletedMessage')}
            </p>
          </div>
        </div>
      )}

      {/* Progress Overview */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
            {t('courses.courseProgress')}
          </h3>
          {isCompleted && (
            <div className="flex items-center gap-2 text-green-600 dark:text-green-400">
              <CheckCircleSolidIcon className="w-5 h-5" />
              <span className="font-medium">{t('courses.completed')}</span>
            </div>
          )}
        </div>

        <div className="space-y-4">
          {/* Progress Bar */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {t('courses.overallProgress')}
              </span>
              <span className="text-sm text-gray-600 dark:text-gray-400">
                {progressPercentage}%
              </span>
            </div>
            <ProgressBar 
              progress={progressPercentage} 
              color={isCompleted ? 'green' : 'blue'}
              size="lg"
            />
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <div className="text-center">
              <div className="text-2xl font-bold text-blue-600 dark:text-blue-400 mb-1">
                {completedModules}
              </div>
              <div className="text-xs text-gray-600 dark:text-gray-300">
                {t('courses.completedModules')}
              </div>
            </div>
            
            <div className="text-center">
              <div className="text-2xl font-bold text-purple-600 dark:text-purple-400 mb-1">
                {totalModules}
              </div>
              <div className="text-xs text-gray-600 dark:text-gray-300">
                {t('courses.totalModules')}
              </div>
            </div>
            
            <div className="text-center">
              <div className="text-2xl font-bold text-orange-600 dark:text-orange-400 mb-1">
                {Math.floor(progress.timeSpent / 60)}h
              </div>
              <div className="text-xs text-gray-600 dark:text-gray-300">
                {t('courses.timeSpent')}
              </div>
            </div>
            
            <div className="text-center">
              <div className="text-2xl font-bold text-green-600 dark:text-green-400 mb-1">
                {isCompleted ? '100%' : `${progressPercentage}%`}
              </div>
              <div className="text-xs text-gray-600 dark:text-gray-300">
                {t('courses.completion')}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Module Progress */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border border-gray-200 dark:border-gray-700">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          {t('courses.moduleProgress')}
        </h3>
        
        <div className="space-y-3">
          {course.modules.map((module, index) => {
            const isModuleCompleted = progress.completedModules.includes(module.id);
            const moduleTitle = language === 'ar' ? module.titleAr : module.titleEn;

            return (
              <div
                key={module.id}
                className="flex items-center gap-4 p-3 rounded-lg bg-gray-50 dark:bg-gray-700"
              >
                <div className={`flex items-center justify-center w-8 h-8 rounded-full ${
                  isModuleCompleted
                    ? 'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-400'
                    : 'bg-gray-200 dark:bg-gray-600 text-gray-600 dark:text-gray-400'
                }`}>
                  {isModuleCompleted ? (
                    <CheckCircleIcon className="w-5 h-5" />
                  ) : (
                    <span className="text-sm font-medium">{index + 1}</span>
                  )}
                </div>
                
                <div className="flex-1">
                  <h4 className="font-medium text-gray-900 dark:text-white">
                    {moduleTitle}
                  </h4>
                  <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-300">
                    <ClockIcon className="w-4 h-4" />
                    <span>{formatDuration(module.duration)}</span>
                  </div>
                </div>
                
                {isModuleCompleted && (
                  <div className="text-sm text-green-600 dark:text-green-400 font-medium">
                    {t('courses.completed')}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Completion Status */}
      {isCompleted ? (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-6">
          <div className="flex items-center gap-4">
            <div className="flex items-center justify-center w-12 h-12 bg-green-100 dark:bg-green-900 rounded-full">
              <AcademicCapIcon className="w-6 h-6 text-green-600 dark:text-green-400" />
            </div>
            
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-green-900 dark:text-green-100 mb-1">
                {t('courses.courseCompleted')}
              </h3>
              <p className="text-green-800 dark:text-green-200">
                {t('courses.completedOn')}: {enrollment.completionDate && formatDate(enrollment.completionDate)}
              </p>
            </div>
            
            <div className="flex flex-col gap-2">
              {enrollment.certificateUrl ? (
                <a
                  href={enrollment.certificateUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-primary"
                >
                  <AcademicCapIcon className="w-5 h-5 mr-2" />
                  {t('courses.viewCertificate')}
                </a>
              ) : (
                <button
                  onClick={handleGenerateCertificate}
                  disabled={isGeneratingCertificate}
                  className="btn btn-primary"
                >
                  {isGeneratingCertificate ? (
                    <>
                      <LoadingSpinner size="sm" className="mr-2" />
                      {t('courses.generating')}
                    </>
                  ) : (
                    <>
                      <AcademicCapIcon className="w-5 h-5 mr-2" />
                      {t('courses.generateCertificate')}
                    </>
                  )}
                </button>
              )}
            </div>
          </div>
        </div>
      ) : (
        <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-6">
          <div className="flex items-center gap-4">
            <div className="flex items-center justify-center w-12 h-12 bg-blue-100 dark:bg-blue-900 rounded-full">
              <ClockIcon className="w-6 h-6 text-blue-600 dark:text-blue-400" />
            </div>
            
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-blue-900 dark:text-blue-100 mb-1">
                {t('courses.keepLearning')}
              </h3>
              <p className="text-blue-800 dark:text-blue-200">
                {t('courses.progressMessage', { 
                  completed: completedModules, 
                  total: totalModules,
                  percentage: progressPercentage 
                })}
              </p>
            </div>
            
            {!isExpired && (
              <div className="text-sm text-blue-600 dark:text-blue-400">
                {t('courses.daysRemaining', { 
                  days: Math.ceil((new Date(enrollment.expiryDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
                })}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default CourseCompletionTracker;