import React from 'react';
import { render, screen } from '@testing-library/react';
import { CallToActionSection } from '../CallToActionSection';

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'home.services': 'Our Services',
        'navigation.products': 'Products',
        'navigation.courses': 'Courses',
        'navigation.appointments': 'Appointments',
        'products.description': 'Discover our range of autism-related products and resources',
        'courses.description': 'Learn about autism with our comprehensive online courses',
        'appointments.book': 'Book Appointment',
        'common.view': 'View',
        'courses.enroll': 'Enroll Now',
        'home.bookAppointment': 'Book Appointment',
        'home.getStarted': 'Get Started',
        'home.contactUs': 'Contact Us'
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

describe('CallToActionSection', () => {
  it('renders the section with main heading', () => {
    render(<CallToActionSection />);
    
    const heading = screen.getByRole('heading', { level: 2 });
    expect(heading).toBeInTheDocument();
    expect(heading).toHaveTextContent('Our Services');
  });

  it('displays all three service cards', () => {
    render(<CallToActionSection />);
    
    expect(screen.getByText('Products')).toBeInTheDocument();
    expect(screen.getByText('Courses')).toBeInTheDocument();
    expect(screen.getByText('Appointments')).toBeInTheDocument();
  });

  it('renders service descriptions', () => {
    render(<CallToActionSection />);
    
    expect(screen.getByText('Discover our range of autism-related products and resources')).toBeInTheDocument();
    expect(screen.getByText('Learn about autism with our comprehensive online courses')).toBeInTheDocument();
    expect(screen.getAllByText('Book Appointment')).toHaveLength(2); // One in description, one in button
  });

  it('displays CTA buttons for each service', () => {
    render(<CallToActionSection />);
    
    const viewButton = screen.getByRole('button', { name: /view - products/i });
    const enrollButton = screen.getByRole('button', { name: /enroll now - courses/i });
    const bookButton = screen.getByRole('button', { name: /book appointment - appointments/i });
    
    expect(viewButton).toBeInTheDocument();
    expect(enrollButton).toBeInTheDocument();
    expect(bookButton).toBeInTheDocument();
  });

  it('renders the bottom CTA section', () => {
    render(<CallToActionSection />);
    
    expect(screen.getByText('Ready to get started?')).toBeInTheDocument();
    expect(screen.getByText(/Join thousands of families/)).toBeInTheDocument();
  });

  it('displays bottom CTA buttons', () => {
    render(<CallToActionSection />);
    
    const getStartedButtons = screen.getAllByRole('button', { name: /get started/i });
    const contactButton = screen.getByRole('button', { name: /contact us/i });
    
    expect(getStartedButtons).toHaveLength(1); // Only one in this component
    expect(contactButton).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<CallToActionSection />);
    
    const section = screen.getByRole('region');
    expect(section).toHaveAttribute('aria-labelledby', 'cta-heading');
    
    const heading = screen.getByRole('heading', { level: 2 });
    expect(heading).toHaveAttribute('id', 'cta-heading');
  });

  it('applies custom className when provided', () => {
    const { container } = render(<CallToActionSection className="custom-class" />);
    
    const section = container.querySelector('section');
    expect(section).toHaveClass('custom-class');
  });

  it('renders service icons', () => {
    const { container } = render(<CallToActionSection />);
    
    // Check that SVG icons are present in the DOM
    const svgElements = container.querySelectorAll('svg');
    expect(svgElements.length).toBeGreaterThan(0);
  });

  it('has proper button focus states', () => {
    render(<CallToActionSection />);
    
    const buttons = screen.getAllByRole('button');
    
    buttons.forEach(button => {
      expect(button).toHaveClass('focus:outline-none');
    });
  });

  it('displays proper hover effects on service cards', () => {
    const { container } = render(<CallToActionSection />);
    
    const serviceCards = container.querySelectorAll('.group');
    expect(serviceCards).toHaveLength(3);
    
    serviceCards.forEach(card => {
      expect(card).toHaveClass('hover:shadow-2xl', 'hover:scale-105');
    });
  });
});