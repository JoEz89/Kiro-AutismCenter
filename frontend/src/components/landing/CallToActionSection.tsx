import React from 'react';
import { useLocalization } from '@/hooks';
import { cn } from '@/lib/utils';

interface CallToActionSectionProps {
  className?: string;
}

export const CallToActionSection: React.FC<CallToActionSectionProps> = ({ className }) => {
  const { t, isRTL } = useLocalization();

  const ctaItems = [
    {
      key: 'products',
      titleKey: 'navigation.products',
      descriptionKey: 'products.description',
      buttonKey: 'common.view',
      href: '/products',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
        </svg>
      ),
      bgColor: 'from-blue-500 to-blue-600',
      hoverColor: 'hover:from-blue-600 hover:to-blue-700'
    },
    {
      key: 'courses',
      titleKey: 'navigation.courses',
      descriptionKey: 'courses.description',
      buttonKey: 'courses.enroll',
      href: '/courses',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
        </svg>
      ),
      bgColor: 'from-green-500 to-green-600',
      hoverColor: 'hover:from-green-600 hover:to-green-700'
    },
    {
      key: 'appointments',
      titleKey: 'navigation.appointments',
      descriptionKey: 'appointments.book',
      buttonKey: 'home.bookAppointment',
      href: '/appointments',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      ),
      bgColor: 'from-purple-500 to-purple-600',
      hoverColor: 'hover:from-purple-600 hover:to-purple-700'
    }
  ];

  return (
    <section 
      className={cn(
        'py-16 px-4 sm:px-6 lg:px-8',
        'bg-white dark:bg-gray-800',
        className
      )}
      aria-labelledby="cta-heading"
    >
      <div className="max-w-7xl mx-auto">
        {/* Section header */}
        <div className="text-center mb-12">
          <h2 
            id="cta-heading"
            className={cn(
              'text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-4',
              isRTL && 'font-arabic'
            )}
          >
            {t('home.services')}
          </h2>
          <p className={cn(
            'text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto',
            isRTL && 'font-arabic'
          )}>
            Explore our comprehensive range of services designed to support individuals with autism and their families
          </p>
        </div>

        {/* CTA Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {ctaItems.map((item) => (
            <div
              key={item.key}
              className={cn(
                'group relative overflow-hidden rounded-2xl',
                'bg-gradient-to-br', item.bgColor,
                'hover:shadow-2xl hover:scale-105',
                'transition-all duration-300 ease-out',
                'focus-within:ring-4 focus-within:ring-blue-300'
              )}
            >
              {/* Background pattern */}
              <div className="absolute inset-0 bg-black bg-opacity-10"></div>
              <div className="absolute top-0 right-0 w-32 h-32 bg-white bg-opacity-10 rounded-full -translate-y-16 translate-x-16"></div>
              <div className="absolute bottom-0 left-0 w-24 h-24 bg-white bg-opacity-10 rounded-full translate-y-12 -translate-x-12"></div>

              {/* Content */}
              <div className="relative p-8 text-white">
                {/* Icon */}
                <div className="mb-6">
                  <div className="w-16 h-16 bg-white bg-opacity-20 rounded-xl flex items-center justify-center">
                    {item.icon}
                  </div>
                </div>

                {/* Title */}
                <h3 className={cn(
                  'text-2xl font-bold mb-4',
                  isRTL && 'font-arabic'
                )}>
                  {t(item.titleKey)}
                </h3>

                {/* Description */}
                <p className={cn(
                  'text-white text-opacity-90 mb-6 leading-relaxed',
                  isRTL && 'font-arabic'
                )}>
                  {t(item.descriptionKey)}
                </p>

                {/* CTA Button */}
                <button
                  className={cn(
                    'inline-flex items-center gap-2',
                    'bg-white text-gray-900 px-6 py-3 rounded-lg font-semibold',
                    'hover:bg-gray-100 transition-colors duration-200',
                    'focus:outline-none focus:ring-2 focus:ring-white focus:ring-offset-2 focus:ring-offset-transparent',
                    'group-hover:shadow-lg',
                    isRTL && 'flex-row-reverse'
                  )}
                  aria-label={`${t(item.buttonKey)} - ${t(item.titleKey)}`}
                >
                  <span className={isRTL ? 'font-arabic' : ''}>
                    {t(item.buttonKey)}
                  </span>
                  <svg 
                    className={cn(
                      'w-5 h-5 transition-transform group-hover:translate-x-1',
                      isRTL && 'rotate-180 group-hover:-translate-x-1'
                    )} 
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                  </svg>
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Bottom CTA */}
        <div className="text-center mt-16">
          <div className={cn(
            'bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-gray-700 dark:to-gray-600',
            'rounded-2xl p-8 sm:p-12'
          )}>
            <h3 className={cn(
              'text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white mb-4',
              isRTL && 'font-arabic'
            )}>
              Ready to get started?
            </h3>
            <p className={cn(
              'text-lg text-gray-600 dark:text-gray-300 mb-8 max-w-2xl mx-auto',
              isRTL && 'font-arabic'
            )}>
              Join thousands of families who have found support and resources through our platform
            </p>
            <div className={cn(
              'flex flex-col sm:flex-row gap-4 justify-center items-center',
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
                  'text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300',
                  'px-8 py-4 rounded-lg font-semibold text-lg',
                  'transition-colors duration-200',
                  'focus:outline-none focus:ring-4 focus:ring-blue-300',
                  'min-w-[200px]',
                  'flex items-center justify-center gap-2',
                  isRTL && 'flex-row-reverse'
                )}
                aria-label={t('home.contactUs')}
              >
                <span className={isRTL ? 'font-arabic' : ''}>
                  {t('home.contactUs')}
                </span>
                <svg 
                  className={cn(
                    'w-5 h-5',
                    isRTL && 'rotate-180'
                  )} 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default CallToActionSection;