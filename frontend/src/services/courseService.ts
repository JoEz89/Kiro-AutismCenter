import { apiClient } from './api';
import type { Course, Enrollment, ApiResponse } from '../types';

export interface CourseFilters {
  search?: string;
  level?: string;
  sortBy?: 'title' | 'duration' | 'newest' | 'oldest';
  page?: number;
  limit?: number;
}

export interface CoursesResponse {
  courses: Course[];
  total: number;
  page: number;
  totalPages: number;
}

export interface EnrollmentResponse {
  enrollment: Enrollment;
  course: Course;
}

export const courseService = {
  // Get all courses with filtering and pagination
  async getCourses(filters: CourseFilters = {}): Promise<ApiResponse<CoursesResponse>> {
    const params = new URLSearchParams();
    
    if (filters.search) params.append('search', filters.search);
    if (filters.level) params.append('level', filters.level);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.limit) params.append('limit', filters.limit.toString());

    const response = await apiClient.get<ApiResponse<CoursesResponse>>(`/courses?${params}`);
    return response.data;
  },

  // Get course by ID
  async getCourseById(courseId: string): Promise<ApiResponse<Course>> {
    const response = await apiClient.get<ApiResponse<Course>>(`/courses/${courseId}`);
    return response.data;
  },

  // Get user's enrollments
  async getUserEnrollments(): Promise<ApiResponse<Enrollment[]>> {
    const response = await apiClient.get<ApiResponse<Enrollment[]>>('/enrollments');
    return response.data;
  },

  // Get specific enrollment
  async getEnrollment(enrollmentId: string): Promise<ApiResponse<EnrollmentResponse>> {
    const response = await apiClient.get<ApiResponse<EnrollmentResponse>>(`/enrollments/${enrollmentId}`);
    return response.data;
  },

  // Enroll in a course
  async enrollInCourse(courseId: string): Promise<ApiResponse<Enrollment>> {
    const response = await apiClient.post<ApiResponse<Enrollment>>('/enrollments', {
      courseId,
    });
    return response.data;
  },

  // Get course progress
  async getCourseProgress(enrollmentId: string): Promise<ApiResponse<{
    enrollmentId: string;
    courseId: string;
    progress: number;
    completedModules: string[];
    totalModules: number;
    timeSpent: number;
    lastAccessedAt: Date;
  }>> {
    const response = await apiClient.get(`/enrollments/${enrollmentId}/progress`);
    return response.data;
  },

  // Update lesson progress
  async updateLessonProgress(enrollmentId: string, moduleId: string, completed: boolean): Promise<ApiResponse<void>> {
    const response = await apiClient.put(`/enrollments/${enrollmentId}/progress`, {
      moduleId,
      completed,
    });
    return response.data;
  },

  // Get secure video URL
  async getVideoUrl(enrollmentId: string, moduleId: string): Promise<ApiResponse<{ videoUrl: string; expiresAt: Date }>> {
    const response = await apiClient.get(`/enrollments/${enrollmentId}/modules/${moduleId}/video`);
    return response.data;
  },

  // Generate certificate
  async generateCertificate(enrollmentId: string): Promise<ApiResponse<{ certificateUrl: string }>> {
    const response = await apiClient.post(`/enrollments/${enrollmentId}/certificate`);
    return response.data;
  },

  // Get bookmarks
  async getBookmarks(enrollmentId: string): Promise<ApiResponse<{
    moduleId: string;
    moduleTitle: string;
    bookmarkedAt: Date;
  }[]>> {
    const response = await apiClient.get(`/enrollments/${enrollmentId}/bookmarks`);
    return response.data;
  },

  // Add bookmark
  async addBookmark(enrollmentId: string, moduleId: string): Promise<ApiResponse<void>> {
    const response = await apiClient.post(`/enrollments/${enrollmentId}/bookmarks`, {
      moduleId,
    });
    return response.data;
  },

  // Remove bookmark
  async removeBookmark(enrollmentId: string, moduleId: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete(`/enrollments/${enrollmentId}/bookmarks/${moduleId}`);
    return response.data;
  },
};

export default courseService;