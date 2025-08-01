import React from 'react';
import { useLocalization } from '@/hooks';
import { cn } from '@/lib/utils';

interface SkipLinkProps {
  href: string;
  children: React.ReactNode;
  className?: string;
}

export const SkipLink: React.FC<SkipLinkProps> = ({ href, children, className }) => {
  const { isRTL } = useLocalization();

  return (
    <a
      href={href}
      className={cn(
        // Position off-screen by default
        'absolute -top-40 left-6 z-50',
        // Show when focused
        'focus:top-6',
        // Styling
        'bg-blue-600 text-white px-4 py-2 rounded-md',
        'font-medium text-sm',
        'transition-all duration-200',
        'focus:outline-none focus:ring-4 focus:ring-blue-300',
        // RTL support
        isRTL && 'right-6 left-auto',
        className
      )}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          const target = document.querySelector(href);
          if (target) {
            target.scrollIntoView({ behavior: 'smooth' });
            // Focus the target element if it's focusable
            if (target instanceof HTMLElement) {
              target.focus();
            }
          }
        }
      }}
    >
      {children}
    </a>
  );
};

export default SkipLink;