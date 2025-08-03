import React, { useState } from 'react';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';

interface BulkActionsProps {
  selectedCount: number;
  selectedProductIds: string[];
  onAction: (action: string, productIds: string[]) => void;
}

export const BulkActions: React.FC<BulkActionsProps> = ({
  selectedCount,
  selectedProductIds,
  onAction,
}) => {
  const { t } = useLocalization();
  const [isLoading, setIsLoading] = useState(false);

  const handleAction = async (action: string) => {
    setIsLoading(true);
    try {
      await onAction(action, selectedProductIds);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3 rtl:space-x-reverse">
          <div className="flex items-center">
            <svg className="h-5 w-5 text-blue-600 dark:text-blue-400 mr-2 rtl:mr-0 rtl:ml-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span className="text-sm font-medium text-blue-900 dark:text-blue-100">
              {t('admin.products.selectedItems', '{count} items selected', { count: selectedCount })}
            </span>
          </div>
        </div>

        <div className="flex items-center space-x-2 rtl:space-x-reverse">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleAction('activate')}
            disabled={isLoading}
            className="text-green-700 border-green-300 hover:bg-green-50 dark:text-green-400 dark:border-green-600 dark:hover:bg-green-900/20"
          >
            {isLoading ? (
              <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            ) : (
              <svg className="h-4 w-4 mr-1 rtl:mr-0 rtl:ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            )}
            {t('admin.products.activate', 'Activate')}
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleAction('deactivate')}
            disabled={isLoading}
            className="text-yellow-700 border-yellow-300 hover:bg-yellow-50 dark:text-yellow-400 dark:border-yellow-600 dark:hover:bg-yellow-900/20"
          >
            {isLoading ? (
              <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            ) : (
              <svg className="h-4 w-4 mr-1 rtl:mr-0 rtl:ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728" />
              </svg>
            )}
            {t('admin.products.deactivate', 'Deactivate')}
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleAction('delete')}
            disabled={isLoading}
            className="text-red-700 border-red-300 hover:bg-red-50 dark:text-red-400 dark:border-red-600 dark:hover:bg-red-900/20"
          >
            {isLoading ? (
              <svg className="animate-spin -ml-1 mr-2 rtl:mr-0 rtl:ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            ) : (
              <svg className="h-4 w-4 mr-1 rtl:mr-0 rtl:ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            )}
            {t('admin.products.delete', 'Delete')}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default BulkActions;