import { useState, useEffect, useCallback } from 'react';
import { courseService, type CourseFilters, type CoursesResponse } from '../services/courseService';
import type { Course, Enrollment } from '../types';

export const useCourses = (initialFilters: CourseFilters = {}) => {
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<CourseFilters>(initialFilters);
  const [pagination, setPagination] = useState({
    page: 1,
    totalPages: 1,
    total: 0,
  });

  const fetchCourses = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await courseService.getCourses(filters);
      
      if (response.success) {
        setCourses(response.data.courses);
        setPagination({
          page: response.data.page,
          totalPages: response.data.totalPages,
          total: response.data.total,
        });
      } else {
        setError('Failed to fetch courses');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch courses');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  const updateFilters = useCallback((newFilters: Partial<CourseFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters, page: 1 }));
  }, []);

  const changePage = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, []);

  const refetch = useCallback(() => {
    fetchCourses();
  }, [fetchCourses]);

  return {
    courses,
    loading,
    error,
    filters,
    pagination,
    updateFilters,
    changePage,
    refetch,
  };
};

export const useCourse = (courseId: string) => {
  const [course, setCourse] = useState<Course | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCourse = async () => {
      if (!courseId) return;

      try {
        setLoading(true);
        setError(null);
        
        const response = await courseService.getCourseById(courseId);
        
        if (response.success) {
          setCourse(response.data);
        } else {
          setError('Course not found');
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch course');
      } finally {
        setLoading(false);
      }
    };

    fetchCourse();
  }, [courseId]);

  return { course, loading, error };
};

export const useEnrollments = () => {
  const [enrollments, setEnrollments] = useState<Enrollment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchEnrollments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await courseService.getUserEnrollments();
      
      if (response.success) {
        setEnrollments(response.data);
      } else {
        setError('Failed to fetch enrollments');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch enrollments');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEnrollments();
  }, [fetchEnrollments]);

  const enrollInCourse = useCallback(async (courseId: string) => {
    try {
      const response = await courseService.enrollInCourse(courseId);
      
      if (response.success) {
        setEnrollments(prev => [...prev, response.data]);
        return response.data;
      } else {
        throw new Error('Failed to enroll in course');
      }
    } catch (err) {
      throw err;
    }
  }, []);

  const refetch = useCallback(() => {
    fetchEnrollments();
  }, [fetchEnrollments]);

  return {
    enrollments,
    loading,
    error,
    enrollInCourse,
    refetch,
  };
};

export const useCourseProgress = (enrollmentId: string) => {
  const [progress, setProgress] = useState<{
    enrollmentId: string;
    courseId: string;
    progress: number;
    completedModules: string[];
    totalModules: number;
    timeSpent: number;
    lastAccessedAt: Date;
  } | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProgress = useCallback(async () => {
    if (!enrollmentId) return;

    try {
      setLoading(true);
      setError(null);
      
      const response = await courseService.getCourseProgress(enrollmentId);
      
      if (response.success) {
        setProgress(response.data);
      } else {
        setError('Failed to fetch progress');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch progress');
    } finally {
      setLoading(false);
    }
  }, [enrollmentId]);

  useEffect(() => {
    fetchProgress();
  }, [fetchProgress]);

  const updateLessonProgress = useCallback(async (moduleId: string, completed: boolean) => {
    try {
      await courseService.updateLessonProgress(enrollmentId, moduleId, completed);
      await fetchProgress(); // Refresh progress
    } catch (err) {
      throw err;
    }
  }, [enrollmentId, fetchProgress]);

  return {
    progress,
    loading,
    error,
    updateLessonProgress,
    refetch: fetchProgress,
  };
};

export default useCourses;