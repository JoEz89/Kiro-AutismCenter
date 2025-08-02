import { useEffect, useRef, useState, useCallback } from 'react';

interface UseAccessibilityOptions {
  announcePageChanges?: boolean;
  manageFocus?: boolean;
  trapFocus?: boolean;
}

export const useAccessibility = (options: UseAccessibilityOptions = {}) => {
  const {
    announcePageChanges = true,
    manageFocus = true,
    trapFocus = false
  } = options;

  const [announcement, setAnnouncement] = useState<string>('');
  const focusRef = useRef<HTMLElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  // Announce messages to screen readers
  const announce = useCallback((message: string, priority: 'polite' | 'assertive' = 'polite') => {
    setAnnouncement(message);
    
    // Clear the announcement after a delay to allow for re-announcements
    setTimeout(() => setAnnouncement(''), 1000);
  }, []);

  // Focus management
  const setFocus = useCallback((element: HTMLElement | null) => {
    if (element && manageFocus) {
      // Store previous focus for restoration
      previousFocusRef.current = document.activeElement as HTMLElement;
      
      // Set focus with a small delay to ensure element is ready
      setTimeout(() => {
        element.focus();
        focusRef.current = element;
      }, 100);
    }
  }, [manageFocus]);

  // Restore previous focus
  const restoreFocus = useCallback(() => {
    if (previousFocusRef.current && manageFocus) {
      previousFocusRef.current.focus();
      previousFocusRef.current = null;
    }
  }, [manageFocus]);

  // Skip to main content
  const skipToMain = useCallback(() => {
    const mainContent = document.getElementById('main-content') || 
                       document.querySelector('main') ||
                       document.querySelector('[role="main"]');
    
    if (mainContent) {
      setFocus(mainContent as HTMLElement);
      announce('Skipped to main content');
    }
  }, [setFocus, announce]);

  // Check if user prefers reduced motion
  const prefersReducedMotion = useCallback(() => {
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }, []);

  // Check if user is using a screen reader
  const isUsingScreenReader = useCallback(() => {
    // This is a heuristic - not 100% accurate but useful
    return window.navigator.userAgent.includes('NVDA') ||
           window.navigator.userAgent.includes('JAWS') ||
           window.speechSynthesis?.speaking ||
           document.querySelector('[aria-live]') !== null;
  }, []);

  // Keyboard navigation helpers
  const handleKeyboardNavigation = useCallback((
    event: KeyboardEvent,
    handlers: {
      onEscape?: () => void;
      onEnter?: () => void;
      onArrowUp?: () => void;
      onArrowDown?: () => void;
      onArrowLeft?: () => void;
      onArrowRight?: () => void;
      onTab?: (event: KeyboardEvent) => void;
      onHome?: () => void;
      onEnd?: () => void;
    }
  ) => {
    switch (event.key) {
      case 'Escape':
        handlers.onEscape?.();
        break;
      case 'Enter':
        handlers.onEnter?.();
        break;
      case 'ArrowUp':
        event.preventDefault();
        handlers.onArrowUp?.();
        break;
      case 'ArrowDown':
        event.preventDefault();
        handlers.onArrowDown?.();
        break;
      case 'ArrowLeft':
        event.preventDefault();
        handlers.onArrowLeft?.();
        break;
      case 'ArrowRight':
        event.preventDefault();
        handlers.onArrowRight?.();
        break;
      case 'Tab':
        handlers.onTab?.(event);
        break;
      case 'Home':
        event.preventDefault();
        handlers.onHome?.();
        break;
      case 'End':
        event.preventDefault();
        handlers.onEnd?.();
        break;
    }
  }, []);

  // Focus trap for modals and dialogs
  const createFocusTrap = useCallback((container: HTMLElement) => {
    if (!trapFocus) return () => {};

    const focusableElements = container.querySelectorAll(
      'button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href], [tabindex]:not([tabindex="-1"]), [contenteditable="true"]'
    ) as NodeListOf<HTMLElement>;

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Tab') {
        if (event.shiftKey) {
          if (document.activeElement === firstElement) {
            event.preventDefault();
            lastElement.focus();
          }
        } else {
          if (document.activeElement === lastElement) {
            event.preventDefault();
            firstElement.focus();
          }
        }
      }
    };

    container.addEventListener('keydown', handleKeyDown);
    
    // Focus first element
    if (firstElement) {
      firstElement.focus();
    }

    return () => {
      container.removeEventListener('keydown', handleKeyDown);
    };
  }, [trapFocus]);

  // Page change announcements
  useEffect(() => {
    if (announcePageChanges) {
      const handleLocationChange = () => {
        const pageTitle = document.title;
        announce(`Navigated to ${pageTitle}`);
      };

      // Listen for route changes (works with React Router)
      window.addEventListener('popstate', handleLocationChange);
      
      return () => {
        window.removeEventListener('popstate', handleLocationChange);
      };
    }
  }, [announcePageChanges, announce]);

  return {
    announcement,
    announce,
    setFocus,
    restoreFocus,
    skipToMain,
    prefersReducedMotion,
    isUsingScreenReader,
    handleKeyboardNavigation,
    createFocusTrap,
    focusRef
  };
};

export default useAccessibility;