import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import ImportExportTools from '../ImportExportTools';
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
    importProducts: vi.fn(),
    exportProducts: vi.fn(),
    getProductTemplate: vi.fn(),
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
    isActive: true,
  },
  {
    id: 'cat2',
    nameEn: 'Category 2',
    nameAr: 'فئة 2',
    isActive: true,
  },
];

const defaultProps = {
  categories: mockCategories,
  onClose: vi.fn(),
  onImportComplete: vi.fn(),
};

// Mock URL.createObjectURL and URL.revokeObjectURL
Object.defineProperty(window, 'URL', {
  value: {
    createObjectURL: vi.fn(() => 'blob:mock-url'),
    revokeObjectURL: vi.fn(),
  },
});

// Mock window.alert
Object.defineProperty(window, 'alert', {
  writable: true,
  value: vi.fn(),
});

describe('ImportExportTools', () => {
  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, fallback?: string) => {
        const translations: Record<string, string> = {
          'admin.products.importExport.title': 'Import/Export Products',
          'admin.products.import.title': 'Import Products',
          'admin.products.export.title': 'Export Products',
          'admin.products.import.instructions': 'Import Instructions',
          'admin.products.import.selectFile': 'Please select a file to import',
          'admin.products.import.downloadTemplate': 'Download Template',
          'admin.products.import.import': 'Import Products',
          'admin.products.import.importing': 'Importing...',
          'admin.products.import.updateExisting': 'Update existing products (match by name)',
          'admin.products.import.results': 'Import Results',
          'admin.products.import.totalProcessed': 'Total Processed',
          'admin.products.import.successful': 'Successful',
          'admin.products.import.errors': 'Errors',
          'admin.products.import.errorDetails': 'Error Details',
          'admin.products.import.warnings': 'Warnings',
          'admin.products.import.failed': 'Import failed. Please check your file and try again.',
          'admin.products.import.templateFailed': 'Failed to download template. Please try again.',
          'admin.products.export.format': 'Export Format',
          'admin.products.export.includeInactive': 'Include inactive products',
          'admin.products.export.categories': 'Filter by Categories (optional)',
          'admin.products.export.noCategories': 'No categories available',
          'admin.products.export.export': 'Export Products',
          'admin.products.export.exporting': 'Exporting...',
          'admin.products.export.failed': 'Export failed. Please try again.',
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

  it('renders correctly with import tab active by default', () => {
    render(<ImportExportTools {...defaultProps} />);
    
    expect(screen.getByText('admin.products.importExport.title')).toBeInTheDocument();
    expect(screen.getByText('admin.products.import.title')).toBeInTheDocument();
    expect(screen.getByText('admin.products.export.title')).toBeInTheDocument();
    expect(screen.getByText('admin.products.import.instructions')).toBeInTheDocument();
  });

  it('switches between import and export tabs', () => {
    render(<ImportExportTools {...defaultProps} />);
    
    // Initially on import tab
    expect(screen.getByText('admin.products.import.selectFile')).toBeInTheDocument();
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    expect(screen.getByText('admin.products.export.format')).toBeInTheDocument();
    expect(screen.queryByText('admin.products.import.selectFile')).not.toBeInTheDocument();
  });

  it('handles template download', async () => {
    const mockBlob = new Blob(['template content'], { type: 'text/csv' });
    mockAdminProductService.getProductTemplate.mockResolvedValue(mockBlob);
    
    render(<ImportExportTools {...defaultProps} />);
    
    const downloadButton = screen.getByText('admin.products.import.downloadTemplate');
    fireEvent.click(downloadButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.getProductTemplate).toHaveBeenCalled();
      expect(window.URL.createObjectURL).toHaveBeenCalledWith(mockBlob);
    });
  });

  it('handles file import with success', async () => {
    const mockFile = new File(['csv content'], 'products.csv', { type: 'text/csv' });
    const mockResult = {
      totalProcessed: 10,
      successCount: 8,
      errorCount: 2,
      errors: [
        { row: 3, field: 'price', message: 'Invalid price', value: 'abc' },
        { row: 5, field: 'name', message: 'Name required', value: '' },
      ],
      warnings: [
        { row: 7, message: 'Category not found, using default' },
      ],
    };
    
    mockAdminProductService.importProducts.mockResolvedValue(mockResult);
    const onImportComplete = vi.fn();
    
    render(<ImportExportTools {...defaultProps} onImportComplete={onImportComplete} />);
    
    // Select file
    const fileInput = screen.getByRole('textbox', { hidden: true }) as HTMLInputElement;
    Object.defineProperty(fileInput, 'files', {
      value: [mockFile],
      writable: false,
    });
    fireEvent.change(fileInput);
    
    // Submit import
    const importButton = screen.getByText('admin.products.import.import');
    fireEvent.click(importButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.importProducts).toHaveBeenCalledWith({
        file: mockFile,
        updateExisting: false,
      });
      expect(onImportComplete).toHaveBeenCalled();
    });
    
    // Check results display
    expect(screen.getByText('admin.products.import.results')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument(); // totalProcessed
    expect(screen.getByText('8')).toBeInTheDocument(); // successCount
    expect(screen.getByText('2')).toBeInTheDocument(); // errorCount
  });

  it('handles import without file selection', async () => {
    render(<ImportExportTools {...defaultProps} />);
    
    const importButton = screen.getByText('admin.products.import.import');
    fireEvent.click(importButton);
    
    expect(window.alert).toHaveBeenCalledWith('admin.products.import.selectFile');
  });

  it('handles import with update existing option', async () => {
    const mockFile = new File(['csv content'], 'products.csv', { type: 'text/csv' });
    const mockResult = {
      totalProcessed: 5,
      successCount: 5,
      errorCount: 0,
      errors: [],
      warnings: [],
    };
    
    mockAdminProductService.importProducts.mockResolvedValue(mockResult);
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Enable update existing
    const updateCheckbox = screen.getByRole('checkbox', { name: 'admin.products.import.updateExisting' });
    fireEvent.click(updateCheckbox);
    
    // Select file
    const fileInput = screen.getByRole('textbox', { hidden: true }) as HTMLInputElement;
    Object.defineProperty(fileInput, 'files', {
      value: [mockFile],
      writable: false,
    });
    fireEvent.change(fileInput);
    
    // Submit import
    const importButton = screen.getByText('admin.products.import.import');
    fireEvent.click(importButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.importProducts).toHaveBeenCalledWith({
        file: mockFile,
        updateExisting: true,
      });
    });
  });

  it('handles export with default settings', async () => {
    const mockBlob = new Blob(['export content'], { type: 'text/csv' });
    mockAdminProductService.exportProducts.mockResolvedValue(mockBlob);
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    // Click export
    const exportButton = screen.getByText('admin.products.export.export');
    fireEvent.click(exportButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.exportProducts).toHaveBeenCalledWith({
        format: 'csv',
        includeInactive: false,
        categoryIds: undefined,
      });
      expect(window.URL.createObjectURL).toHaveBeenCalledWith(mockBlob);
    });
  });

  it('handles export with custom settings', async () => {
    const mockBlob = new Blob(['export content'], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    mockAdminProductService.exportProducts.mockResolvedValue(mockBlob);
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    // Change format to XLSX
    const xlsxRadio = screen.getByRole('radio', { name: 'Excel (XLSX)' });
    fireEvent.click(xlsxRadio);
    
    // Include inactive products
    const includeInactiveCheckbox = screen.getByRole('checkbox', { name: 'admin.products.export.includeInactive' });
    fireEvent.click(includeInactiveCheckbox);
    
    // Select specific categories
    const categoryCheckboxes = screen.getAllByRole('checkbox');
    const category1Checkbox = categoryCheckboxes.find(cb => 
      cb.nextElementSibling?.textContent === 'Category 1'
    );
    if (category1Checkbox) {
      fireEvent.click(category1Checkbox);
    }
    
    // Click export
    const exportButton = screen.getByText('admin.products.export.export');
    fireEvent.click(exportButton);
    
    await waitFor(() => {
      expect(mockAdminProductService.exportProducts).toHaveBeenCalledWith({
        format: 'xlsx',
        includeInactive: true,
        categoryIds: ['cat1'],
      });
    });
  });

  it('handles export error', async () => {
    mockAdminProductService.exportProducts.mockRejectedValue(new Error('Export failed'));
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    // Click export
    const exportButton = screen.getByText('admin.products.export.export');
    fireEvent.click(exportButton);
    
    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith('admin.products.export.failed');
    });
  });

  it('shows loading state during operations', async () => {
    mockAdminProductService.exportProducts.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 100))
    );
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    // Click export
    const exportButton = screen.getByText('admin.products.export.export');
    fireEvent.click(exportButton);
    
    expect(screen.getByText('admin.products.export.exporting')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.queryByText('admin.products.export.exporting')).not.toBeInTheDocument();
    });
  });

  it('handles modal close', () => {
    const onClose = vi.fn();
    render(<ImportExportTools {...defaultProps} onClose={onClose} />);
    
    const closeButton = screen.getByRole('button', { name: '' }); // Close button with SVG
    fireEvent.click(closeButton);
    
    expect(onClose).toHaveBeenCalled();
  });

  it('displays categories in export filter', () => {
    render(<ImportExportTools {...defaultProps} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    expect(screen.getByText('Category 1')).toBeInTheDocument();
    expect(screen.getByText('Category 2')).toBeInTheDocument();
  });

  it('handles empty categories list in export', () => {
    render(<ImportExportTools {...defaultProps} categories={[]} />);
    
    // Switch to export tab
    const exportTab = screen.getByText('admin.products.export.title');
    fireEvent.click(exportTab);
    
    expect(screen.getByText('admin.products.export.noCategories')).toBeInTheDocument();
  });

  it('displays import error details', async () => {
    const mockFile = new File(['csv content'], 'products.csv', { type: 'text/csv' });
    const mockResult = {
      totalProcessed: 3,
      successCount: 1,
      errorCount: 2,
      errors: [
        { row: 2, field: 'price', message: 'Invalid price format', value: 'abc' },
        { row: 3, field: 'name', message: 'Name is required', value: '' },
      ],
      warnings: [
        { row: 1, message: 'Category not found, using default category' },
      ],
    };
    
    mockAdminProductService.importProducts.mockResolvedValue(mockResult);
    
    render(<ImportExportTools {...defaultProps} />);
    
    // Select file and import
    const fileInput = screen.getByRole('textbox', { hidden: true }) as HTMLInputElement;
    Object.defineProperty(fileInput, 'files', {
      value: [mockFile],
      writable: false,
    });
    fireEvent.change(fileInput);
    
    const importButton = screen.getByText('admin.products.import.import');
    fireEvent.click(importButton);
    
    await waitFor(() => {
      expect(screen.getByText('admin.products.import.errorDetails')).toBeInTheDocument();
      expect(screen.getByText('admin.products.import.warnings')).toBeInTheDocument();
    });
  });
});