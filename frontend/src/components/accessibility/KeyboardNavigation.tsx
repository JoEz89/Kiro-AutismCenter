import React, { useEffect, useRef, useCallback } from 'react';
import { useAccessibility } from '@/hooks/useAccessibility';

interface KeyboardNavigationProps {
  children: React.ReactNode;
  onEscape?: () => void;
  onEnter?: () => void;
  trapFocus?: boolean;
  autoFocus?: boolean;
  className?: string;
}

export const KeyboardNavigation: React.FC<KeyboardNavigationProps> = ({
  children,
  onEscape,
  onEnter,
  trapFocus = false,
  autoFocus = false,
  className
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const { handleKeyboardNavigation, createFocusTrap, setFocus } = useAccessibility({
    trapFocus
  });

  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    handleKeyboardNavigation(event, {
      onEscape,
      onEnter
    });
  }, [handleKeyboardNavigation, onEscape, onEnter]);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    container.addEventListener('keydown', handleKeyDown);
    
    let cleanupFocusTrap: (() => void) | undefined;
    
    if (trapFocus) {
      cleanupFocusTrap = createFocusTrap(container);
    }

    if (autoFocus) {
      const firstFocusable = container.querySelector(
        'button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href], [tabindex]:not([tabindex="-1"]), [contenteditable="true"]'
      ) as HTMLElement;
      
      if (firstFocusable) {
        setFocus(firstFocusable);
      }
    }

    return () => {
      container.removeEventListener('keydown', handleKeyDown);
      cleanupFocusTrap?.();
    };
  }, [handleKeyDown, trapFocus, autoFocus, createFocusTrap, setFocus]);

  return (
    <div
      ref={containerRef}
      className={className}
      role="group"
    >
      {children}
    </div>
  );
};

export default KeyboardNavigation;