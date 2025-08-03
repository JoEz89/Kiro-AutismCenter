import React, { useState, useEffect } from 'react';
import { AdminLayout } from '@/components/admin';
import { 
  ProductList, 
  ProductFilters, 
  ProductForm, 
  BulkActions,
  CategoryManager,
  ImportExportTools
} from '@/components/admin/products';
import { useLocalization } from '@/hooks';
import { Button } from '@/components/ui';
import type { Product, ProductCategory } from '@/types';
import { adminProductService } from '@/services/adminProductService';
import { productService } from '@/services/productService';

export const AdminProductsPage: React.FC = () => {
  const { t } = useLocalization();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [selectedProducts, setSelectedProducts] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showProductForm, setShowProductForm] = useState(false);
  const [showCategoryManager, setShowCategoryManager] = useState(false);
  const [showImportExport, setShowImportExport] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({
    search: '',
    category: '',
    inStock: undefined as boolean | undefined,
    isActive: undefined as boolean | undefined,
  });

  useEffect(() => {
    loadData();
  }, [currentPage, filters]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [productsResponse, categoriesResponse] = await Promise.all([
        productService.getProducts(currentPage, 20, {
          search: filters.search || undefined,
          category: filters.category || undefined,
          inStock: filters.inStock,
        }),
        productService.getCategories(),
      ]);

      setProducts(productsResponse.products);
      setTotalPages(productsResponse.totalPages);
      setCategories(categoriesResponse);
    } catch (error) {
      console.error('Failed to load products:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleProductSave = async (productData: any) => {
    try {
      if (editingProduct) {
        await adminProductService.updateProduct({ id: editingProduct.id, ...productData });
      } else {
        await adminProductService.createProduct(productData);
      }
      await loadData();
      setShowProductForm(false);
      setEditingProduct(null);
    } catch (error) {
      console.error('Failed to save product:', error);
    }
  };

  const handleProductDelete = async (productId: string) => {
    if (window.confirm(t('admin.products.confirmDelete', 'Are you sure you want to delete this product?'))) {
      try {
        await adminProductService.deleteProduct(productId);
        await loadData();
      } catch (error) {
        console.error('Failed to delete product:', error);
      }
    }
  };

  const handleBulkAction = async (action: string, productIds: string[]) => {
    try {
      switch (action) {
        case 'delete':
          if (window.confirm(t('admin.products.confirmBulkDelete', 'Are you sure you want to delete the selected products?'))) {
            await adminProductService.bulkDeleteProducts(productIds);
            await loadData();
            setSelectedProducts([]);
          }
          break;
        case 'activate':
          await adminProductService.bulkUpdateProducts({
            productIds,
            updates: { isActive: true },
          });
          await loadData();
          setSelectedProducts([]);
          break;
        case 'deactivate':
          await adminProductService.bulkUpdateProducts({
            productIds,
            updates: { isActive: false },
          });
          await loadData();
          setSelectedProducts([]);
          break;
      }
    } catch (error) {
      console.error('Failed to perform bulk action:', error);
    }
  };

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Page header */}
        <div className="md:flex md:items-center md:justify-between">
          <div className="min-w-0 flex-1">
            <h2 className="text-2xl font-bold leading-7 text-gray-900 dark:text-white sm:truncate sm:text-3xl sm:tracking-tight">
              {t('admin.products.title', 'Product Management')}
            </h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {t('admin.products.subtitle', 'Manage your product catalog and inventory.')}
            </p>
          </div>
          <div className="mt-4 flex md:ml-4 md:mt-0 space-x-3 rtl:space-x-reverse">
            <Button
              variant="outline"
              onClick={() => setShowCategoryManager(true)}
            >
              {t('admin.products.manageCategories', 'Manage Categories')}
            </Button>
            <Button
              variant="outline"
              onClick={() => setShowImportExport(true)}
            >
              {t('admin.products.importExport', 'Import/Export')}
            </Button>
            <Button
              onClick={() => setShowProductForm(true)}
            >
              {t('admin.products.addProduct', 'Add Product')}
            </Button>
          </div>
        </div>

        {/* Filters */}
        <ProductFilters
          filters={filters}
          categories={categories}
          onFiltersChange={setFilters}
        />

        {/* Bulk actions */}
        {selectedProducts.length > 0 && (
          <BulkActions
            selectedCount={selectedProducts.length}
            onAction={handleBulkAction}
            selectedProductIds={selectedProducts}
          />
        )}

        {/* Product list */}
        <ProductList
          products={products}
          categories={categories}
          selectedProducts={selectedProducts}
          onSelectionChange={setSelectedProducts}
          onEdit={(product) => {
            setEditingProduct(product);
            setShowProductForm(true);
          }}
          onDelete={handleProductDelete}
          isLoading={isLoading}
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />

        {/* Product form modal */}
        {showProductForm && (
          <ProductForm
            product={editingProduct}
            categories={categories}
            onSave={handleProductSave}
            onCancel={() => {
              setShowProductForm(false);
              setEditingProduct(null);
            }}
          />
        )}

        {/* Category manager modal */}
        {showCategoryManager && (
          <CategoryManager
            categories={categories}
            onClose={() => setShowCategoryManager(false)}
            onCategoriesChange={loadData}
          />
        )}

        {/* Import/Export modal */}
        {showImportExport && (
          <ImportExportTools
            categories={categories}
            onClose={() => setShowImportExport(false)}
            onImportComplete={loadData}
          />
        )}
      </div>
    </AdminLayout>
  );
};

export default AdminProductsPage;