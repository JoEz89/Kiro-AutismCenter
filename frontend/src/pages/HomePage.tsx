import React from 'react';
import { Navigation, HeroSection, TestimonialsSection, CallToActionSection, SEOHead, ScreenReaderOnly } from '@/components';
import { useLocalization } from '@/hooks';

const HomePage: React.FC = () => {
  const { t } = useLocalization();

  return (
    <div className="min-h-screen">
      {/* SEO Head */}
      <SEOHead
        title={t('home.welcome')}
        description={t('home.mission')}
        type="website"
      />
      
      {/* Navigation */}
      <Navigation id="navigation" />
      
      {/* Main content */}
      <main id="main-content" tabIndex={-1}>
        {/* Screen reader announcement */}
        <ScreenReaderOnly>
          <h1>{t('home.welcome')} - {t('accessibility.mainContent', 'Main content')}</h1>
        </ScreenReaderOnly>
        
        {/* Hero Section */}
        <HeroSection />
        
        {/* Call to Action Section */}
        <CallToActionSection />
        
        {/* Testimonials Section */}
        <TestimonialsSection />
      </main>
    </div>
  );
};

export default HomePage;
