import React from 'react';
import { cn } from '@/lib/utils';

interface ScreenReaderOnlyProps {
  children: React.ReactNode;
  className?: string;
  as?: keyof JSX.IntrinsicElements;
}

export const ScreenReaderOnly: React.FC<ScreenReaderOnlyProps> = ({ 
  children, 
  className,
  as: Component = 'span'
}) => {
  return (
    <Component
      className={cn(
        // Screen reader only - visually hidden but accessible to screen readers
        'sr-only',
        // Alternative implementation for better browser support
        'absolute w-px h-px p-0 -m-px overflow-hidden',
        'whitespace-nowrap border-0',
        // Clip path for older browsers
        '[clip:rect(0,0,0,0)]',
        className
      )}
    >
      {children}
    </Component>
  );
};

export default ScreenReaderOnly;