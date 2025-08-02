import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { CourseDetail } from '../CourseDetail';
import { useLocalization } from '../../../hooks/useLocalization';
import { useAuth } from '../../../hooks/useAuth';
import { useEnrollments } from '../../../hooks/useCourses';
import type { Course } from '../../../types';

// Mock the hooks
jest.mock('../../../hooks/useLocalization');
jest.mock('../../../hooks/useAuth');
jest.mock('../../../hooks/useCourses');
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => jest.fn(),
}));

const mockUseLocalization = useLocalization as jest.MockedFunction<typeof useLocalization>;
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;
const mockUseEnrollments = useEnrollments as jest.MockedFunction<typeof useEnrollments>;

const mockCourse: Course = {
  id: '1',
  titleEn: 'Introduction to Autism',
  titleAr: 'مقدمة في التوحد',
  descriptionEn: 'Learn the basics of autism spectrum disorders and effective intervention strategies.',
  descriptionAr: 'تعلم أساسيات اضطرابات طيف التوحد واستراتيجيات التدخل الفعالة.',
  duration: 180,
  thumbnailUrl: '/images/course1.jpg',
  modules: [
    {
      id: '1',
      titleEn: 'Understanding Autism Spectrum Disorders',
      titleAr: 'فهم اضطرابات طيف التوحد',
      videoUrl: '/videos/module1.mp4',
      duration: 60,
      order: 1,
    },
    {
      id: '2',
      titleEn: 'Communication Strategies',
      titleAr: 'استراتيجيات التواصل',
      videoUrl: '/videos/module2.mp4',
      duration: 60,
      order: 2,
    },
    {
      id: '3',
      titleEn: 'Behavioral Interventions',
      titleAr: 'التدخلات السلوكية',
      videoUrl: '/videos/module3.mp4',
      duration: 60,
      order: 3,
    },
  ],
  isActive: true,
};

