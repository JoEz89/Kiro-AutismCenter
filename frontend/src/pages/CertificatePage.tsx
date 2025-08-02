import React from 'react';
import { useParams, Navigate } from 'react-router-dom';
import { Certificate } from '../components/courses/Certificate';
import { SEOHead } from '../components/seo/SEOHead';
import { LoadingSpinner } from '../components/ui/LoadingSpinner';
import { useCourse, useEnrollments } from '../hooks/useCourses';
import { useAuth } from '../hooks/useAuth';
import { useLocalization } from '../hooks/useLocalization';

export const CertificatePage: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const { t, language } = useLocalization();
  const { isAuthenticated } = useAuth();
  const { course, loading: courseLoading, error: courseError } = useCourse(courseId!);
  const { enrollments, loading: enrollmentsLoading } = useEnrollments();

  // Redirect if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ returnTo: `/courses/${courseId}/certificate` }} replace />;
  }

  if (courseLoading || enrollmentsLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (courseError || !course) {
    return (
      <>
        <SEOHead
          title={t('courses.courseNotFound')}
          description={t('courses.courseNotFoundDescription')}
        />
        
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              {t('courses.courseNotFound')}
            </h1>
            <p className="text-gray-600 dark:text-gray-300 mb-6">
              {t('courses.courseNotFoundDescription')}
            </p>
            <a
              href="/courses"
              className="btn btn-primary"
            >
              {t('common.back')}
            </a>
          </div>
        </div>
      </>
    );
  }

  // Find the user's enrollment for this course
  const enrollment = enrollments.find(e => e.courseId === course.id);

  if (!enrollment) {
    return (
      <>
        <SEOHead
          title={t('courses.enrollmentRequired')}
          description={t('courses.accessDeniedDescription')}
        />
        
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              {t('courses.enrollmentRequired')}
            </h1>
            <p className="text-gray-600 dark:text-gray-300 mb-6">
              {t('courses.accessDeniedDescription')}
            </p>
            <a
              href={`/courses/${courseId}`}
              className="btn btn-primary"
            >
              {t('courses.viewCourse')}
            </a>
          </div>
        </div>
      </>
    );
  }

  const title = language === 'ar' ? course.titleAr : course.titleEn;

  return (
    <>
      <SEOHead
        title={t('courses.certificateTitle', { course: title })}
        description={t('courses.certificateDescription', { course: title })}
        keywords={`${title}, certificate, completion certificate, autism education`}
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="container mx-auto px-4 py-8">
          <Certificate
            course={course}
            enrollment={enrollment}
          />
        </div>
      </div>
    </>
  );
};

export default CertificatePage;