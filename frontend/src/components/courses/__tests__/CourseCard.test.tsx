import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { CourseCard } from '../CourseCard';
import { useLocalization } from '../../../hooks/useLocalization';
import { useEnrollments } from '../../../hooks/useCourses';
import type { Course } from '../../../types';

// Mock the hooks
jest.mock('../../../hooks/useLocalization');
jest.mock('../../../hooks/useCourses');

const mockUseLocalization = useLocalization as jest.MockedFunction<typeof useLocalization>;
const mockUseEnrollments = useEnrollments as jest.MockedFunction<typeof useEnrollments>;

const mockCourse: Course = {
  id: '1',
  titleEn: 'Introduction to Autism',
  titleAr: 'مقدمة في التوحد',
  descriptionEn: 'Learn the basics of autism spectrum disorders',
  descriptionAr: 'تعلم أساسيات اضطرابات طيف التوحد',
  duration: 120,
  thumbnailUrl: '/images/course1.jpg',
  modules: [
    {
      id: '1',
      titleEn: 'Module 1: Understanding Autism',
      titleAr: 'الوحدة 1: فهم التوحد',
      videoUrl: '/videos/module1.mp4',
      duration: 60,
      order: 1,
    },
    {
      id: '2',
      titleEn: 'Module 2: Communication Strategies',
      titleAr: 'الوحدة 2: استراتيجيات التواصل',
      videoUrl: '/videos/module2.mp4',
      duration: 60,
      order: 2,
    },
  ],
  isActive: true,
};

const mockT = (key: string, params?: any) => {
  const translations: Record<string, string> = {
    'courses.enroll': 'Enroll Now',
    'courses.enrolled': 'Enrolled',
    'courses.completed': 'Completed',
    'courses.inProgress': 'In Progress',
    'courses.enrollmentExpired': 'Enrollment Expired',
    'courses.startCourse': 'Start Course',
    'courses.continueCourse': 'Continue Course',
    'courses.modules': 'modules',
    'courses.progress': 'Progress',
    'courses.bookmark': 'Bookmark',
    'courses.removeBookmark': 'Remove Bookmark',
  };
  return translations[key] || key;
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('CourseCard', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      language: 'en',
      direction: 'ltr',
      setLanguage: jest.fn(),
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('renders course information correctly', () => {
    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    expect(screen.getByText('Learn the basics of autism spectrum disorders')).toBeInTheDocument();
    expect(screen.getByText('2h')).toBeInTheDocument(); // Duration formatted
    expect(screen.getByText('2')).toBeInTheDocument(); // Module count
    expect(screen.getByText('Enroll Now')).toBeInTheDocument();
  });

  it('renders in Arabic when language is set to Arabic', () => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      language: 'ar',
      direction: 'rtl',
      setLanguage: jest.fn(),
    });

    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('مقدمة في التوحد')).toBeInTheDocument();
    expect(screen.getByText('تعلم أساسيات اضطرابات طيف التوحد')).toBeInTheDocument();
  });

  it('shows enrolled status when user is enrolled', () => {
    const mockEnrollment = {
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date('2024-01-01'),
      expiryDate: new Date('2024-02-01'),
      progress: 50,
      completionDate: undefined,
      certificateUrl: undefined,
    };

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('In Progress')).toBeInTheDocument();
    expect(screen.getByText('Continue Course')).toBeInTheDocument();
    expect(screen.getByText('50%')).toBeInTheDocument(); // Progress percentage
  });

  it('shows completed status when course is completed', () => {
    const mockEnrollment = {
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date('2024-01-01'),
      expiryDate: new Date('2024-02-01'),
      progress: 100,
      completionDate: new Date('2024-01-15'),
      certificateUrl: '/certificates/cert1.pdf',
    };

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('Completed')).toBeInTheDocument();
    expect(screen.getByText('Continue Course')).toBeInTheDocument();
  });

  it('shows expired status when enrollment is expired', () => {
    const mockEnrollment = {
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date('2023-01-01'),
      expiryDate: new Date('2023-02-01'), // Expired
      progress: 30,
      completionDate: undefined,
      certificateUrl: undefined,
    };

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('Enrollment Expired')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /enrollment expired/i })).toBeDisabled();
  });

  it('shows start course button for enrolled but not started course', () => {
    const mockEnrollment = {
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date('2024-01-01'),
      expiryDate: new Date('2024-02-01'),
      progress: 0, // Not started
      completionDate: undefined,
      certificateUrl: undefined,
    };

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCard course={mockCourse} />);

    expect(screen.getByText('Start Course')).toBeInTheDocument();
  });

  it('renders in list view mode', () => {
    const { container } = renderWithRouter(
      <CourseCard course={mockCourse} viewMode="list" />
    );

    // List view should have different layout classes
    expect(container.querySelector('.flex-col.sm\\:flex-row')).toBeInTheDocument();
  });

  it('handles bookmark functionality', () => {
    const mockOnBookmarkToggle = jest.fn();

    renderWithRouter(
      <CourseCard
        course={mockCourse}
        showBookmark={true}
        isBookmarked={false}
        onBookmarkToggle={mockOnBookmarkToggle}
      />
    );

    const bookmarkButton = screen.getByLabelText('Bookmark');
    fireEvent.click(bookmarkButton);

    expect(mockOnBookmarkToggle).toHaveBeenCalled();
  });

  it('shows bookmarked state correctly', () => {
    renderWithRouter(
      <CourseCard
        course={mockCourse}
        showBookmark={true}
        isBookmarked={true}
        onBookmarkToggle={jest.fn()}
      />
    );

    expect(screen.getByLabelText('Remove Bookmark')).toBeInTheDocument();
  });

  it('has correct links to course pages', () => {
    renderWithRouter(<CourseCard course={mockCourse} />);

    const courseLinks = screen.getAllByRole('link');
    const enrollLink = courseLinks.find(link => 
      link.getAttribute('href') === '/courses/1'
    );
    
    expect(enrollLink).toBeInTheDocument();
  });

  it('displays course thumbnail with correct alt text', () => {
    renderWithRouter(<CourseCard course={mockCourse} />);

    const thumbnail = screen.getByAltText('Introduction to Autism');
    expect(thumbnail).toBeInTheDocument();
    expect(thumbnail).toHaveAttribute('src', '/images/course1.jpg');
  });
});