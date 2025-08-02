import React, { useState, useEffect } from 'react';
import { useLocalization, useKeyboardNavigation } from '@/hooks';
import { cn } from '@/lib/utils';

interface Testimonial {
  id: string;
  nameEn: string;
  nameAr: string;
  roleEn: string;
  roleAr: string;
  contentEn: string;
  contentAr: string;
  rating: number;
  avatar?: string;
}

interface TestimonialsSectionProps {
  className?: string;
}

export const TestimonialsSection: React.FC<TestimonialsSectionProps> = ({ className }) => {
  const { t, isRTL, language } = useLocalization();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isAutoPlaying, setIsAutoPlaying] = useState(true);

  // Sample testimonials data
  const testimonials: Testimonial[] = [
    {
      id: '1',
      nameEn: 'Sarah Johnson',
      nameAr: 'سارة جونسون',
      roleEn: 'Mother of Alex',
      roleAr: 'والدة أليكس',
      contentEn: 'The Autism Center has been a lifeline for our family. The courses helped me understand my son better, and the products they recommend have made a real difference in his daily routine.',
      contentAr: 'كان مركز التوحد شريان حياة لعائلتنا. ساعدتني الدورات على فهم ابني بشكل أفضل، والمنتجات التي يوصون بها أحدثت فرقاً حقيقياً في روتينه اليومي.',
      rating: 5
    },
    {
      id: '2',
      nameEn: 'Ahmed Al-Rashid',
      nameAr: 'أحمد الراشد',
      roleEn: 'Father of Layla',
      roleAr: 'والد ليلى',
      contentEn: 'The online appointments with specialists have been incredibly convenient. The doctors are knowledgeable and caring, and the video sessions work perfectly.',
      contentAr: 'كانت المواعيد عبر الإنترنت مع المختصين مريحة بشكل لا يصدق. الأطباء مطلعون ومهتمون، وجلسات الفيديو تعمل بشكل مثالي.',
      rating: 5
    },
    {
      id: '3',
      nameEn: 'Maria Rodriguez',
      nameAr: 'ماريا رودريغيز',
      roleEn: 'Special Education Teacher',
      roleAr: 'معلمة تربية خاصة',
      contentEn: 'I recommend the Autism Center courses to all my colleagues. The content is evidence-based and practical, making it easy to apply in real classroom situations.',
      contentAr: 'أوصي بدورات مركز التوحد لجميع زملائي. المحتوى قائم على الأدلة وعملي، مما يجعل من السهل تطبيقه في مواقف الفصل الدراسي الحقيقية.',
      rating: 5
    }
  ];

  // Auto-advance carousel
  useEffect(() => {
    if (!isAutoPlaying) return;

    const interval = setInterval(() => {
      setCurrentIndex((prevIndex) => 
        prevIndex === testimonials.length - 1 ? 0 : prevIndex + 1
      );
    }, 5000);

    return () => clearInterval(interval);
  }, [isAutoPlaying, testimonials.length]);

  const goToSlide = (index: number) => {
    setCurrentIndex(index);
    setIsAutoPlaying(false);
    // Resume auto-play after 10 seconds
    setTimeout(() => setIsAutoPlaying(true), 10000);
  };

  const goToPrevious = () => {
    const newIndex = currentIndex === 0 ? testimonials.length - 1 : currentIndex - 1;
    goToSlide(newIndex);
  };

  const goToNext = () => {
    const newIndex = currentIndex === testimonials.length - 1 ? 0 : currentIndex + 1;
    goToSlide(newIndex);
  };

  // Keyboard navigation
  useKeyboardNavigation({
    onArrowLeft: goToPrevious,
    onArrowRight: goToNext,
    onHome: () => goToSlide(0),
    onEnd: () => goToSlide(testimonials.length - 1),
    enabled: true
  });

  const currentTestimonial = testimonials[currentIndex];

  const renderStars = (rating: number) => {
    return Array.from({ length: 5 }, (_, index) => (
      <svg
        key={index}
        className={cn(
          'w-5 h-5',
          index < rating ? 'text-yellow-400' : 'text-gray-300 dark:text-gray-600'
        )}
        fill="currentColor"
        viewBox="0 0 20 20"
        aria-hidden="true"
      >
        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
      </svg>
    ));
  };

  return (
    <section 
      className={cn(
        'py-16 px-4 sm:px-6 lg:px-8',
        'bg-gray-50 dark:bg-gray-900',
        className
      )}
      aria-labelledby="testimonials-heading"
    >
      <div className="max-w-7xl mx-auto">
        {/* Section header */}
        <div className="text-center mb-12">
          <h2 
            id="testimonials-heading"
            className={cn(
              'text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-4',
              isRTL && 'font-arabic'
            )}
          >
            {t('home.testimonials')}
          </h2>
          <p className={cn(
            'text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto',
            isRTL && 'font-arabic'
          )}>
            Hear from families and professionals who have benefited from our services
          </p>
        </div>

        {/* Testimonial carousel */}
        <div className="relative">
          <div 
            className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-8 sm:p-12 mx-auto max-w-4xl"
            role="region"
            aria-label="Testimonials carousel"
            aria-live="polite"
          >
            {/* Quote icon */}
            <div className="flex justify-center mb-6">
              <svg 
                className="w-12 h-12 text-blue-600 dark:text-blue-400" 
                fill="currentColor" 
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path d="M14.017 21v-7.391c0-5.704 3.731-9.57 8.983-10.609l.995 2.151c-2.432.917-3.995 3.638-3.995 5.849h4v10h-9.983zm-14.017 0v-7.391c0-5.704 3.748-9.57 9-10.609l.996 2.151c-2.433.917-3.996 3.638-3.996 5.849h3.983v10h-9.983z" />
              </svg>
            </div>

            {/* Testimonial content */}
            <blockquote className="text-center">
              <p className={cn(
                'text-xl sm:text-2xl text-gray-700 dark:text-gray-300 mb-8 leading-relaxed',
                isRTL && 'font-arabic'
              )}>
                "{language === 'ar' ? currentTestimonial.contentAr : currentTestimonial.contentEn}"
              </p>
              
              {/* Rating */}
              <div className="flex justify-center mb-6" aria-label={`Rating: ${currentTestimonial.rating} out of 5 stars`}>
                {renderStars(currentTestimonial.rating)}
              </div>

              {/* Author info */}
              <footer>
                <div className="flex items-center justify-center">
                  <div className="text-center">
                    <cite className={cn(
                      'text-lg font-semibold text-gray-900 dark:text-white not-italic',
                      isRTL && 'font-arabic'
                    )}>
                      {language === 'ar' ? currentTestimonial.nameAr : currentTestimonial.nameEn}
                    </cite>
                    <p className={cn(
                      'text-gray-600 dark:text-gray-400 mt-1',
                      isRTL && 'font-arabic'
                    )}>
                      {language === 'ar' ? currentTestimonial.roleAr : currentTestimonial.roleEn}
                    </p>
                  </div>
                </div>
              </footer>
            </blockquote>
          </div>

          {/* Navigation buttons */}
          <button
            onClick={goToPrevious}
            className={cn(
              'absolute top-1/2 -translate-y-1/2',
              isRTL ? 'right-4' : 'left-4',
              'bg-white dark:bg-gray-800 shadow-lg rounded-full p-3',
              'hover:bg-gray-50 dark:hover:bg-gray-700',
              'focus:outline-none focus:ring-4 focus:ring-blue-300',
              'transition-all duration-200'
            )}
            aria-label="Previous testimonial"
          >
            <svg 
              className={cn(
                'w-6 h-6 text-gray-600 dark:text-gray-300',
                isRTL && 'rotate-180'
              )} 
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>

          <button
            onClick={goToNext}
            className={cn(
              'absolute top-1/2 -translate-y-1/2',
              isRTL ? 'left-4' : 'right-4',
              'bg-white dark:bg-gray-800 shadow-lg rounded-full p-3',
              'hover:bg-gray-50 dark:hover:bg-gray-700',
              'focus:outline-none focus:ring-4 focus:ring-blue-300',
              'transition-all duration-200'
            )}
            aria-label="Next testimonial"
          >
            <svg 
              className={cn(
                'w-6 h-6 text-gray-600 dark:text-gray-300',
                isRTL && 'rotate-180'
              )} 
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>

        {/* Carousel indicators */}
        <div className="flex justify-center mt-8 space-x-2 rtl:space-x-reverse">
          {testimonials.map((_, index) => (
            <button
              key={index}
              onClick={() => goToSlide(index)}
              className={cn(
                'w-3 h-3 rounded-full transition-all duration-200',
                'focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2',
                index === currentIndex
                  ? 'bg-blue-600 dark:bg-blue-400'
                  : 'bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500'
              )}
              aria-label={`Go to testimonial ${index + 1}`}
              aria-current={index === currentIndex ? 'true' : 'false'}
            />
          ))}
        </div>
      </div>
    </section>
  );
};

export default TestimonialsSection;