import { describe, it, expect } from 'vitest';
import { cn, formatCurrency, formatDate, generateId } from './utils';

describe('Utils', () => {
  describe('cn', () => {
    it('should merge class names correctly', () => {
      expect(cn('px-2 py-1', 'bg-red-500')).toBe('px-2 py-1 bg-red-500');
    });

    it('should handle conditional classes', () => {
      expect(cn('px-2', true && 'py-1', false && 'bg-red-500')).toBe(
        'px-2 py-1'
      );
    });
  });

  describe('formatCurrency', () => {
    it('should format BHD currency correctly', () => {
      const result = formatCurrency(100, 'BHD', 'en');
      expect(result).toContain('100');
      expect(result).toContain('BHD');
    });

    it('should format USD currency correctly', () => {
      const result = formatCurrency(100, 'USD', 'en');
      expect(result).toContain('100');
      expect(result).toContain('$');
    });
  });

  describe('formatDate', () => {
    it('should format date correctly for English locale', () => {
      const date = new Date('2024-01-15');
      const result = formatDate(date, 'en');
      expect(result).toContain('January');
      expect(result).toContain('15');
      expect(result).toContain('2024');
    });
  });

  describe('generateId', () => {
    it('should generate a unique ID', () => {
      const id1 = generateId();
      const id2 = generateId();
      expect(id1).not.toBe(id2);
      expect(typeof id1).toBe('string');
      expect(id1.length).toBeGreaterThan(0);
    });
  });
});