import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { App } from '@/App';
import { courseService } from '@/services/courseService';
import { authService } from '@/services/authService';
import { User, UserRole, Course, Enrollment } from '@/types';

// Mock services
vi.mock('@/services/courseService', () => ({
  courseService: {
    getCourses: vi.fn(),
    getCourseById: vi.fn(),
    enrollInCourse: vi.fn(),
    getEnrollments: vi.fn(),
    updateProgress: vi.fn(),
    generateCertificate: vi.fn(),
  },
}));

vi.mock('@/services/authService', () => ({
  authService: {
    verifyToken: vi.fn(),
    logout: vi.fn(),
  },
}));

const mockCourseService = vi.mocked(courseService);
const mockAuthService = vi.mocked(authService);

const mockUser: User = {
  id: '1',
  email: 'user@example.com',
  firstName: 'John',
  lastName: 'Doe',
  role: UserRole.USER,
  preferredLanguage: 'en',
  isEmailVerified: true,
  createdAt: new Date(),
};

const mockCourses: Course[] = [
  {
    id: '1',
    titleEn: 'Understanding Autism Basics',
    titleAr: 'فهم أساسيات التوحد',
    descriptionEn: 'A comprehensive introduction to autism spectrum disorders',
    descriptionAr: 'مقدمة شاملة لاضطرابات طيف التوحد',
    duration: 120,
    thumbnailUrl: 'https://example.com/course1.jpg',
    modules: [
      {
        id: 'module-1',
        titleEn: 'What is Autism?',
        titleAr: 'ما هو التوحد؟',
        videos: [
          {
            id: 'video-1',
            title: 'Introduction to Autism',
            url: 'https://example.com/video1.mp4',
            duration: 600,
            order: 1,
          },
        ],
        order: 1,
      },
    ],
    isActive: true,
    price: 99.99,
    currency: 'BHD',
  },
  {
    id: '2',
    titleEn: 'Advanced Autism Interventions',
    titleAr: 'تدخلات التوحد المتقدمة',
    descriptionEn: 'Advanced strategies for autism intervention',
    descriptionAr: 'استراتيجيات متقدمة للتدخل في التوحد',
    duration: 180,
    thumbnailUrl: 'https://example.com/course2.jpg',
    modules: [],
    isActive: true,
    price: 149.99,
    currency: 'BHD',
  },
];

const mockEnrollment: Enrollment = {
  id: '1',
  userId: '1',
  courseId: '1',
  enrollmentDate: new Date(),
  expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), // 30 days from now
  progress: 25,
  completionDate: undefined,
  certificateUrl: undefined,
};

