import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import CategoryManager from '../CategoryManager';
import { useLocalization } from '@/hooks';
import { adminProductService } from '@/services/adminProductService';
import type { ProductCategory } from '@/types';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
}));

// Mock the admin product service
vi.mock('@/services/adminProductService', () => ({
  adminProductService: {
    createCategory: vi.fn(),
    updateCategory: vi.fn(),
    deleteCategory: vi.fn(),
  },
}));

// Mock the Button component
vi.mock('@/components/ui', () => ({
  Button: ({ children, onClick, disabled, type, ...props }: any) => (
    <button onClick={onClick} disabled={disabled} type={type} {...props}>
      {children}
    </button>
  ),
}));

const mockUseLocalization = useLocalization as vi.MockedFunction<typeof useLocalization>;
const mockAdminProductService = adminProductService as vi.Mocked<typeof adminProductService>;

const mockCategories: ProductCategory[] = [
  {
    id: 'cat1',
    nameEn: 'Category 1',
    nameAr: 'فئة 1',
    descriptionEn: 'Description 1',
    descriptionAr: 'وصف 1',
    isActive: true,
  },
  {
    id: 'cat2',
    nameEn: 'Category 2',
    nameAr: 'فئة 2',
    isActive: false,
  },
];

const defaultProps = {
  categories: mockCategories,
  onClose: vi.fn(),
  onCategoriesChange: vi.fn(),
};

// Mock window.confirm
Object.defineProperty(window, 'confirm', {
  writable: true,
  value: vi.fn(),
});

