import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BookmarkButton } from '../BookmarkButton';
import { LanguageContext } from '../../../context/LanguageContext';
import { courseService } from '../../../services/courseService';

// Mock the course service
jest.mock('../../../services/courseService', () => ({
  courseService: {
    addBookmark: jest.fn(),
    removeBookmark: jest.fn(),
  },
}));

const mockLanguageContext = {
  language: 'en' as const,
  direction: 'ltr' as const,
  setLanguage: jest.fn(),
  t: (key: string) => {
    const translations: Record<string, string> = {
      'courses.bookmark': 'Bookmark',
      'courses.removeBookmark': 'Remove Bookmark',
      'courses.bookmarkError': 'Failed to update bookmark',
    };
    return translations[key] || key;
  },
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <LanguageContext.Provider value={mockLanguageContext}>
      {component}
    </LanguageContext.Provider>
  );
};

describe('BookmarkButton', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders bookmark button with label when not bookmarked', () => {
    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
        showLabel={true}
      />
    );

    expect(screen.getByText('Bookmark')).toBeInTheDocument();
    expect(screen.getByRole('button')).toHaveAttribute('title', 'Bookmark');
  });

  it('renders remove bookmark button when bookmarked', () => {
    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={true}
        showLabel={true}
      />
    );

    expect(screen.getByText('Remove Bookmark')).toBeInTheDocument();
    expect(screen.getByRole('button')).toHaveAttribute('title', 'Remove Bookmark');
  });

  it('renders without label when showLabel is false', () => {
    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
        showLabel={false}
      />
    );

    expect(screen.queryByText('Bookmark')).not.toBeInTheDocument();
    expect(screen.getByRole('button')).toHaveAttribute('title', 'Bookmark');
  });

  it('calls addBookmark when clicking unbookmarked button', async () => {
    const mockAddBookmark = courseService.addBookmark as jest.Mock;
    mockAddBookmark.mockResolvedValue({ success: true });

    const onBookmarkChange = jest.fn();

    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
        onBookmarkChange={onBookmarkChange}
      />
    );

    const button = screen.getByRole('button');
    fireEvent.click(button);

    await waitFor(() => {
      expect(mockAddBookmark).toHaveBeenCalledWith('enrollment-1', 'module-1');
      expect(onBookmarkChange).toHaveBeenCalledWith(true);
    });
  });

  it('calls removeBookmark when clicking bookmarked button', async () => {
    const mockRemoveBookmark = courseService.removeBookmark as jest.Mock;
    mockRemoveBookmark.mockResolvedValue({ success: true });

    const onBookmarkChange = jest.fn();

    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={true}
        onBookmarkChange={onBookmarkChange}
      />
    );

    const button = screen.getByRole('button');
    fireEvent.click(button);

    await waitFor(() => {
      expect(mockRemoveBookmark).toHaveBeenCalledWith('enrollment-1', 'module-1');
      expect(onBookmarkChange).toHaveBeenCalledWith(false);
    });
  });

  it('shows loading spinner while processing', async () => {
    const mockAddBookmark = courseService.addBookmark as jest.Mock;
    mockAddBookmark.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
      />
    );

    const button = screen.getByRole('button');
    fireEvent.click(button);

    // Should show loading spinner
    expect(button).toBeDisabled();
  });

  it('handles error when bookmark operation fails', async () => {
    const mockAddBookmark = courseService.addBookmark as jest.Mock;
    mockAddBookmark.mockRejectedValue(new Error('Network error'));

    const consoleSpy = jest.spyOn(console, 'error').mockImplementation();

    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
      />
    );

    const button = screen.getByRole('button');
    fireEvent.click(button);

    await waitFor(() => {
      expect(consoleSpy).toHaveBeenCalledWith('Bookmark error:', expect.any(Error));
    });

    consoleSpy.mockRestore();
  });

  it('applies correct size classes', () => {
    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
        size="lg"
        showLabel={false}
      />
    );

    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
  });

  it('applies custom className', () => {
    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
        className="custom-class"
        showLabel={false}
      />
    );

    const button = screen.getByRole('button');
    expect(button).toHaveClass('custom-class');
  });

  it('prevents multiple clicks while loading', async () => {
    const mockAddBookmark = courseService.addBookmark as jest.Mock;
    mockAddBookmark.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    renderWithProviders(
      <BookmarkButton
        enrollmentId="enrollment-1"
        moduleId="module-1"
        isBookmarked={false}
      />
    );

    const button = screen.getByRole('button');
    
    // Click multiple times rapidly
    fireEvent.click(button);
    fireEvent.click(button);
    fireEvent.click(button);

    // Should only be called once
    expect(mockAddBookmark).toHaveBeenCalledTimes(1);
  });
});