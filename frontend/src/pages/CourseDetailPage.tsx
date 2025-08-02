import React from 'react';
import { useParams } from 'react-router-dom';
import { CourseDetail } from '../components/courses/CourseDetail';
import { SEOHead } from '../components/seo/SEOHead';
import { LoadingSpinner } from '../components/ui/LoadingSpinner';
import { useCourse } from '../hooks/useCourses';
import { useLocalization } from '../hooks/useLocalization';

export const CourseDetailPage: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const { t, language } = useLocalization();
  const { course, loading, error } = useCourse(courseId!);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error || !course) {
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

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const description = language === 'ar' ? course.descriptionAr : course.descriptionEn;

  return (
    <>
      <SEOHead
        title={title}
        description={description}
        keywords={`${title}, autism course, online learning, autism education`}
        image={course.thumbnailUrl}
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="container mx-auto px-4 py-8">
          <CourseDetail course={course} />
        </div>
      </div>
    </>
  );
};

export default CourseDetailPage;