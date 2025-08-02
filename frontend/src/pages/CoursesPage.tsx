import React from 'react';
import { CourseCatalog } from '../components/courses/CourseCatalog';
import { SEOHead } from '../components/seo/SEOHead';
import { useLocalization } from '../hooks/useLocalization';

export const CoursesPage: React.FC = () => {
  const { t } = useLocalization();

  return (
    <>
      <SEOHead
        title={t('courses.title')}
        description={t('courses.description')}
        keywords="autism courses, online learning, autism education, professional development"
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="container mx-auto px-4 py-8">
          <CourseCatalog />
        </div>
      </div>
    </>
  );
};

export default CoursesPage;