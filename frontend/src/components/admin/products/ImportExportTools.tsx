import React, { useState, useRef } from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import { adminProductService } from '@/services/adminProductService';
import type { ProductCategory, ImportResult } from '@/services/adminProductService';

interface ImportExportToolsProps {
  categories: ProductCategory[];
  onClose: () => void;
  onImportComplete: () => void;
}

export const ImportExportTools: React.FC<ImportExportToolsProps> = ({
  categories,
  onClose,
  onImportComplete,
}) => {
  const { t, isRTL } = useLocalization();
  const [activeTab, setActiveTab] = useState<'import' | 'export'>('import');
  const [isLoading, setIsLoading] = useState(false);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const [importSettings, setImportSettings] = useState({
    updateExisting: false,
  });

  const [exportSettings, setExportSettings] = useState({
    format: 'csv' as 'csv' | 'xlsx',
    includeInactive: false,
    selectedCategories: [] as string[],
  });

  const handleImport = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const file = fileInputRef.current?.files?.[0];
    if (!file) {
      alert(t('admin.products.import.selectFile', 'Please select a file to import'));
      return;
    }

    setIsLoading(true);
    setImportResult(null);
    
    try {
      const result = await adminProductService.importProducts({
        file,
        updateExisting: importSettings.updateExisting,
      });
      
      setImportResult(result);
      
      if (result.successCount > 0) {
        await onImportComplete();
      }
    } catch (error) {
      console.error('Import failed:', error);
      alert(t('admin.products.import.failed', 'Import failed. Please check your file and try again.'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleExport = async () => {
    setIsLoading(true);
    
    try {
      const blob = await adminProductService.exportProducts({
        format: exportSettings.format,
        includeInactive: exportSettings.includeInactive,
        categoryIds: exportSettings.selectedCategories.length > 0 ? exportSettings.selectedCategories : undefined,
      });
      
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `products-export.${exportSettings.format}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Export failed:', error);
      alert(t('admin.products.export.failed', 'Export failed. Please try again.'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      const blob = await adminProductService.getProductTemplate();
      
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'product-import-template.csv';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Template download failed:', error);
      alert(t('admin.products.import.templateFailed', 'Failed to download template. Please try again.'));
    }
  };

  const handleCategoryToggle = (categoryId: string) => {
    setExportSettings(prev => ({
      ...prev,
      selectedCategories: prev.selectedCategories.includes(categoryId)
        ? prev.selectedCategories.filter(id => id !== categoryId)
        : [...prev.selectedCategories, categoryId]
    }));
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-11/12 max-w-4xl shadow-lg rounded-md bg-white dark:bg-gray-800">
        <div className="mt-3">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-white">
              {t('admin.products.importExport.title', 'Import/Export Products')}
            </h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-200 dark:border-gray-700 mb-6">
            <nav className="-mb-px flex space-x-8 rtl:space-x-reverse">
              <button
                onClick={() => setActiveTab('import')}
                className={cn(
                  'py-2 px-1 border-b-2 font-medium text-sm',
                  activeTab === 'import'
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
                )}
              >
                {t('admin.products.import.title', 'Import Products')}
              </button>
              <button
                onClick={() => setActiveTab('export')}
                className={cn(
                  'py-2 px-1 border-b-2 font-medium text-sm',
                  activeTab === 'export'
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
                )}
              >
                {t('admin.products.export.title', 'Export Products')}
              </button>
            </nav>
          </div>

          {/* Import Tab */}
          {activeTab === 'import' && (
            <div className="space-y-6">
              {/* Import Instructions */}
              <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-3 rtl:ml-0 rtl:mr-3">
                    <h3 className="text-sm font-medium text-blue-800 dark:text-blue-200">
                      {t('admin.products.import.instructions', 'Import Instructions')}
                    </h3>
                    <div className="mt-2 text-sm text-blue-700 dark:text-blue-300">
                      <ul className="list-disc list-inside space-y-1">
                        <li>{t('admin.products.import.instruction1', 'Download the template file to see the required format')}</li>
                        <li>{t('admin.products.import.instruction2', 'Fill in your product data following the template structure')}</li>
                        <li>{t('admin.products.import.instruction3', 'Upload your completed CSV file using the form below')}</li>
                        <li>{t('admin.products.import.instruction4', 'Review the import results and fix any errors if needed')}</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>

              {/* Download Template */}
              <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div>
                  <h4 className="text-sm font-medium text-gray-900 dark:text-white">
                    {t('admin.products.import.template', 'Import Template')}
                  </h4>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    {t('admin.products.import.templateDescription', 'Download the CSV template with the correct format and sample data')}
                  </p>
                </div>
                <Button
                  variant="outline"
                  onClick={handleDownloadTemplate}
                >
                  {t('admin.products.import.downloadTemplate', 'Download Template')}
                </Button>
              </div>

              {/* Import Form */}
              <form onSubmit={handleImport} className="space-y-4">
                {/* File Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t('admin.products.import.selectFile', 'Select CSV File')} *
                  </label>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept=".csv"
                    className="block w-full text-sm text-gray-500 dark:text-gray-400 file:mr-4 rtl:file:mr-0 rtl:file:ml-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 dark:file:bg-blue-900/20 dark:file:text-blue-400"
                  />
                </div>

                {/* Import Options */}
                <div className="space-y-3">
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="updateExisting"
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      checked={importSettings.updateExisting}
                      onChange={(e) => setImportSettings(prev => ({ ...prev, updateExisting: e.target.checked }))}
                    />
                    <label htmlFor="updateExisting" className="ml-2 rtl:ml-0 rtl:mr-2 block text-sm text-gray-900 dark:text-white">
                      {t('admin.products.import.updateExisting', 'Update existing products (match by name)')}
                    </label>
                  </div>
                </div>

                {/* Import Button */}
                <div className="flex justify-end">
                  <Button
                    type="submit"
                    disabled={isLoading}
                  >
                    {isLoading ? (
                      <>
                        <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        {t('admin.products.import.importing', 'Importing...')}
                      </>
                    ) : (
                      t('admin.products.import.import', 'Import Products')
                    )}
                  </Button>
                </div>
              </form>

              {/* Import Results */}
              {importResult && (
                <div className="mt-6 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                  <h4 className="text-sm font-medium text-gray-900 dark:text-white mb-3">
                    {t('admin.products.import.results', 'Import Results')}
                  </h4>
                  
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <div className="text-center p-3 bg-blue-100 dark:bg-blue-900/20 rounded">
                      <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                        {importResult.totalProcessed}
                      </div>
                      <div className="text-sm text-blue-800 dark:text-blue-300">
                        {t('admin.products.import.totalProcessed', 'Total Processed')}
                      </div>
                    </div>
                    <div className="text-center p-3 bg-green-100 dark:bg-green-900/20 rounded">
                      <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                        {importResult.successCount}
                      </div>
                      <div className="text-sm text-green-800 dark:text-green-300">
                        {t('admin.products.import.successful', 'Successful')}
                      </div>
                    </div>
                    <div className="text-center p-3 bg-red-100 dark:bg-red-900/20 rounded">
                      <div className="text-2xl font-bold text-red-600 dark:text-red-400">
                        {importResult.errorCount}
                      </div>
                      <div className="text-sm text-red-800 dark:text-red-300">
                        {t('admin.products.import.errors', 'Errors')}
                      </div>
                    </div>
                  </div>

                  {/* Errors */}
                  {importResult.errors.length > 0 && (
                    <div className="mb-4">
                      <h5 className="text-sm font-medium text-red-800 dark:text-red-300 mb-2">
                        {t('admin.products.import.errorDetails', 'Error Details')}
                      </h5>
                      <div className="max-h-40 overflow-y-auto">
                        {importResult.errors.map((error, index) => (
                          <div key={index} className="text-sm text-red-700 dark:text-red-400 mb-1">
                            {t('admin.products.import.errorRow', 'Row {row}', { row: error.row })}: {error.field} - {error.message}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Warnings */}
                  {importResult.warnings.length > 0 && (
                    <div>
                      <h5 className="text-sm font-medium text-yellow-800 dark:text-yellow-300 mb-2">
                        {t('admin.products.import.warnings', 'Warnings')}
                      </h5>
                      <div className="max-h-40 overflow-y-auto">
                        {importResult.warnings.map((warning, index) => (
                          <div key={index} className="text-sm text-yellow-700 dark:text-yellow-400 mb-1">
                            {t('admin.products.import.warningRow', 'Row {row}', { row: warning.row })}: {warning.message}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Export Tab */}
          {activeTab === 'export' && (
            <div className="space-y-6">
              {/* Export Options */}
              <div className="space-y-4">
                {/* Format Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t('admin.products.export.format', 'Export Format')}
                  </label>
                  <div className="flex space-x-4 rtl:space-x-reverse">
                    <label className="flex items-center">
                      <input
                        type="radio"
                        name="format"
                        value="csv"
                        checked={exportSettings.format === 'csv'}
                        onChange={(e) => setExportSettings(prev => ({ ...prev, format: e.target.value as 'csv' | 'xlsx' }))}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                      />
                      <span className="ml-2 rtl:ml-0 rtl:mr-2 text-sm text-gray-900 dark:text-white">CSV</span>
                    </label>
                    <label className="flex items-center">
                      <input
                        type="radio"
                        name="format"
                        value="xlsx"
                        checked={exportSettings.format === 'xlsx'}
                        onChange={(e) => setExportSettings(prev => ({ ...prev, format: e.target.value as 'csv' | 'xlsx' }))}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                      />
                      <span className="ml-2 rtl:ml-0 rtl:mr-2 text-sm text-gray-900 dark:text-white">Excel (XLSX)</span>
                    </label>
                  </div>
                </div>

                {/* Include Options */}
                <div className="space-y-3">
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="includeInactive"
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      checked={exportSettings.includeInactive}
                      onChange={(e) => setExportSettings(prev => ({ ...prev, includeInactive: e.target.checked }))}
                    />
                    <label htmlFor="includeInactive" className="ml-2 rtl:ml-0 rtl:mr-2 block text-sm text-gray-900 dark:text-white">
                      {t('admin.products.export.includeInactive', 'Include inactive products')}
                    </label>
                  </div>
                </div>

                {/* Category Filter */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t('admin.products.export.categories', 'Filter by Categories (optional)')}
                  </label>
                  <div className="max-h-40 overflow-y-auto border border-gray-300 dark:border-gray-600 rounded-md p-3 bg-white dark:bg-gray-700">
                    {categories.length === 0 ? (
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {t('admin.products.export.noCategories', 'No categories available')}
                      </p>
                    ) : (
                      <div className="space-y-2">
                        {categories.map((category) => (
                          <label key={category.id} className="flex items-center">
                            <input
                              type="checkbox"
                              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                              checked={exportSettings.selectedCategories.includes(category.id)}
                              onChange={() => handleCategoryToggle(category.id)}
                            />
                            <span className="ml-2 rtl:ml-0 rtl:mr-2 text-sm text-gray-900 dark:text-white">
                              {isRTL ? category.nameAr : category.nameEn}
                            </span>
                          </label>
                        ))}
                      </div>
                    )}
                  </div>
                  <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                    {t('admin.products.export.categoriesHelp', 'Leave empty to export all categories')}
                  </p>
                </div>
              </div>

              {/* Export Button */}
              <div className="flex justify-end">
                <Button
                  onClick={handleExport}
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <>
                      <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      {t('admin.products.export.exporting', 'Exporting...')}
                    </>
                  ) : (
                    t('admin.products.export.export', 'Export Products')
                  )}
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ImportExportTools;