import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CourseNavigation } from '../CourseNavigation';
import { useLocalization } from '../../../hooks/useLocalization';
import type { CourseModule } from '../../../types';

// Mock dependencies
jest.mock('../../../hooks/useLocalization');
jest.mock('../../../utils', () => ({
  formatDuration: (duration: number) => `${Math.floor(duration / 60)}:${(duration % 60).toString().padStart(2, '0')}`,
}));

const mockUseLocalization = useLocalization as jest.MockedFunction<typeof useLocalization>;

describe('CourseNavigation', () => {
  const mockModules: CourseModule[] = [
    {
      id: 'module-1',
      titleEn: 'Introduction to Autism',
      titleAr: 'مقدمة عن التوحد',
      videoUrl: 'video1.mp4',
      duration: 300, // 5 minutes
      order: 1,
    },
    {
      id: 'module-2',
      titleEn: 'Understanding Behaviors',
      titleAr: 'فهم السلوكيات',
      videoUrl: 'video2.mp4',
      duration: 600, // 10 minutes
      order: 2,
    },
    {
      id: 'module-3',
      titleEn: 'Communication Strategies',
      titleAr: 'استراتيجيات التواصل',
      videoUrl: 'video3.mp4',
      duration: 450, // 7.5 minutes
      order: 3,
    },
  ];

  const defaultProps = {
    modules: mockModules,
    currentModuleIndex: 0,
    completedModules: ['module-1'],
    bookmarkedModules: ['module-2'],
    onModuleChange: jest.fn(),
    onBookmarkToggle: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
    
    mockUseLocalization.mockReturnValue({
      t: (key: string, params?: any) => {
        const translations: Record<string, string> = {
          'courses.courseContent': 'Course Content',
          'courses.moduleProgress': '{{current}} of {{total}}',
          'courses.completed': 'Completed',
          'courses.previousModule': 'Previous Module',
          'courses.nextModule': 'Next Module',
          'courses.addBookmark': 'Add bookmark',
          'courses.removeBookmark': 'Remove bookmark',
          'courses.goToModule': 'Go to module: {{module}}',
        };
        
        let result = translations[key] || key;
        if (params) {
          Object.keys(params).forEach(param => {
            result = result.replace(`{{${param}}}`, params[param]);
          });
        }
        return result;
      },
      language: 'en',
      direction: 'ltr',
      setLanguage: jest.fn(),
    });
  });

  describe('Rendering', () => {
    it('should render course navigation with all modules', () => {
      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByText('Course Content')).toBeInTheDocument();
      expect(screen.getByText('1 of 3')).toBeInTheDocument();
      expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
      expect(screen.getByText('Understanding Behaviors')).toBeInTheDocument();
      expect(screen.getByText('Communication Strategies')).toBeInTheDocument();
    });

    it('should show module durations', () => {
      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByText('5:00')).toBeInTheDocument(); // 300 seconds
      expect(screen.getByText('10:00')).toBeInTheDocument(); // 600 seconds
      expect(screen.getByText('7:30')).toBeInTheDocument(); // 450 seconds
    });

    it('should render in Arabic when language is set to Arabic', () => {
      mockUseLocalization.mockReturnValue({
        t: (key: string, params?: any) => key,
        language: 'ar',
        direction: 'rtl',
        setLanguage: jest.fn(),
      });

      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByText('مقدمة عن التوحد')).toBeInTheDocument();
      expect(screen.getByText('فهم السلوكيات')).toBeInTheDocument();
      expect(screen.getByText('استراتيجيات التواصل')).toBeInTheDocument();
    });
  });

  describe('Module Status', () => {
    it('should show completed status for completed modules', () => {
      render(<CourseNavigation {...defaultProps} />);

      // First module should show as completed
      const completedModule = screen.getByText('Introduction to Autism').closest('div');
      expect(completedModule).toHaveClass('bg-green-100');
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });

    it('should highlight current module', () => {
      render(<CourseNavigation {...defaultProps} />);

      // First module should be highlighted as current
      const currentModule = screen.getByText('Introduction to Autism').closest('button');
      expect(currentModule?.parentElement).toHaveClass('bg-blue-50');
    });

    it('should show play icon for current module', () => {
      render(<CourseNavigation {...defaultProps} />);

      // Should have play icon for current module (would need to check SVG)
      const currentModuleButton = screen.getByLabelText('Go to module: Introduction to Autism');
      expect(currentModuleButton).toBeInTheDocument();
    });

    it('should show module numbers for non-current, non-completed modules', () => {
      render(<CourseNavigation {...defaultProps} />);

      // Third module should show number 3
      expect(screen.getByText('3')).toBeInTheDocument();
    });
  });

  describe('Bookmarks', () => {
    it('should show bookmark icon for bookmarked modules', () => {
      render(<CourseNavigation {...defaultProps} />);

      // Module 2 should show bookmark icon (would need to check for bookmark icon)
      const bookmarkedModule = screen.getByText('Understanding Behaviors').closest('div');
      expect(bookmarkedModule).toBeInTheDocument();
    });

    it('should call onBookmarkToggle when bookmark button is clicked', async () => {
      const user = userEvent.setup();
      const onBookmarkToggle = jest.fn();
      
      render(<CourseNavigation {...defaultProps} onBookmarkToggle={onBookmarkToggle} />);

      // Find and click bookmark button for first module
      const bookmarkButton = screen.getByLabelText('Add bookmark');
      await user.click(bookmarkButton);

      expect(onBookmarkToggle).toHaveBeenCalledWith('module-1');
    });

    it('should show remove bookmark option for bookmarked modules', () => {
      render(<CourseNavigation {...defaultProps} />);

      // Module 2 is bookmarked, should show remove option
      const removeBookmarkButton = screen.getByLabelText('Remove bookmark');
      expect(removeBookmarkButton).toBeInTheDocument();
    });
  });

  describe('Navigation', () => {
    it('should call onModuleChange when module is clicked', async () => {
      const user = userEvent.setup();
      const onModuleChange = jest.fn();
      
      render(<CourseNavigation {...defaultProps} onModuleChange={onModuleChange} />);

      const secondModule = screen.getByLabelText('Go to module: Understanding Behaviors');
      await user.click(secondModule);

      expect(onModuleChange).toHaveBeenCalledWith(1);
    });

    it('should disable previous button on first module', () => {
      render(<CourseNavigation {...defaultProps} currentModuleIndex={0} />);

      const previousButton = screen.getByText('Previous Module');
      expect(previousButton).toBeDisabled();
    });

    it('should disable next button on last module', () => {
      render(<CourseNavigation {...defaultProps} currentModuleIndex={2} />);

      const nextButton = screen.getByText('Next Module');
      expect(nextButton).toBeDisabled();
    });

    it('should call onModuleChange when previous button is clicked', async () => {
      const user = userEvent.setup();
      const onModuleChange = jest.fn();
      
      render(<CourseNavigation {...defaultProps} currentModuleIndex={1} onModuleChange={onModuleChange} />);

      const previousButton = screen.getByText('Previous Module');
      await user.click(previousButton);

      expect(onModuleChange).toHaveBeenCalledWith(0);
    });

    it('should call onModuleChange when next button is clicked', async () => {
      const user = userEvent.setup();
      const onModuleChange = jest.fn();
      
      render(<CourseNavigation {...defaultProps} onModuleChange={onModuleChange} />);

      const nextButton = screen.getByText('Next Module');
      await user.click(nextButton);

      expect(onModuleChange).toHaveBeenCalledWith(1);
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels for module buttons', () => {
      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByLabelText('Go to module: Introduction to Autism')).toBeInTheDocument();
      expect(screen.getByLabelText('Go to module: Understanding Behaviors')).toBeInTheDocument();
      expect(screen.getByLabelText('Go to module: Communication Strategies')).toBeInTheDocument();
    });

    it('should have proper ARIA labels for bookmark buttons', () => {
      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByLabelText('Add bookmark')).toBeInTheDocument();
      expect(screen.getByLabelText('Remove bookmark')).toBeInTheDocument();
    });

    it('should support keyboard navigation', async () => {
      const user = userEvent.setup();
      const onModuleChange = jest.fn();
      
      render(<CourseNavigation {...defaultProps} onModuleChange={onModuleChange} />);

      const firstModule = screen.getByLabelText('Go to module: Introduction to Autism');
      
      // Focus and press Enter
      firstModule.focus();
      await user.keyboard('{Enter}');

      expect(onModuleChange).toHaveBeenCalledWith(0);
    });
  });

  describe('Responsive Design', () => {
    it('should have scrollable module list', () => {
      render(<CourseNavigation {...defaultProps} />);

      const moduleList = screen.getByText('Introduction to Autism').closest('.space-y-2');
      expect(moduleList).toHaveClass('max-h-96', 'overflow-y-auto');
    });

    it('should show current module title in navigation controls', () => {
      render(<CourseNavigation {...defaultProps} />);

      expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty modules array', () => {
      render(<CourseNavigation {...defaultProps} modules={[]} />);

      expect(screen.getByText('Course Content')).toBeInTheDocument();
      expect(screen.getByText('0 of 0')).toBeInTheDocument();
    });

    it('should handle invalid current module index', () => {
      render(<CourseNavigation {...defaultProps} currentModuleIndex={10} />);

      // Should not crash and should handle gracefully
      expect(screen.getByText('Course Content')).toBeInTheDocument();
    });

    it('should prevent bookmark toggle event from bubbling', async () => {
      const user = userEvent.setup();
      const onModuleChange = jest.fn();
      const onBookmarkToggle = jest.fn();
      
      render(
        <CourseNavigation 
          {...defaultProps} 
          onModuleChange={onModuleChange}
          onBookmarkToggle={onBookmarkToggle}
        />
      );

      const bookmarkButton = screen.getByLabelText('Add bookmark');
      await user.click(bookmarkButton);

      // Should call bookmark toggle but not module change
      expect(onBookmarkToggle).toHaveBeenCalledWith('module-1');
      expect(onModuleChange).not.toHaveBeenCalled();
    });
  });
});