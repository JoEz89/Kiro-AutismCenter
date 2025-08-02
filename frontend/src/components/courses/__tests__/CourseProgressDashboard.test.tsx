import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { CourseProgressDashboard } from '../CourseProgressDashboard';
import { LanguageContext } from '../../../context/LanguageContext';
import type { Course, Enrollment } from '../../../types';

// Mock the hooks
jest.mock('../../../hooks/useCourses', () => ({
  useCourseProgress: jest.fn(() => ({
    progress: {
      enrollmentId: 'enrollment-1',
      courseId: 'course-1',
      progress: 75,
      completedModules: ['module-1', 'module-2'],
      totalModules: 3,
      timeSpent: 7200, // 2 hours in seconds
      lastAccessedAt: new Date('2024-01-15'),
    },
    loading: false,
    error: null,
  })),
}));

const mockCourse: Course = {
  id: 'course-1',
  titleEn: 'Introduction to Autism',
  titleAr: 'مقدمة في التوحد',
  descriptionEn: 'Learn the basics of autism',
  descriptionAr: 'تعلم أساسيات التوحد',
  duration: 10800, // 3 hours in seconds
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  modules: [
    {
      id: 'module-1',
      titleEn: 'Module 1',
      titleAr: 'الوحدة 1',
      videoUrl: 'https://example.com/video1.mp4',
      duration: 3600,
      order: 1,
    },
    {
      id: 'module-2',
      titleEn: 'Module 2',
      titleAr: 'الوحدة 2',
      videoUrl: 'https://example.com/video2.mp4',
      duration: 3600,
      order: 2,
    },
    {
      id: 'module-3',
      titleEn: 'Module 3',
      titleAr: 'الوحدة 3',
      videoUrl: 'https://example.com/video3.mp4',
      duration: 3600,
      order: 3,
    },
  ],
  isActive: true,
};

const mockEnrollment: Enrollment = {
  id: 'enrollment-1',
  userId: 'user-1',
  courseId: 'course-1',
  enrollmentDate: new Date('2024-01-01'),
  expiryDate: new Date('2024-02-01'),
  progress: 75,
  completionDate: undefined,
  certificateUrl: undefined,
};

const mockLanguageContext = {
  language: 'en' as const,
  direction: 'ltr' as const,
  setLanguage: jest.fn(),
  t: (key: string) => {
    const translations: Record<string, string> = {
      'courses.overallProgress': 'Overall Progress',
      'courses.completedLessons': '{{completed}} of {{total}} lessons completed',
      'courses.timeSpent': 'Time Spent',
      'courses.enrolledOn': 'Enrolled on',
      'courses.enrollmentExpiresOn': 'Expires on {{date}}',
      'courses.courseProgress': 'Course Progress',
      'courses.modules': 'Modules',
      'courses.myBookmarks': 'My Bookmarks',
      'courses.continueCourse': 'Continue Course',
      'courses.viewCertificate': 'View Certificate',
      'courses.completed': 'Completed',
      'courses.start': 'Start',
      'courses.review': 'Review',
      'courses.noBookmarks': 'No bookmarks yet',
      'courses.noBookmarksDescription': 'Bookmark lessons to easily find them later.',
      'courses.startLearning': 'Start Learning',
      'courses.duration': 'Duration',
      'courses.progress': 'Progress',
      'courses.courseDetails': 'Course Details',
      'courses.nextSteps': 'Next Steps',
      'courses.continueProgressMessage': 'Continue your learning journey.',
      'common.back': 'Back',
    };
    return translations[key] || key;
  },
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageContext.Provider value={mockLanguageContext}>
        {component}
      </LanguageContext.Provider>
    </BrowserRouter>
  );
};

describe('CourseProgressDashboard', () => {
  it('renders course progress dashboard correctly', async () => {
    renderWithProviders(
      <CourseProgressDashboard
        enrollment={mockEnrollment}
        course={mockCourse}
      />
    );

    // Check if course title is displayed
    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();

    // Check if progress percentage is displayed
    await waitFor(() => {
      expect(screen.getByText('75%')).toBeInTheDocument();
    });

    // Check if completed modules count is displayed
    expect(screen.getByText('2')).toBeInTheDocument();

    // Check if time spent is displayed
    expect(screen.getByText('2h')).toBeInTheDocument();
  });

  it('displays enrollment information', () => {
    renderWithProviders(
      <CourseProgressDashboard
        enrollment={mockEnrollment}
        course={mockCourse}
      />
    );

    expect(screen.getByText(/Enrolled on/)).toBeInTheDocument();
    expect(screen.getByText(/Expires on/)).toBeInTheDocument();
  });

  it('shows progress bar with correct percentage', async () => {
    renderWithProviders(
      <CourseProgressDashboard
        enrollment={mockEnrollment}
        course={mockCourse}
      />
    );

    await waitFor(() => {
      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toHaveAttribute('aria-valuenow', '75');
    });
  });

  it('displays module list with completion status', async () => {
    renderWithProviders(
      <CourseProgressDashboard
        enrollment={mockEnrollment}
        course={mockCourse}
      />
    );

    // Click on modules tab
    const modulesTab = screen.getByText('Modules');
    modulesTab.click();

    await waitFor(() => {
      expect(screen.getByText('Module 1')).toBeInTheDocument();
      expect(screen.getByText('Module 2')).toBeInTheDocument();
      expect(screen.getByText('Module 3')).toBeInTheDocument();
    });
  });

  it('shows certificate button when course is completed', () => {
    const completedEnrollment = {
      ...mockEnrollment,
      completionDate: new Date('2024-01-20'),
      certificateUrl: 'https://example.com/certificate.pdf',
    };

    renderWithProviders(
      <CourseProgressDashboard
        enrollment={completedEnrollment}
        course={mockCourse}
      />
    );

    expect(screen.getByText('View Certificate')).toBeInTheDocument();
  });

  it('handles Arabic language correctly', () => {
    const arabicContext = {
      ...mockLanguageContext,
      language: 'ar' as const,
      direction: 'rtl' as const,
    };

    render(
      <BrowserRouter>
        <LanguageContext.Provider value={arabicContext}>
          <CourseProgressDashboard
            enrollment={mockEnrollment}
            course={mockCourse}
          />
        </LanguageContext.Provider>
      </BrowserRouter>
    );

    expect(screen.getByText('مقدمة في التوحد')).toBeInTheDocument();
  });

  it('displays bookmarks tab', async () => {
    renderWithProviders(
      <CourseProgressDashboard
        enrollment={mockEnrollment}
        course={mockCourse}
      />
    );

    // Click on bookmarks tab
    const bookmarksTab = screen.getByText('My Bookmarks');
    bookmarksTab.click();

    await waitFor(() => {
      expect(screen.getByText('No bookmarks yet')).toBeInTheDocument();
      expect(screen.getByText('Bookmark lessons to easily find them later.')).toBeInTheDocument();
    });
  });
});