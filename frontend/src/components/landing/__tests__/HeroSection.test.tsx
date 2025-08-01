import React from 'react';
import { render, screen } from '@testing-library/react';
import { HeroSection } from '../HeroSection';

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'home.welcome': 'Welcome to Autism Center',
        'home.mission': 'Our mission is to provide comprehensive support and resources for individuals with autism and their families.',
        'home.getStarted': 'Get Started',
        'home.learnMore': 'Learn More',
        'navigation.products': 'Products',
        'navigation.courses': 'Courses',
        'navigation.appointments': 'Appointments',
        'products.description': 'Discover our range of autism-related products and resources',
        'courses.description': 'Learn about autism with our comprehensive online courses'
      };
      return translations[key] || key;
    },
    isRTL: false,
    language: 'en'
  })
}));

// Mock the utils
vi.mock('@/lib/utils', () => ({
  cn: (...classes: any[]) => classes.filter(Boolean).join(' ')
}));

describe('HeroSection', () => {
  it('renders the hero section with main heading', () => {
    render(<HeroSection />);
    
    const heading = screen.getByRole('heading', { level: 1 });
    expect(heading).toBeInTheDocument();
    expect(heading).toHaveTextContent('Welcome to Autism Center');
  });

  it('displays the mission statement', () => {
    render(<HeroSection />);
    
    const mission = screen.getByText(/Our mission is to provide comprehensive support/);
    expect(mission).toBeInTheDocument();
  });

  it('renders call-to-action buttons', () => {
    render(<HeroSection />);
    
    const getStartedButton = screen.getByRole('button', { name: /get started/i });
    const learnMoreButton = screen.getByRole('button', { name: /learn more/i });
    
    expect(getStartedButton).toBeInTheDocument();
    expect(learnMoreButton).toBeInTheDocument();
  });

  it('displays service overview cards', () => {
    render(<HeroSection />);
    
    expect(screen.getByText('Products')).toBeInTheDocument();
    expect(screen.getByText('Courses')).toBeInTheDocument();
    expect(screen.getByText('Appointments')).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<HeroSection />);
    
    const section = screen.getByRole('region');
    expect(section).toHaveAttribute('aria-labelledby', 'hero-heading');
    
    const heading = screen.getByRole('heading', { level: 1 });
    expect(heading).toHaveAttribute('id', 'hero-heading');
  });

  it('applies custom className when provided', () => {
    const { container } = render(<HeroSection className="custom-class" />);
    
    const section = container.querySelector('section');
    expect(section).toHaveClass('custom-class');
  });

  it('has proper button accessibility labels', () => {
    render(<HeroSection />);
    
    const getStartedButton = screen.getByRole('button', { name: 'Get Started' });
    const learnMoreButton = screen.getByRole('button', { name: 'Learn More' });
    
    expect(getStartedButton).toHaveAttribute('aria-label', 'Get Started');
    expect(learnMoreButton).toHaveAttribute('aria-label', 'Learn More');
  });
});