import React, { useEffect, useRef } from 'react';
import { ScreenReaderOnly } from './ScreenReaderOnly';

interface LiveRegionProps {
  message: string;
  politeness?: 'polite' | 'assertive' | 'off';
  clearOnUnmount?: boolean;
}

export const LiveRegion: React.FC<LiveRegionProps> = ({
  message,
  politeness = 'polite',
  clearOnUnmount = true
}) => {
  const regionRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (regionRef.current && message) {
      // Clear previous message first to ensure screen readers announce the new one
      regionRef.current.textContent = '';
      
      // Use a small delay to ensure the clear is processed
      const timer = setTimeout(() => {
        if (regionRef.current) {
          regionRef.current.textContent = message;
        }
      }, 100);

      return () => clearTimeout(timer);
    }
  }, [message]);

  useEffect(() => {
    return () => {
      if (clearOnUnmount && regionRef.current) {
        regionRef.current.textContent = '';
      }
    };
  }, [clearOnUnmount]);

  return (
    <div
      ref={regionRef}
      className="sr-only absolute w-px h-px p-0 -m-px overflow-hidden whitespace-nowrap border-0"
      aria-live={politeness}
      aria-atomic="true"
      role="status"
    >
      {message}
    </div>
  );
};

export default LiveRegion;