const mockT = (key: string, params?: any) => {
  const translations: Record<string, string> = {
    'common.back': 'Back',
    'common.loading': 'Loading...',
    'courses.enroll': 'Enroll Now',
    'courses.enrolled': 'Enrolled',
    'courses.startCourse': 'Start Course',
    'courses.continueCourse': 'Continue Course',
    'courses.enrollmentExpired': 'Enrollment Expired',
    'courses.courseOverview': 'Course Overview',
    'courses.whatYouWillLearn': 'What You\'ll Learn',
    'courses.courseContent': 'Course Content',
    'courses.courseIncludes': 'This course includes',
    'courses.videoContent': `${params?.hours || 0} hours of video content`,
    'courses.certificateOfCompletion': 'Certificate of completion',
    'courses.lifetimeAccess': '30-day access',
    'courses.modules': 'modules',
    'courses.progress': 'Progress',
    'courses.enrolledOn': 'Enrolled on',
    'courses.enrollmentExpiresOn': `Expires on ${params?.date || ''}`,
    'courses.completedOn': 'Completed on',
    'courses.watchPreview': 'Watch Preview',
    'courses.previewVideo': 'Preview Video',
    'courses.enrollmentSuccess': 'Successfully enrolled in course',
    'courses.enrollmentError': 'Failed to enroll in course',
  };
  return translations[key] || key;
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('CourseDetail', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      language: 'en',
      direction: 'ltr',
      setLanguage: jest.fn(),
    });

    mockUseAuth.mockReturnValue({
      user: null,
      isAuthenticated: false,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
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
    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    expect(screen.getByText('Learn the basics of autism spectrum disorders and effective intervention strategies.')).toBeInTheDocument();
    expect(screen.getByText('3h')).toBeInTheDocument(); // Duration
    expect(screen.getByText('3 modules')).toBeInTheDocument();
    expect(screen.getByText('English')).toBeInTheDocument(); // Language
  });

  it('shows enroll button for unauthenticated users', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Enroll Now')).toBeInTheDocument();
  });

  it('shows enroll button for authenticated users not enrolled', () => {
    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Enroll Now')).toBeInTheDocument();
  });

  it('shows enrollment information when user is enrolled', () => {
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

    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Enrolled')).toBeInTheDocument();
    expect(screen.getByText('Continue Course')).toBeInTheDocument();
    expect(screen.getByText('50%')).toBeInTheDocument(); // Progress
  });

  it('shows start course button for enrolled user with no progress', () => {
    const mockEnrollment = {
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date('2024-01-01'),
      expiryDate: new Date('2024-02-01'),
      progress: 0,
      completionDate: undefined,
      certificateUrl: undefined,
    };

    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Start Course')).toBeInTheDocument();
  });

  it('shows expired status for expired enrollment', () => {
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

    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [mockEnrollment],
      loading: false,
      error: null,
      enrollInCourse: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByRole('button', { name: /enrollment expired/i })).toBeDisabled();
  });

  it('displays course modules correctly', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Course Content')).toBeInTheDocument();
    expect(screen.getByText('Understanding Autism Spectrum Disorders')).toBeInTheDocument();
    expect(screen.getByText('Communication Strategies')).toBeInTheDocument();
    expect(screen.getByText('Behavioral Interventions')).toBeInTheDocument();
  });

  it('shows preview button for first module', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('Watch Preview')).toBeInTheDocument();
  });

  it('opens preview modal when preview button is clicked', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    const previewButton = screen.getByText('Watch Preview');
    fireEvent.click(previewButton);

    expect(screen.getByText('Preview Video')).toBeInTheDocument();
  });

  it('displays course includes section', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('This course includes')).toBeInTheDocument();
    expect(screen.getByText('3 hours of video content')).toBeInTheDocument();
    expect(screen.getByText('Certificate of completion')).toBeInTheDocument();
    expect(screen.getByText('30-day access')).toBeInTheDocument();
  });

  it('handles enrollment process', async () => {
    const mockEnrollInCourse = jest.fn().mockResolvedValue({
      id: '1',
      userId: 'user1',
      courseId: '1',
      enrollmentDate: new Date(),
      expiryDate: new Date(),
      progress: 0,
    });

    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [],
      loading: false,
      error: null,
      enrollInCourse: mockEnrollInCourse,
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    const enrollButton = screen.getByText('Enroll Now');
    fireEvent.click(enrollButton);

    await waitFor(() => {
      expect(mockEnrollInCourse).toHaveBeenCalledWith('1');
    });
  });

  it('displays enrollment error when enrollment fails', async () => {
    const mockEnrollInCourse = jest.fn().mockRejectedValue(new Error('Enrollment failed'));

    mockUseAuth.mockReturnValue({
      user: { id: '1', email: 'test@example.com', firstName: 'Test', lastName: 'User', role: 'user' as any, preferredLanguage: 'en', isEmailVerified: true, createdAt: new Date() },
      isAuthenticated: true,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      loading: false,
      error: null,
    });

    mockUseEnrollments.mockReturnValue({
      enrollments: [],
      loading: false,
      error: null,
      enrollInCourse: mockEnrollInCourse,
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    const enrollButton = screen.getByText('Enroll Now');
    fireEvent.click(enrollButton);

    await waitFor(() => {
      expect(screen.getByText('Enrollment failed')).toBeInTheDocument();
    });
  });

  it('renders in Arabic when language is set to Arabic', () => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      language: 'ar',
      direction: 'rtl',
      setLanguage: jest.fn(),
    });

    renderWithRouter(<CourseDetail course={mockCourse} />);

    expect(screen.getByText('مقدمة في التوحد')).toBeInTheDocument();
    expect(screen.getByText('تعلم أساسيات اضطرابات طيف التوحد واستراتيجيات التدخل الفعالة.')).toBeInTheDocument();
  });

  it('has back link to courses page', () => {
    renderWithRouter(<CourseDetail course={mockCourse} />);

    const backLink = screen.getByText('Back');
    expect(backLink.closest('a')).toHaveAttribute('href', '/courses');
  });
});