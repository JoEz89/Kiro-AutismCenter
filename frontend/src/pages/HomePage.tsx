import React from 'react';
import { Navigation, HeroSection, TestimonialsSection, CallToActionSection } from '@/components';

const HomePage: React.FC = () => {
  return (
    <div className="min-h-screen">
      {/* Navigation */}
      <Navigation />
      
      {/* Main content */}
      <main>
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
