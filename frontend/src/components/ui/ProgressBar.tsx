import React from 'react';

interface ProgressBarProps {
  progress: number; // 0-100
  className?: string;
  color?: 'blue' | 'green' | 'purple' | 'orange' | 'red';
  showLabel?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  progress,
  className = '',
  color = 'blue',
  showLabel = false,
  size = 'md',
}) => {
  const clampedProgress = Math.max(0, Math.min(100, progress));
  
  const colorClasses = {
    blue: 'bg-blue-600 dark:bg-blue-400',
    green: 'bg-green-600 dark:bg-green-400',
    purple: 'bg-purple-600 dark:bg-purple-400',
    orange: 'bg-orange-600 dark:bg-orange-400',
    red: 'bg-red-600 dark:bg-red-400',
  };

  const sizeClasses = {
    sm: 'h-1',
    md: 'h-2',
    lg: 'h-3',
  };

  return (
    <div className={className}>
      {showLabel && (
        <div className="flex items-center justify-between mb-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
            Progress
          </span>
          <span className="text-sm text-gray-600 dark:text-gray-400">
            {Math.round(clampedProgress)}%
          </span>
        </div>
      )}
      
      <div className={`w-full bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden ${sizeClasses[size]}`}>
        <div
          className={`${colorClasses[color]} ${sizeClasses[size]} rounded-full transition-all duration-300 ease-out`}
          style={{ width: `${clampedProgress}%` }}
          role="progressbar"
          aria-valuenow={clampedProgress}
          aria-valuemin={0}
          aria-valuemax={100}
          aria-label={`Progress: ${Math.round(clampedProgress)}%`}
        />
      </div>
    </div>
  );
};

export default ProgressBar;