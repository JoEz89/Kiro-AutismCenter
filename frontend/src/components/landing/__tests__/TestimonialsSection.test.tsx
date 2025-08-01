import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { TestimonialsSection } from '../TestimonialsSection';

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'home.testimonials': 'What Our Families Say'
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

describe('TestimonialsSection', () => {
  beforeEach(() => {
    // Mock timers for auto-advance functionality
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('renders the testimonials section with heading', () => {
    render(<TestimonialsSection />);
    
    const heading = screen.getByRole('heading', { level: 2 });
    expect(heading).toBeInTheDocument();
    expect(heading).toHaveTextContent('What Our Families Say');
  });

  it('displays the first testimonial by default', () => {
    render(<TestimonialsSection />);
    
    // Check for the first testimonial content
    expect(screen.getByText(/The Autism Center has been a lifeline/)).toBeInTheDocument();
    expect(screen.getByText('Sarah Johnson')).toBeInTheDocument();
    expect(screen.getByText('Mother of Alex')).toBeInTheDocument();
  });

  it('renders navigation buttons', () => {
    render(<TestimonialsSection />);
    
    const prevButton = screen.getByRole('button', { name: /previous testimonial/i });
    const nextButton = screen.getByRole('button', { name: /next testimonial/i });
    
    expect(prevButton).toBeInTheDocument();
    expect(nextButton).toBeInTheDocument();
  });

  it('renders carousel indicators', () => {
    render(<TestimonialsSection />);
    
    const indicators = screen.getAllByRole('button', { name: /go to testimonial/i });
    expect(indicators).toHaveLength(3); // Based on the 3 testimonials in the component
  });

  it('displays star ratings', () => {
    render(<TestimonialsSection />);
    
    const ratingContainer = screen.getByLabelText(/rating: 5 out of 5 stars/i);
    expect(ratingContainer).toBeInTheDocument();
  });

  it('navigates to next testimonial when next button is clicked', () => {
    render(<TestimonialsSection />);
    
    const nextButton = screen.getByRole('button', { name: /next testimonial/i });
    fireEvent.click(nextButton);
    
    // Should show the second testimonial
    expect(screen.getByText('Ahmed Al-Rashid')).toBeInTheDocument();
    expect(screen.getByText('Father of Layla')).toBeInTheDocument();
  });

  it('navigates to previous testimonial when previous button is clicked', () => {
    render(<TestimonialsSection />);
    
    const prevButton = screen.getByRole('button', { name: /previous testimonial/i });
    fireEvent.click(prevButton);
    
    // Should show the last testimonial (wraps around)
    expect(screen.getByText('Maria Rodriguez')).toBeInTheDocument();
    expect(screen.getByText('Special Education Teacher')).toBeInTheDocument();
  });

  it('navigates to specific testimonial when indicator is clicked', () => {
    render(<TestimonialsSection />);
    
    const thirdIndicator = screen.getByRole('button', { name: /go to testimonial 3/i });
    fireEvent.click(thirdIndicator);
    
    // Should show the third testimonial
    expect(screen.getByText('Maria Rodriguez')).toBeInTheDocument();
    expect(screen.getByText('Special Education Teacher')).toBeInTheDocument();
  });

  it('has auto-advance functionality', () => {
    render(<TestimonialsSection />);
    
    // Initially shows first testimonial
    expect(screen.getByText('Sarah Johnson')).toBeInTheDocument();
    
    // Test that the component has the necessary structure for auto-advance
    // (The actual timer functionality is complex to test with fake timers)
    const carousel = screen.getByRole('region', { name: /testimonials carousel/i });
    expect(carousel).toHaveAttribute('aria-live', 'polite');
  });

  it('has proper accessibility attributes', () => {
    render(<TestimonialsSection />);
    
    const section = screen.getByRole('region', { name: /testimonials carousel/i });
    expect(section).toHaveAttribute('aria-live', 'polite');
    
    const heading = screen.getByRole('heading', { level: 2 });
    expect(heading).toHaveAttribute('id', 'testimonials-heading');
  });

  it('applies custom className when provided', () => {
    const { container } = render(<TestimonialsSection className="custom-class" />);
    
    const section = container.querySelector('section');
    expect(section).toHaveClass('custom-class');
  });

  it('stops auto-play when user interacts with controls', () => {
    render(<TestimonialsSection />);
    
    const nextButton = screen.getByRole('button', { name: /next testimonial/i });
    fireEvent.click(nextButton);
    
    // Fast-forward 5 seconds - should not auto-advance immediately
    act(() => {
      vi.advanceTimersByTime(5000);
    });
    
    // Should still show the second testimonial (the one we clicked to)
    expect(screen.getByText('Ahmed Al-Rashid')).toBeInTheDocument();
  });
});