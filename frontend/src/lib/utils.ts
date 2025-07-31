import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Utility function to merge Tailwind CSS classes
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Format currency based on locale
 */
export function formatCurrency(
  amount: number,
  currency: 'USD' | 'BHD' = 'BHD',
  locale: 'en' | 'ar' = 'en'
): string {
  const localeMap = {
    en: 'en-US',
    ar: 'ar-BH',
  };

  return new Intl.NumberFormat(localeMap[locale], {
    style: 'currency',
    currency,
  }).format(amount);
}

/**
 * Format date based on locale
 */
export function formatDate(
  date: Date | string,
  locale: 'en' | 'ar' = 'en',
  options?: Intl.DateTimeFormatOptions
): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const localeMap = {
    en: 'en-US',
    ar: 'ar-BH',
  };

  return new Intl.DateTimeFormat(localeMap[locale], {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    ...options,
  }).format(dateObj);
}

/**
 * Debounce function
 */
export function debounce<T extends (...args: unknown[]) => unknown>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout;
  return (...args: Parameters<T>) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
}

/**
 * Generate unique ID
 */
export function generateId(): string {
  return Math.random().toString(36).substr(2, 9);
}
