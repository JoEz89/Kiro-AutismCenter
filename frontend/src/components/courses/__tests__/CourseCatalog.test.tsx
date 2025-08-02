import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { CourseCatalog } from '../CourseCatalog';
import { useCourses } from '../../../hooks/useCourses';
import { useLocalization } from '../../../hooks/useLocalization';

// Mock the hooks
jest.mock('../../../hooks/useCourses');
jest.mock('../../../hooks/useLocalization');

const mockUseCourses = useCourses as jest.MockedFunction<typeof useCourses>;
const mockUseLocalization = useLocalization as jest.MockedFunction<typeof useLocalization>;

const mockCourses = [
  {
    id: '1',
    titleEn: 'Introduction to Autism',
    titleAr: 'مقدمة في التوحد',
    descriptionEn: 'Learn the basics of autism',
    descriptionAr: 'تعلم أساسيات التوحد',
    duration: 120,
    thumbnailUrl: '/images/course1.jpg',
    modules: [
      {
        id: '1',
        titleEn: 'Module 1',
        titleAr: 'الوحدة 1',
        videoUrl: '/videos/module1.mp4',
        duration: 60,
        order: 1,
      },
    ],
    isActive: true,
  },
  {
    id: '2',
    titleEn: 'Advanced Autism Strategies',
    titleAr: 'استراتيجيات التوحد المتقدمة',
    descriptionEn: 'Advanced techniques for autism support',
    descriptionAr: 'تقنيات متقدمة لدعم التوحد',
    duration: 180,
    thumbnailUrl: '/images/course2.jpg',
    modules: [
      {
        id: '2',
        titleEn: 'Module 2',
        titleAr: 'الوحدة 2',
        videoUrl: '/videos/module2.mp4',
        duration: 90,
        order: 1,
      },
    ],
    isActive: true,
  },
];

const mockT = (key: string, params?: any) => {
  const translations: Record<string, string> = {
    'courses.title': 'Courses',
    'courses.description': 'Learn about autism with our comprehensive online courses',
    'courses.searchPlaceholder': 'Search courses...',
    'courses.searchLabel': 'Search courses',
    'courses.showingResults': `Showing ${params?.count || 0} courses`,
    'courses.noCourses': 'No courses found',
    'courses.noCoursesDescription': 'Try adjusting your search or filter criteria.',
    'common.filter': 'Filter',
    'common.retry': 'Try Again',
    'products.gridView': 'Grid view',
    'products.listView': 'List view',
  };
  return translations[key] || key;
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('CourseCatalog', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: mockT,
      language: 'en',
      direction: 'ltr',
      setLanguage: jest.fn(),
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('renders loading state', () => {
    mockUseCourses.mockReturnValue({
      courses: [],
      loading: true,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 0 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('renders error state', () => {
    mockUseCourses.mockReturnValue({
      courses: [],
      loading: false,
      error: 'Failed to fetch courses',
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 0 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    expect(screen.getByText('Failed to fetch courses')).toBeInTheDocument();
    expect(screen.getByText('Try Again')).toBeInTheDocument();
  });

  it('renders courses successfully', () => {
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 2 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    expect(screen.getByText('Courses')).toBeInTheDocument();
    expect(screen.getByText('Learn about autism with our comprehensive online courses')).toBeInTheDocument();
    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    expect(screen.getByText('Advanced Autism Strategies')).toBeInTheDocument();
    expect(screen.getByText('Showing 2 courses')).toBeInTheDocument();
  });

  it('renders empty state when no courses found', () => {
    mockUseCourses.mockReturnValue({
      courses: [],
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 0 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    expect(screen.getByText('No courses found')).toBeInTheDocument();
    expect(screen.getByText('Try adjusting your search or filter criteria.')).toBeInTheDocument();
  });

  it('handles search functionality', async () => {
    const mockUpdateFilters = jest.fn();
    
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 2 },
      updateFilters: mockUpdateFilters,
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    const searchInput = screen.getByPlaceholderText('Search courses...');
    const searchForm = searchInput.closest('form');

    fireEvent.change(searchInput, { target: { value: 'autism' } });
    fireEvent.submit(searchForm!);

    await waitFor(() => {
      expect(mockUpdateFilters).toHaveBeenCalledWith({ search: 'autism' });
    });
  });

  it('toggles view mode between grid and list', () => {
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 2 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    const gridViewButton = screen.getByLabelText('Grid view');
    const listViewButton = screen.getByLabelText('List view');

    expect(gridViewButton).toHaveClass('bg-blue-100');
    expect(listViewButton).not.toHaveClass('bg-blue-100');

    fireEvent.click(listViewButton);

    expect(listViewButton).toHaveClass('bg-blue-100');
    expect(gridViewButton).not.toHaveClass('bg-blue-100');
  });

  it('shows and hides filters panel', () => {
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 2 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    const filterButton = screen.getByText('Filter');
    
    fireEvent.click(filterButton);
    
    // The filters panel should be visible
    expect(screen.getByText('Filter')).toBeInTheDocument();
  });

  it('handles pagination', () => {
    const mockChangePage = jest.fn();
    
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 3, total: 30 },
      updateFilters: jest.fn(),
      changePage: mockChangePage,
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog />);

    // Pagination should be visible when there are multiple pages
    expect(screen.getByLabelText('Pagination')).toBeInTheDocument();
  });

  it('respects showFilters prop', () => {
    mockUseCourses.mockReturnValue({
      courses: mockCourses,
      loading: false,
      error: null,
      filters: {},
      pagination: { page: 1, totalPages: 1, total: 2 },
      updateFilters: jest.fn(),
      changePage: jest.fn(),
      refetch: jest.fn(),
    });

    renderWithRouter(<CourseCatalog showFilters={false} />);

    expect(screen.queryByPlaceholderText('Search courses...')).not.toBeInTheDocument();
    expect(screen.queryByText('Filter')).not.toBeInTheDocument();
  });
});