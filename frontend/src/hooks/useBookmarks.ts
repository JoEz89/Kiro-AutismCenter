import { useState, useEffect, useCallback } from 'react';
import { courseService } from '../services/courseService';

interface Bookmark {
  moduleId: string;
  moduleTitle: string;
  bookmarkedAt: Date;
}

export const useBookmarks = (enrollmentId: string) => {
  const [bookmarks, setBookmarks] = useState<Bookmark[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchBookmarks = useCallback(async () => {
    if (!enrollmentId) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      const response = await courseService.getBookmarks(enrollmentId);
      
      if (response.success) {
        setBookmarks(response.data);
      } else {
        setError('Failed to fetch bookmarks');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch bookmarks');
    } finally {
      setLoading(false);
    }
  }, [enrollmentId]);

  useEffect(() => {
    fetchBookmarks();
  }, [fetchBookmarks]);

  const addBookmark = useCallback(async (moduleId: string) => {
    if (!enrollmentId) return;

    try {
      await courseService.addBookmark(enrollmentId, moduleId);
      await fetchBookmarks(); // Refresh bookmarks
    } catch (err) {
      throw err;
    }
  }, [enrollmentId, fetchBookmarks]);

  const removeBookmark = useCallback(async (moduleId: string) => {
    if (!enrollmentId) return;

    try {
      await courseService.removeBookmark(enrollmentId, moduleId);
      await fetchBookmarks(); // Refresh bookmarks
    } catch (err) {
      throw err;
    }
  }, [enrollmentId, fetchBookmarks]);

  const isBookmarked = useCallback((moduleId: string) => {
    return bookmarks.some(bookmark => bookmark.moduleId === moduleId);
  }, [bookmarks]);

  return {
    bookmarks,
    loading,
    error,
    addBookmark,
    removeBookmark,
    isBookmarked,
    refetch: fetchBookmarks,
  };
};

export default useBookmarks;