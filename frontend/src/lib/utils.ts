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

/**
 * Format duration in minutes to human readable format
 */
export function formatDuration(minutes: number, locale: 'en' | 'ar' = 'en'): string {
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;

  if (locale === 'ar') {
    if (hours > 0 && remainingMinutes > 0) {
      return `${hours} ساعة ${remainingMinutes} دقيقة`;
    } else if (hours > 0) {
      return `${hours} ساعة`;
    } else {
      return `${remainingMinutes} دقيقة`;
    }
  } else {
    if (hours > 0 && remainingMinutes > 0) {
      return `${hours}h ${remainingMinutes}m`;
    } else if (hours > 0) {
      return `${hours}h`;
    } else {
      return `${remainingMinutes}m`;
    }
  }
}

/**
 * Calculate reading time for text content
 */
export function calculateReadingTime(text: string, wordsPerMinute: number = 200): number {
  const words = text.trim().split(/\s+/).length;
  return Math.ceil(words / wordsPerMinute);
}

/**
 * Truncate text to specified length
 */
export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength).trim() + '...';
}

/**
 * Get relative time string
 */
export function getRelativeTime(date: Date | string, locale: 'en' | 'ar' = 'en'): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - dateObj.getTime()) / 1000);

  const intervals = {
    year: 31536000,
    month: 2592000,
    week: 604800,
    day: 86400,
    hour: 3600,
    minute: 60,
  };

  for (const [unit, seconds] of Object.entries(intervals)) {
    const interval = Math.floor(diffInSeconds / seconds);
    if (interval >= 1) {
      const rtf = new Intl.RelativeTimeFormat(locale === 'ar' ? 'ar-BH' : 'en-US', {
        numeric: 'auto',
      });
      return rtf.format(-interval, unit as Intl.RelativeTimeFormatUnit);
    }
  }

  return locale === 'ar' ? 'الآن' : 'just now';
}

/**
 * Validate email format
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Get file extension from URL or filename
 */
export function getFileExtension(filename: string): string {
  return filename.slice(((filename.lastIndexOf('.') - 1) >>> 0) + 2);
}

/**
 * Format file size in bytes to human readable format
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}
