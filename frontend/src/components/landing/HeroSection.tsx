import React from 'react';
import { useLocalization } from '@/hooks';
import { cn } from '@/lib/utils';

interface HeroSectionProps {
  className?: string;
}

export const HeroSection: React.FC<HeroSectionProps> = ({ className }) => {
  const { t, isRTL } = useLocalization();

  return (
    <section 
      className={cn(
        'relative bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800',
        'py-20 px-4 sm:px-6 lg:px-8',
        className
      )}
      aria-labelledby="hero-heading"
    >
      <div className="max-w-7xl mx-auto">
        <div className="text-center">
          {/* Main heading */}
          <h1 
            id="hero-heading"
            className={cn(
              'text-4xl sm:text-5xl lg:text-6xl font-bold',
              'text-gray-900 dark:text-white',
              'mb-6 leading-tight',
              isRTL && 'font-arabic'
            )}
          >
            {t('home.welcome')}
          </h1>
          
          {/* Mission statement */}
          <p className={cn(
            'text-xl sm:text-2xl text-gray-600 dark:text-gray-300',
            'mb-8 max-w-4xl mx-auto leading-relaxed',
            isRTL && 'font-arabic'
          )}>
            {t('home.mission')}
          </p>

          {/* Call-to-action buttons */}
          <div className={cn(
            'flex flex-col sm:flex-row gap-4 justify-center items-center',
            'mb-12',
            isRTL && 'sm:flex-row-reverse'
          )}>
            <button
              className={cn(
                'bg-blue-600 hover:bg-blue-700 text-white',
                'px-8 py-4 rounded-lg font-semibold text-lg',
                'transition-all duration-200 transform hover:scale-105',
                'focus:outline-none focus:ring-4 focus:ring-blue-300',
                'shadow-lg hover:shadow-xl',
                'min-w-[200px]'
              )}
              aria-label={t('home.getStarted')}
            >
              {t('home.getStarted')}
            </button>
            
            <button
              className={cn(
                'border-2 border-blue-600 text-blue-600 hover:bg-blue-600 hover:text-white',
                'px-8 py-4 rounded-lg font-semibold text-lg',
                'transition-all duration-200',
                'focus:outline-none focus:ring-4 focus:ring-blue-300',
                'min-w-[200px]'
              )}
              aria-label={t('home.learnMore')}
            >
              {t('home.learnMore')}
            </button>
          </div>

          {/* Services overview */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mt-16">
            <div className={cn(
              'bg-white dark:bg-gray-800 p-6 rounded-xl shadow-md',
              'hover:shadow-lg transition-shadow duration-200',
              'border border-gray-200 dark:border-gray-700'
            )}>
              <div className="w-12 h-12 bg-blue-100 dark:bg-blue-900 rounded-lg flex items-center justify-center mb-4 mx-auto">
                <svg className="w-6 h-6 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
              </div>
              <h3 className={cn(
                'text-xl font-semibold mb-3 text-gray-900 dark:text-white',
                isRTL && 'font-arabic'
              )}>
                {t('navigation.products')}
              </h3>
              <p className={cn(
                'text-gray-600 dark:text-gray-300',
                isRTL && 'font-arabic'
              )}>
                {t('products.description')}
              </p>
            </div>

            <div className={cn(
              'bg-white dark:bg-gray-800 p-6 rounded-xl shadow-md',
              'hover:shadow-lg transition-shadow duration-200',
              'border border-gray-200 dark:border-gray-700'
            )}>
              <div className="w-12 h-12 bg-green-100 dark:bg-green-900 rounded-lg flex items-center justify-center mb-4 mx-auto">
                <svg className="w-6 h-6 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                </svg>
              </div>
              <h3 className={cn(
                'text-xl font-semibold mb-3 text-gray-900 dark:text-white',
                isRTL && 'font-arabic'
              )}>
                {t('navigation.courses')}
              </h3>
              <p className={cn(
                'text-gray-600 dark:text-gray-300',
                isRTL && 'font-arabic'
              )}>
                {t('courses.description')}
              </p>
            </div>

            <div className={cn(
              'bg-white dark:bg-gray-800 p-6 rounded-xl shadow-md',
              'hover:shadow-lg transition-shadow duration-200',
              'border border-gray-200 dark:border-gray-700'
            )}>
              <div className="w-12 h-12 bg-purple-100 dark:bg-purple-900 rounded-lg flex items-center justify-center mb-4 mx-auto">
                <svg className="w-6 h-6 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <h3 className={cn(
                'text-xl font-semibold mb-3 text-gray-900 dark:text-white',
                isRTL && 'font-arabic'
              )}>
                {t('navigation.appointments')}
              </h3>
              <p className={cn(
                'text-gray-600 dark:text-gray-300',
                isRTL && 'font-arabic'
              )}>
                Schedule consultations with autism specialists
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Background decoration */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute -top-40 -right-40 w-80 h-80 bg-blue-200 dark:bg-blue-800 rounded-full opacity-20 blur-3xl"></div>
        <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-indigo-200 dark:bg-indigo-800 rounded-full opacity-20 blur-3xl"></div>
      </div>
    </section>
  );
};

export default HeroSection;