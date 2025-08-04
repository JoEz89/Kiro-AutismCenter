import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import ProductForm from '../ProductForm';
import { useLocalization } from '@/hooks';
import type { Product, ProductCategory } from '@/types';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useLocalization: vi.fn(),
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

const mockCategories: ProductCategory[] = [
  {
    id: 'cat1',
    nameEn: 'Category 1',
    nameAr: 'فئة 1',
    isActive: true,
  },
  {
    id: 'cat2',
    nameEn: 'Category 2',
    nameAr: 'فئة 2',
    isActive: true,
  },
];

const mockProduct: Product = {
  id: '1',
  nameEn: 'Test Product',
  nameAr: 'منتج تجريبي',
  descriptionEn: 'Test description',
  descriptionAr: 'وصف تجريبي',
  price: 29.99,
  currency: 'BHD',
  stockQuantity: 10,
  categoryId: 'cat1',
  imageUrls: ['https://example.com/image1.jpg'],
  isActive: true,
};

const defaultProps = {
  categories: mockCategories,
  onSave: vi.fn(),
  onCancel: vi.fn(),
};

describe('ProductForm', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.addProduct': 'Add Product',
          'admin.products.editProduct': 'Edit Product',
          'admin.products.nameEn': 'Product Name (English)',
          'admin.products.nameAr': 'Product Name (Arabic)',
          'admin.products.descriptionEn': 'Description (English)',
          'admin.products.descriptionAr': 'Description (Arabic)',
          'admin.products.price': 'Price (BHD)',
          'admin.products.stockQuantity': 'Stock Quantity',
          'admin.products.category': 'Category',
          'admin.products.imageUrls': 'Product Images',
          'admin.products.isActive': 'Product is active',
          'admin.products.selectCategory': 'Select a category',
          'admin.products.addImage': 'Add Image',
          'admin.products.nameEnPlaceholder': 'Enter product name in English',
          'admin.products.nameArPlaceholder': 'Enter product name in Arabic',
          'admin.products.descriptionEnPlaceholder': 'Enter product description in English',
          'admin.products.descriptionArPlaceholder': 'Enter product description in Arabic',
          'admin.products.imageUrlPlaceholder': 'Enter image URL',
          'admin.products.validation.nameEnRequired': 'English name is required',
          'admin.products.validation.nameArRequired': 'Arabic name is required',
          'admin.products.validation.descriptionEnRequired': 'English description is required',
          'admin.products.validation.descriptionArRequired': 'Arabic description is required',
          'admin.products.validation.priceRequired': 'Valid price is required',
          'admin.products.validation.stockRequired': 'Valid stock quantity is required',
          'admin.products.validation.categoryRequired': 'Category is required',
          'common.cancel': 'Cancel',
          'common.save': 'Save',
          'common.saving': 'Saving...',
          'common.remove': 'Remove',
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

  it('renders add product form correctly', () => {
    render(<ProductForm {...defaultProps} />);
    
    expect(screen.getByText('admin.products.addProduct')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('admin.products.nameEnPlaceholder')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('admin.products.nameArPlaceholder')).toBeInTheDocument();
  });

  it('renders edit product form with existing data', () => {
    render(<ProductForm {...defaultProps} product={mockProduct} />);
    
    expect(screen.getByText('admin.products.editProduct')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Product')).toBeInTheDocument();
    expect(screen.getByDisplayValue('منتج تجريبي')).toBeInTheDocument();
    expect(screen.getByDisplayValue('29.99')).toBeInTheDocument();
    expect(screen.getByDisplayValue('10')).toBeInTheDocument();
  });

  it('validates required fields', async () => {
    render(<ProductForm {...defaultProps} />);
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.products.validation.nameEnRequired')).toBeInTheDocument();
      expect(screen.getByText('admin.products.validation.nameArRequired')).toBeInTheDocument();
      expect(screen.getByText('admin.products.validation.descriptionEnRequired')).toBeInTheDocument();
      expect(screen.getByText('admin.products.validation.descriptionArRequired')).toBeInTheDocument();
    });
  });

  it('validates price field', async () => {
    render(<ProductForm {...defaultProps} />);
    
    const priceInput = screen.getByPlaceholderText('0.00');
    fireEvent.change(priceInput, { target: { value: '-5' } });
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.products.validation.priceRequired')).toBeInTheDocument();
    });
  });

  it('validates stock quantity field', async () => {
    render(<ProductForm {...defaultProps} />);
    
    const stockInput = screen.getByPlaceholderText('0');
    fireEvent.change(stockInput, { target: { value: '-1' } });
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.products.validation.stockRequired')).toBeInTheDocument();
    });
  });

  it('handles form submission with valid data', async () => {
    const onSave = vi.fn().mockResolvedValue(undefined);
    render(<ProductForm {...defaultProps} onSave={onSave} />);
    
    // Fill in required fields
    fireEvent.change(screen.getByPlaceholderText('admin.products.nameEnPlaceholder'), {
      target: { value: 'New Product' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.nameArPlaceholder'), {
      target: { value: 'منتج جديد' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.descriptionEnPlaceholder'), {
      target: { value: 'New description' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.descriptionArPlaceholder'), {
      target: { value: 'وصف جديد' }
    });
    fireEvent.change(screen.getByPlaceholderText('0.00'), {
      target: { value: '25.99' }
    });
    fireEvent.change(screen.getByPlaceholderText('0'), {
      target: { value: '5' }
    });
    
    // Select category
    const categorySelect = screen.getByDisplayValue('admin.products.selectCategory');
    fireEvent.change(categorySelect, { target: { value: 'cat1' } });
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(onSave).toHaveBeenCalledWith({
        nameEn: 'New Product',
        nameAr: 'منتج جديد',
        descriptionEn: 'New description',
        descriptionAr: 'وصف جديد',
        price: 25.99,
        stockQuantity: 5,
        categoryId: 'cat1',
        imageUrls: [],
        isActive: true,
      });
    });
  });

  it('handles cancel button click', () => {
    const onCancel = vi.fn();
    render(<ProductForm {...defaultProps} onCancel={onCancel} />);
    
    const cancelButton = screen.getByText('common.cancel');
    fireEvent.click(cancelButton);
    
    expect(onCancel).toHaveBeenCalled();
  });

  it('handles close button click', () => {
    const onCancel = vi.fn();
    render(<ProductForm {...defaultProps} onCancel={onCancel} />);
    
    const closeButton = screen.getByRole('button', { name: '' }); // Close button with SVG
    fireEvent.click(closeButton);
    
    expect(onCancel).toHaveBeenCalled();
  });

  it('handles image URL management', () => {
    render(<ProductForm {...defaultProps} />);
    
    // Add image URL
    const addImageButton = screen.getByText('admin.products.addImage');
    fireEvent.click(addImageButton);
    
    const imageInputs = screen.getAllByPlaceholderText('admin.products.imageUrlPlaceholder');
    expect(imageInputs).toHaveLength(2);
    
    // Fill first image URL
    fireEvent.change(imageInputs[0], { target: { value: 'https://example.com/image.jpg' } });
    
    // Remove second image URL input
    const removeButtons = screen.getAllByText('common.remove');
    fireEvent.click(removeButtons[0]);
    
    const updatedImageInputs = screen.getAllByPlaceholderText('admin.products.imageUrlPlaceholder');
    expect(updatedImageInputs).toHaveLength(1);
  });

  it('handles active status toggle', () => {
    render(<ProductForm {...defaultProps} />);
    
    const activeCheckbox = screen.getByRole('checkbox', { name: 'admin.products.isActive' });
    expect(activeCheckbox).toBeChecked();
    
    fireEvent.click(activeCheckbox);
    expect(activeCheckbox).not.toBeChecked();
  });

  it('shows loading state during submission', async () => {
    const onSave = vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
    render(<ProductForm {...defaultProps} onSave={onSave} />);
    
    // Fill required fields
    fireEvent.change(screen.getByPlaceholderText('admin.products.nameEnPlaceholder'), {
      target: { value: 'Test' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.nameArPlaceholder'), {
      target: { value: 'تجربة' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.descriptionEnPlaceholder'), {
      target: { value: 'Test' }
    });
    fireEvent.change(screen.getByPlaceholderText('admin.products.descriptionArPlaceholder'), {
      target: { value: 'تجربة' }
    });
    fireEvent.change(screen.getByPlaceholderText('0.00'), {
      target: { value: '10' }
    });
    fireEvent.change(screen.getByPlaceholderText('0'), {
      target: { value: '1' }
    });
    
    const categorySelect = screen.getByDisplayValue('admin.products.selectCategory');
    fireEvent.change(categorySelect, { target: { value: 'cat1' } });
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    expect(screen.getByText('common.saving')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.queryByText('common.saving')).not.toBeInTheDocument();
    });
  });

  it('clears validation errors when user types', async () => {
    render(<ProductForm {...defaultProps} />);
    
    const submitButton = screen.getByText('common.save');
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.products.validation.nameEnRequired')).toBeInTheDocument();
    });
    
    const nameInput = screen.getByPlaceholderText('admin.products.nameEnPlaceholder');
    fireEvent.change(nameInput, { target: { value: 'Test Product' } });
    
    expect(screen.queryByText('admin.products.validation.nameEnRequired')).not.toBeInTheDocument();
  });
});