describe('Course Flow Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    
    // Set up authenticated user
    localStorage.setItem('authToken', 'mock-token');
    localStorage.setItem('user', JSON.stringify(mockUser));
    
    mockAuthService.verifyToken.mockResolvedValue({
      data: mockUser,
      success: true,
    });
  });

  describe('Course Catalog', () => {
    it('should display course catalog with filtering and search', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getCourses.mockResolvedValue({
        data: {
          courses: mockCourses,
          total: 2,
          page: 1,
          limit: 10,
        },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      // Wait for courses to load
      await waitFor(() => {
        expect(screen.getByText('Understanding Autism Basics')).toBeInTheDocument();
        expect(screen.getByText('Advanced Autism Interventions')).toBeInTheDocument();
      });

      // Test search functionality
      const searchInput = screen.getByPlaceholderText(/search courses/i);
      await user.type(searchInput, 'basics');

      await waitFor(() => {
        expect(mockCourseService.getCourses).toHaveBeenCalledWith({
          search: 'basics',
          page: 1,
          limit: 10,
        });
      });

      // Test filtering
      const filterButton = screen.getByRole('button', { name: /filter/i });
      await user.click(filterButton);

      const beginnerFilter = screen.getByLabelText(/beginner/i);
      await user.click(beginnerFilter);

      await waitFor(() => {
        expect(mockCourseService.getCourses).toHaveBeenCalledWith({
          level: 'beginner',
          page: 1,
          limit: 10,
        });
      });
    });

    it('should handle course enrollment', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getCourses.mockResolvedValue({
        data: { courses: mockCourses, total: 2, page: 1, limit: 10 },
        success: true,
      });

      mockCourseService.enrollInCourse.mockResolvedValue({
        data: mockEnrollment,
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Understanding Autism Basics')).toBeInTheDocument();
      });

      // Click enroll button
      const enrollButton = screen.getByRole('button', { name: /enroll now/i });
      await user.click(enrollButton);

      // Should show enrollment confirmation
      await waitFor(() => {
        expect(screen.getByText(/enrollment successful/i)).toBeInTheDocument();
      });

      expect(mockCourseService.enrollInCourse).toHaveBeenCalledWith('1');
    });
  });

  describe('Course Detail and Learning', () => {
    it('should display course details and allow access to enrolled course', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Understanding Autism Basics')).toBeInTheDocument();
        expect(screen.getByText('A comprehensive introduction to autism spectrum disorders')).toBeInTheDocument();
      });

      // Should show course modules
      expect(screen.getByText('What is Autism?')).toBeInTheDocument();

      // Should show progress
      expect(screen.getByText('25% Complete')).toBeInTheDocument();

      // Click to start learning
      const startLearningButton = screen.getByRole('button', { name: /continue learning/i });
      await user.click(startLearningButton);

      // Should navigate to learning page
      await waitFor(() => {
        expect(window.location.pathname).toBe('/courses/1/learn');
      });
    });

    it('should handle video playback and progress tracking', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      mockCourseService.updateProgress.mockResolvedValue({
        data: { progress: 50 },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
      });

      // Should show video player
      const video = screen.getByRole('video');
      expect(video).toBeInTheDocument();

      // Simulate video progress
      const playButton = screen.getByRole('button', { name: /play/i });
      await user.click(playButton);

      // Simulate video time update (50% watched)
      Object.defineProperty(video, 'currentTime', { value: 300 });
      Object.defineProperty(video, 'duration', { value: 600 });
      video.dispatchEvent(new Event('timeupdate'));

      await waitFor(() => {
        expect(mockCourseService.updateProgress).toHaveBeenCalledWith('1', 'video-1', 50);
      });
    });

    it('should handle course completion and certificate generation', async () => {
      const user = userEvent.setup();
      
      const completedEnrollment = {
        ...mockEnrollment,
        progress: 100,
        completionDate: new Date(),
        certificateUrl: 'https://example.com/certificate.pdf',
      };

      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [completedEnrollment],
        success: true,
      });

      mockCourseService.generateCertificate.mockResolvedValue({
        data: { certificateUrl: 'https://example.com/certificate.pdf' },
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('100% Complete')).toBeInTheDocument();
        expect(screen.getByText(/congratulations/i)).toBeInTheDocument();
      });

      // Should show certificate download button
      const downloadButton = screen.getByRole('button', { name: /download certificate/i });
      expect(downloadButton).toBeInTheDocument();

      await user.click(downloadButton);

      // Should trigger certificate download
      expect(mockCourseService.generateCertificate).toHaveBeenCalledWith('1');
    });
  });

  describe('Course Progress Dashboard', () => {
    it('should display user course progress and bookmarks', async () => {
      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/progress']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('My Learning Progress')).toBeInTheDocument();
        expect(screen.getByText('Understanding Autism Basics')).toBeInTheDocument();
        expect(screen.getByText('25% Complete')).toBeInTheDocument();
      });

      // Should show time remaining
      expect(screen.getByText(/days remaining/i)).toBeInTheDocument();

      // Should show bookmarks section
      expect(screen.getByText('Bookmarks')).toBeInTheDocument();
    });

    it('should handle bookmark functionality', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
      });

      // Click bookmark button
      const bookmarkButton = screen.getByRole('button', { name: /bookmark/i });
      await user.click(bookmarkButton);

      // Should show bookmark confirmation
      await waitFor(() => {
        expect(screen.getByText(/bookmarked/i)).toBeInTheDocument();
      });

      // Bookmark button should show active state
      expect(bookmarkButton).toHaveClass('active');
    });
  });

  describe('Course Access Control', () => {
    it('should prevent access to non-enrolled courses', async () => {
      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [], // No enrollments
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/you are not enrolled/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /enroll now/i })).toBeInTheDocument();
      });

      // Should not show video player
      expect(screen.queryByRole('video')).not.toBeInTheDocument();
    });

    it('should prevent access to expired courses', async () => {
      const expiredEnrollment = {
        ...mockEnrollment,
        expiryDate: new Date(Date.now() - 24 * 60 * 60 * 1000), // Yesterday
      };

      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [expiredEnrollment],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/course access expired/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /renew access/i })).toBeInTheDocument();
      });
    });

    it('should prevent multiple device access', async () => {
      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      // Mock multiple device detection
      const originalUserAgent = navigator.userAgent;
      Object.defineProperty(navigator, 'userAgent', {
        value: 'Different Device',
        configurable: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/multiple device access detected/i)).toBeInTheDocument();
      });

      // Restore original user agent
      Object.defineProperty(navigator, 'userAgent', {
        value: originalUserAgent,
        configurable: true,
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle course loading errors', async () => {
      mockCourseService.getCourses.mockRejectedValue(new Error('Network error'));

      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/error loading courses/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
      });
    });

    it('should handle enrollment errors', async () => {
      const user = userEvent.setup();
      
      mockCourseService.getCourses.mockResolvedValue({
        data: { courses: mockCourses, total: 2, page: 1, limit: 10 },
        success: true,
      });

      mockCourseService.enrollInCourse.mockRejectedValue(new Error('Enrollment failed'));

      render(
        <MemoryRouter initialEntries={['/courses']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText('Understanding Autism Basics')).toBeInTheDocument();
      });

      const enrollButton = screen.getByRole('button', { name: /enroll now/i });
      await user.click(enrollButton);

      await waitFor(() => {
        expect(screen.getByText(/enrollment failed/i)).toBeInTheDocument();
      });
    });

    it('should handle video playback errors', async () => {
      mockCourseService.getCourseById.mockResolvedValue({
        data: mockCourses[0],
        success: true,
      });

      mockCourseService.getEnrollments.mockResolvedValue({
        data: [mockEnrollment],
        success: true,
      });

      render(
        <MemoryRouter initialEntries={['/courses/1/learn']}>
          <App />
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('video')).toBeInTheDocument();
      });

      // Simulate video error
      const video = screen.getByRole('video');
      video.dispatchEvent(new Event('error'));

      await waitFor(() => {
        expect(screen.getByText(/error loading video/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
      });
    });
  });
});