describe('CategoryManager', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.categories.title': 'Category Management',
          'admin.categories.addCategory': 'Add Category',
          'admin.categories.editCategory': 'Edit Category',
          'admin.categories.existingCategories': 'Existing Categories',
          'admin.categories.noCategories': 'No categories found',
          'admin.categories.noCategoriesDescription': 'Create your first category to organize products.',
          'admin.categories.active': 'Active',
          'admin.categories.inactive': 'Inactive',
          'admin.categories.nameEn': 'Category Name (English)',
          'admin.categories.nameAr': 'Category Name (Arabic)',
          'admin.categories.descriptionEn': 'Description (English)',
          'admin.categories.descriptionAr': 'Description (Arabic)',
          'admin.categories.isActive': 'Category is active',
          'admin.categories.nameEnPlaceholder': 'Enter category name in English',
          'admin.categories.nameArPlaceholder': 'Enter category name in Arabic',
          'admin.categories.descriptionEnPlaceholder': 'Enter category description in English',
          'admin.categories.descriptionArPlaceholder': 'Enter category description in Arabic',
          'admin.categories.validation.nameEnRequired': 'English name is required',
          'admin.categories.validation.nameArRequired': 'Arabic name is required',
          'admin.categories.confirmDelete': 'Are you sure you want to delete this category?',
          'common.edit': 'Edit',
          'common.delete': 'Delete',
          'common.cancel': 'Cancel',
          'common.save': 'Save',
          'common.saving': 'Saving...',
        };
        return translations[key] || fallback || key;
      },
      isRTL: false,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn(),
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly with categories', () => {
    render(<CategoryManager {...defaultProps} />);
    
    expect(screen.getByText('admin.categories.title')).toBeInTheDocument();
    expect(screen.getByText('admin.categories.addCategory')).toBeInTheDocument();
    expect(screen.getByText('Category 1')).toBeInTheDocument();
    expect(screen.getByText('Category 2')).toBeInTheDocument();
  });

  it('renders empty state when no categories', () => {
    render(<CategoryManager {...defaultProps} categories={[]} />);
    
    expect(screen.getByText('admin.categories.noCategories')).toBeInTheDocument();
    expect(screen.getByText('admin.categories.noCategoriesDescription')).toBeInTheDocument();
  });

  it('shows category form when add button is clicked', () => {
    render(<CategoryManager {...defaultProps} />);
    
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    expect(screen.getByPlaceholderText('admin.categories.nameEnPlaceholder')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('admin.categories.nameArPlaceholder')).toBeInTheDocument();
  });

  it('handles category creation', async () => {
    mockAdminProductService.createCategory.mockResolvedValue(mockCategories[0]);
    const onCategoriesChange = vi.fn();
    
    render(<CategoryManager {...defaultProps} onCategoriesChange={onCategoriesChange} />);
    
    // Open form
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    // Fill form
    fireEvent.change(screen.getByPlaceholderText('admin.categories.nameEnPlaceholder'), {
      target: { value: 'New Category' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.categories.nameArPlaceholder'), {
      target: { value: 'فئة جديدة' }
    });
    
    // Submit form
    const saveButton = screen.getByText('common.save');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.createCategory).toHaveBeenCalledWith({
        nameEn: 'New Category',
        nameAr: 'فئة جديدة',
        descriptionEn: '',
        descriptionAr: '',
        isActive: true,
      });
      expect(onCategoriesChange).toHaveBeenCalled();
    });
  });

  it('handles category editing', async () => {
    mockAdminProductService.updateCategory.mockResolvedValue(mockCategories[0]);
    const onCategoriesChange = vi.fn();
    
    render(<CategoryManager {...defaultProps} onCategoriesChange={onCategoriesChange} />);
    
    // Click edit button
    const editButtons = screen.getAllByText('common.edit');
    fireEvent.click(editButtons[0]);
    
    // Form should be pre-filled
    expect(screen.getByDisplayValue('Category 1')).toBeInTheDocument();
    expect(screen.getByDisplayValue('فئة 1')).toBeInTheDocument();
    
    // Modify and submit
    fireEvent.change(screen.getByDisplayValue('Category 1'), {
      target: { value: 'Updated Category' }
    });
    
    const saveButton = screen.getByText('common.save');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.updateCategory).toHaveBeenCalledWith('cat1', {
        nameEn: 'Updated Category',
        nameAr: 'فئة 1',
        descriptionEn: 'Description 1',
        descriptionAr: 'وصف 1',
        isActive: true,
      });
      expect(onCategoriesChange).toHaveBeenCalled();
    });
  });

  it('handles category deletion with confirmation', async () => {
    (window.confirm as vi.Mock).mockReturnValue(true);
    mockAdminProductService.deleteCategory.mockResolvedValue(undefined);
    const onCategoriesChange = vi.fn();
    
    render(<CategoryManager {...defaultProps} onCategoriesChange={onCategoriesChange} />);
    
    const deleteButtons = screen.getAllByText('common.delete');
    fireEvent.click(deleteButtons[0]);
    
    await waitFor(() => {
      expect(window.confirm).toHaveBeenCalledWith('admin.categories.confirmDelete');
      expect(mockAdminProductService.deleteCategory).toHaveBeenCalledWith('cat1');
      expect(onCategoriesChange).toHaveBeenCalled();
    });
  });

  it('cancels deletion when user declines confirmation', async () => {
    (window.confirm as vi.Mock).mockReturnValue(false);
    
    render(<CategoryManager {...defaultProps} />);
    
    const deleteButtons = screen.getAllByText('common.delete');
    fireEvent.click(deleteButtons[0]);
    
    expect(window.confirm).toHaveBeenCalledWith('admin.categories.confirmDelete');
    expect(mockAdminProductService.deleteCategory).not.toHaveBeenCalled();
  });

  it('validates required fields', async () => {
    render(<CategoryManager {...defaultProps} />);
    
    // Open form
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    // Submit without filling required fields
    const saveButton = screen.getByText('common.save');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.categories.validation.nameEnRequired')).toBeInTheDocument();
      expect(screen.getByText('admin.categories.validation.nameArRequired')).toBeInTheDocument();
    });
  });

  it('handles form cancellation', () => {
    render(<CategoryManager {...defaultProps} />);
    
    // Open form
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    // Cancel form
    const cancelButton = screen.getByText('common.cancel');
    fireEvent.click(cancelButton);
    
    // Form should be hidden
    expect(screen.queryByPlaceholderText('admin.categories.nameEnPlaceholder')).not.toBeInTheDocument();
  });

  it('handles modal close', () => {
    const onClose = vi.fn();
    render(<CategoryManager {...defaultProps} onClose={onClose} />);
    
    const closeButton = screen.getByRole('button', { name: '' }); // Close button with SVG
    fireEvent.click(closeButton);
    
    expect(onClose).toHaveBeenCalled();
  });

  it('shows loading state during operations', async () => {
    mockAdminProductService.createCategory.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 100))
    );
    
    render(<CategoryManager {...defaultProps} />);
    
    // Open form and fill it
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    fireEvent.change(screen.getByPlaceholderText('admin.categories.nameEnPlaceholder'), {
      target: { value: 'Test' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.categories.nameArPlaceholder'), {
      target: { value: 'تجربة' }
    });
    
    // Submit form
    const saveButton = screen.getByText('common.save');
    fireEvent.click(saveButton);
    
    expect(screen.getByText('common.saving')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.queryByText('common.saving')).not.toBeInTheDocument();
    });
  });

  it('displays correct status badges', () => {
    render(<CategoryManager {...defaultProps} />);
    
    expect(screen.getByText('admin.categories.active')).toBeInTheDocument();
    expect(screen.getByText('admin.categories.inactive')).toBeInTheDocument();
  });

  it('handles active status toggle', () => {
    render(<CategoryManager {...defaultProps} />);
    
    // Open form
    const addButton = screen.getByText('admin.categories.addCategory');
    fireEvent.click(addButton);
    
    const activeCheckbox = screen.getByRole('checkbox', { name: 'admin.categories.isActive' });
    expect(activeCheckbox).toBeChecked();
    
    fireEvent.click(activeCheckbox);
    expect(activeCheckbox).not.toBeChecked();
  });

  it('renders Arabic content when RTL', () => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.categories.title': 'Category Management',
          'admin.categories.addCategory': 'Add Category',
          'admin.categories.editCategory': 'Edit Category',
          'admin.categories.existingCategories': 'Existing Categories',
          'admin.categories.noCategories': 'No categories found',
          'admin.categories.noCategoriesDescription': 'Create your first category to organize products.',
          'admin.categories.active': 'Active',
          'admin.categories.inactive': 'Inactive',
          'admin.categories.nameEn': 'Category Name (English)',
          'admin.categories.nameAr': 'Category Name (Arabic)',
          'admin.categories.descriptionEn': 'Description (English)',
          'admin.categories.descriptionAr': 'Description (Arabic)',
          'admin.categories.isActive': 'Category is active',
          'admin.categories.nameEnPlaceholder': 'Enter category name in English',
          'admin.categories.nameArPlaceholder': 'Enter category name in Arabic',
          'admin.categories.descriptionEnPlaceholder': 'Enter category description in English',
          'admin.categories.descriptionArPlaceholder': 'Enter category description in Arabic',
          'admin.categories.validation.nameEnRequired': 'English name is required',
          'admin.categories.validation.nameArRequired': 'Arabic name is required',
          'admin.categories.confirmDelete': 'Are you sure you want to delete this category?',
          'common.edit': 'Edit',
          'common.delete': 'Delete',
          'common.cancel': 'Cancel',
          'common.save': 'Save',
          'common.saving': 'Saving...',
        };
        return translations[key] || fallback || key;
      },
      isRTL: true,
      language: 'ar',
      direction: 'rtl',
      setLanguage: vi.fn(),
    });

    render(<CategoryManager {...defaultProps} />);
    
    expect(screen.getByText('فئة 1')).toBeInTheDocument();
    expect(screen.getByText('فئة 2')).toBeInTheDocument();
  });
});