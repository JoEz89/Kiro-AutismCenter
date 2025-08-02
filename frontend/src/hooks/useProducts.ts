import { useState, useEffect, useCallback } from 'react';
import { productService, type ProductFilters, type ProductSortOptions, type ProductsResponse } from '@/services/productService';
import type { Product } from '@/types';

export const useProducts = (
  initialPage: number = 1,
  initialPageSize: number = 12,
  initialFilters?: ProductFilters,
  initialSort?: ProductSortOptions
) => {
  const [data, setData] = useState<ProductsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [filters, setFilters] = useState<ProductFilters | undefined>(initialFilters);
  const [sort, setSort] = useState<ProductSortOptions | undefined>(initialSort);

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await productService.getProducts(page, pageSize, filters, sort);
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch products');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, filters, sort]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const updateFilters = useCallback((newFilters: ProductFilters) => {
    setFilters(newFilters);
    setPage(1); // Reset to first page when filters change
  }, []);

  const updateSort = useCallback((newSort: ProductSortOptions) => {
    setSort(newSort);
    setPage(1); // Reset to first page when sort changes
  }, []);

  const goToPage = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  const refresh = useCallback(() => {
    fetchProducts();
  }, [fetchProducts]);

  return {
    products: data?.products || [],
    totalCount: data?.totalCount || 0,
    totalPages: data?.totalPages || 0,
    currentPage: data?.currentPage || 1,
    pageSize: data?.pageSize || pageSize,
    loading,
    error,
    filters,
    sort,
    updateFilters,
    updateSort,
    goToPage,
    refresh,
  };
};

export const useProduct = (id: string) => {
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProduct = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        setError(null);
        const result = await productService.getProduct(id);
        setProduct(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch product');
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  const refresh = useCallback(async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError(null);
      const result = await productService.getProduct(id);
      setProduct(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch product');
    } finally {
      setLoading(false);
    }
  }, [id]);

  return {
    product,
    loading,
    error,
    refresh,
  };
};

export const useProductSearch = () => {
  const [results, setResults] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = useCallback(async (query: string, limit: number = 10) => {
    if (!query.trim()) {
      setResults([]);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const searchResults = await productService.searchProducts(query, limit);
      setResults(searchResults);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search failed');
      setResults([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const clearResults = useCallback(() => {
    setResults([]);
    setError(null);
  }, []);

  return {
    results,
    loading,
    error,
    search,
    clearResults,
  };
};