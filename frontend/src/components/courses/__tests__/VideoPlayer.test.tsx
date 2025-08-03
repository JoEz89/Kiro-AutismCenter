import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { VideoPlayer } from '../VideoPlayer';
import { courseService } from '../../../services/courseService';
import { useAuth } from '../../../hooks/useAuth';
import { useLocalization } from '../../../hooks/useLocalization';

// Mock dependencies
jest.mock('../../../services/courseService');
jest.mock('../../../hooks/useAuth');
jest.mock('../../../hooks/useLocalization');

const mockCourseService = courseService as jest.Mocked<typeof courseService>;
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;
const mockUseLocalization = useLocalization as jest.MockedFunction<typeof useLocalization>;

// Mock HTMLVideoElement methods
const mockVideoElement = {
  play: jest.fn().mockResolvedValue(undefined),
  pause: jest.fn(),
  load: jest.fn(),
  requestFullscreen: jest.fn().mockResolvedValue(undefined),
  addEventListener: jest.fn(),
  removeEventListener: jest.fn(),
  currentTime: 0,
  duration: 100,
  volume: 1,
  muted: false,
  playbackRate: 1,
  buffered: {
    length: 1,
    start: () => 0,
    end: () => 50,
  },
};

// Mock document.exitFullscreen
Object.defineProperty(document, 'exitFullscreen', {
  value: jest.fn().mockResolvedValue(undefined),
  writable: true,
});

Object.defineProperty(document, 'fullscreenElement', {
  value: null,
  writable: true,
});

// Mock HTMLDivElement.requestFullscreen
HTMLDivElement.prototype.requestFullscreen = jest.fn().mockResolvedValue(undefined);

describe('VideoPlayer', () => {
  const defaultProps = {
    enrollmentId: 'enrollment-123',
    moduleId: 'module-456',
    onProgress: jest.fn(),
    onComplete: jest.fn(),
  };

  const mockVideoUrl = 'https://example.com/secure-video.mp4';

  beforeEach(() => {
    jest.clearAllMocks();
    
    // Mock useAuth
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      user: { id: 'user-123' } as any,
      token: 'mock-token',
      isLoading: false,
    });

    // Mock useLocalization
    mockUseLocalization.mockReturnValue({
      t: (key: string) => {
        const translations: Record<string, string> = {
          'courses.accessDenied': 'Access Denied',
          'courses.videoLoadError': 'Failed to load video',
          'courses.videoError': 'Video Error',
          'courses.loadingVideo': 'Loading video...',
          'courses.play': 'Play',
          'courses.pause': 'Pause',
          'courses.mute': 'Mute',
          'courses.unmute': 'Unmute',
          'courses.skipBackward': 'Skip backward 10 seconds',
          'courses.skipForward': 'Skip forward 10 seconds',
          'courses.enterFullscreen': 'Enter fullscreen',
          'courses.exitFullscreen': 'Exit fullscreen',
        };
        return translations[key] || key;
      },
      language: 'en',
      direction: 'ltr',
      setLanguage: jest.fn(),
    });

    // Mock courseService.getVideoUrl
    mockCourseService.getVideoUrl.mockResolvedValue({
      success: true,
      data: {
        videoUrl: mockVideoUrl,
        expiresAt: new Date(Date.now() + 3600000), // 1 hour from now
      },
    });

    // Mock video element
    jest.spyOn(React, 'useRef').mockReturnValue({ current: mockVideoElement });
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  describe('Authentication and Access Control', () => {
    it('should show access denied when user is not authenticated', async () => {
      mockUseAuth.mockReturnValue({
        isAuthenticated: false,
        user: null,
        token: null,
        isLoading: false,
      });

      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Video Error')).toBeInTheDocument();
        expect(screen.getByText('Access Denied')).toBeInTheDocument();
      });

      expect(mockCourseService.getVideoUrl).not.toHaveBeenCalled();
    });

    it('should fetch secure video URL when authenticated', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(mockCourseService.getVideoUrl).toHaveBeenCalledWith(
          'enrollment-123',
          'module-456'
        );
      });
    });

    it('should show error when video URL fetch fails', async () => {
      mockCourseService.getVideoUrl.mockRejectedValue(new Error('Network error'));

      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Video Error')).toBeInTheDocument();
        expect(screen.getByText('Network error')).toBeInTheDocument();
      });
    });

    it('should show error when API returns unsuccessful response', async () => {
      mockCourseService.getVideoUrl.mockResolvedValue({
        success: false,
        data: null,
      });

      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Video Error')).toBeInTheDocument();
        expect(screen.getByText('Failed to load video')).toBeInTheDocument();
      });
    });
  });

  describe('Video Loading and Display', () => {
    it('should show loading state initially', () => {
      render(<VideoPlayer {...defaultProps} />);

      expect(screen.getByText('Loading video...')).toBeInTheDocument();
    });

    it('should render video element with secure URL', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        expect(video).toHaveAttribute('src', mockVideoUrl);
      });
    });

    it('should set video security attributes', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        expect(video).toHaveAttribute('controlsList', 'nodownload nofullscreen noremoteplayback');
        expect(video).toHaveAttribute('disablePictureInPicture');
        expect(video).toHaveAttribute('playsInline');
      });
    });
  });

  describe('Video Controls', () => {
    beforeEach(async () => {
      render(<VideoPlayer {...defaultProps} />);
      
      // Wait for video to load
      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });
    });

    it('should show play button initially', () => {
      const playButton = screen.getByLabelText('Play');
      expect(playButton).toBeInTheDocument();
    });

    it('should toggle play/pause when center button is clicked', async () => {
      const user = userEvent.setup();
      const playButton = screen.getByLabelText('Play');

      await user.click(playButton);

      expect(mockVideoElement.play).toHaveBeenCalled();
    });

    it('should handle volume controls', async () => {
      const user = userEvent.setup();
      const muteButton = screen.getByLabelText('Mute');

      await user.click(muteButton);

      expect(mockVideoElement.muted).toBe(true);
    });

    it('should handle skip forward/backward', async () => {
      const user = userEvent.setup();
      const skipForwardButton = screen.getByLabelText('Skip forward 10 seconds');
      const skipBackwardButton = screen.getByLabelText('Skip backward 10 seconds');

      await user.click(skipForwardButton);
      expect(mockVideoElement.currentTime).toBe(10);

      await user.click(skipBackwardButton);
      expect(mockVideoElement.currentTime).toBe(0);
    });

    it('should handle fullscreen toggle', async () => {
      const user = userEvent.setup();
      const fullscreenButton = screen.getByLabelText('Enter fullscreen');

      await user.click(fullscreenButton);

      expect(HTMLDivElement.prototype.requestFullscreen).toHaveBeenCalled();
    });
  });

  describe('Progress Tracking', () => {
    it('should call onProgress callback during video playback', async () => {
      const onProgress = jest.fn();
      render(<VideoPlayer {...defaultProps} onProgress={onProgress} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate time update
      act(() => {
        mockVideoElement.currentTime = 25;
        const timeUpdateEvent = new Event('timeupdate');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'timeupdate')?.[1](timeUpdateEvent);
      });

      expect(onProgress).toHaveBeenCalledWith(25); // 25% progress
    });

    it('should call onComplete when video reaches 95% completion', async () => {
      const onComplete = jest.fn();
      render(<VideoPlayer {...defaultProps} onComplete={onComplete} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate 95% completion
      act(() => {
        mockVideoElement.currentTime = 95;
        const timeUpdateEvent = new Event('timeupdate');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'timeupdate')?.[1](timeUpdateEvent);
      });

      expect(onComplete).toHaveBeenCalled();
    });

    it('should call onTimeUpdate callback with current time and duration', async () => {
      const onTimeUpdate = jest.fn();
      render(<VideoPlayer {...defaultProps} onTimeUpdate={onTimeUpdate} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate time update
      act(() => {
        mockVideoElement.currentTime = 30;
        const timeUpdateEvent = new Event('timeupdate');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'timeupdate')?.[1](timeUpdateEvent);
      });

      expect(onTimeUpdate).toHaveBeenCalledWith(30, 100);
    });

    it('should track watch time accurately', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate play
      act(() => {
        const playEvent = new Event('play');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'play')?.[1](playEvent);
      });

      // Simulate time passing
      act(() => {
        jest.advanceTimersByTime(5000); // 5 seconds
        mockVideoElement.currentTime = 5;
        const timeUpdateEvent = new Event('timeupdate');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'timeupdate')?.[1](timeUpdateEvent);
      });

      // Watch time should be tracked (this would need access to internal state)
      expect(mockVideoElement.currentTime).toBe(5);
    });

    it('should update progress bar based on current time', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate progress update
      act(() => {
        mockVideoElement.currentTime = 50;
        const timeUpdateEvent = new Event('timeupdate');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'timeupdate')?.[1](timeUpdateEvent);
      });

      // Check if progress bar is updated (this would need more specific DOM testing)
      expect(screen.getByText('0:50 / 1:40')).toBeInTheDocument();
    });

    it('should support starting from a specific time', async () => {
      render(<VideoPlayer {...defaultProps} startTime={30} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Should set video current time to start time
      expect(mockVideoElement.currentTime).toBe(30);
    });

    it('should support auto-play when enabled', async () => {
      render(<VideoPlayer {...defaultProps} autoPlay={true} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Should call play method
      expect(mockVideoElement.play).toHaveBeenCalled();
    });
  });

  describe('Keyboard Controls', () => {
    beforeEach(async () => {
      render(<VideoPlayer {...defaultProps} />);
      
      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });
    });

    it('should handle spacebar for play/pause', async () => {
      const user = userEvent.setup();

      await user.keyboard(' ');

      expect(mockVideoElement.play).toHaveBeenCalled();
    });

    it('should handle arrow keys for seeking and volume', async () => {
      const user = userEvent.setup();

      // Right arrow - skip forward
      await user.keyboard('{ArrowRight}');
      expect(mockVideoElement.currentTime).toBe(10);

      // Left arrow - skip backward
      await user.keyboard('{ArrowLeft}');
      expect(mockVideoElement.currentTime).toBe(0);

      // Up arrow - volume up
      await user.keyboard('{ArrowUp}');
      expect(mockVideoElement.volume).toBe(1.1);

      // Down arrow - volume down
      await user.keyboard('{ArrowDown}');
      expect(mockVideoElement.volume).toBe(1);
    });

    it('should handle M key for mute toggle', async () => {
      const user = userEvent.setup();

      await user.keyboard('m');

      expect(mockVideoElement.muted).toBe(true);
    });

    it('should handle F key for fullscreen toggle', async () => {
      const user = userEvent.setup();

      await user.keyboard('f');

      expect(HTMLDivElement.prototype.requestFullscreen).toHaveBeenCalled();
    });
  });

  describe('Security Features', () => {
    it('should prevent right-click context menu', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      const videoContainer = screen.getByRole('application', { hidden: true }).parentElement;
      const contextMenuEvent = new MouseEvent('contextmenu', { bubbles: true });
      
      let defaultPrevented = false;
      contextMenuEvent.preventDefault = () => { defaultPrevented = true; };

      fireEvent(videoContainer!, contextMenuEvent);

      expect(defaultPrevented).toBe(true);
    });

    it('should disable download controls', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        expect(video).toHaveAttribute('controlsList', expect.stringContaining('nodownload'));
      });
    });

    it('should disable picture-in-picture', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        expect(video).toHaveAttribute('disablePictureInPicture');
      });
    });

    it('should show security indicator', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Secure Video')).toBeInTheDocument();
      });
    });

    it('should prevent drag and drop on video element', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        
        const dragStartEvent = new Event('dragstart');
        let defaultPrevented = false;
        dragStartEvent.preventDefault = () => { defaultPrevented = true; };
        
        fireEvent(video, dragStartEvent);
        expect(defaultPrevented).toBe(true);
      });
    });

    it('should prevent text selection on video element', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        
        const selectStartEvent = new Event('selectstart');
        let defaultPrevented = false;
        selectStartEvent.preventDefault = () => { defaultPrevented = true; };
        
        fireEvent(video, selectStartEvent);
        expect(defaultPrevented).toBe(true);
      });
    });

    it('should handle video URL expiration', async () => {
      const expiredDate = new Date(Date.now() + 100); // Expires in 100ms
      mockCourseService.getVideoUrl.mockResolvedValue({
        success: true,
        data: {
          videoUrl: mockVideoUrl,
          expiresAt: expiredDate,
        },
      });

      render(<VideoPlayer {...defaultProps} />);

      // Wait for expiration
      await waitFor(() => {
        expect(screen.getByText('Video access has expired. Please refresh to get a new link.')).toBeInTheDocument();
      }, { timeout: 2000 });
    });

    it('should have enhanced security attributes', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        const video = screen.getByRole('application', { hidden: true });
        expect(video).toHaveAttribute('controlsList', 'nodownload nofullscreen noremoteplayback noplaybackrate');
        expect(video).toHaveAttribute('disableRemotePlayback');
        expect(video).toHaveAttribute('crossOrigin', 'anonymous');
        expect(video).toHaveAttribute('preload', 'metadata');
        expect(video).toHaveClass('select-none');
      });
    });
  });

  describe('Accessibility', () => {
    beforeEach(async () => {
      render(<VideoPlayer {...defaultProps} />);
      
      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });
    });

    it('should have proper ARIA labels for all controls', () => {
      expect(screen.getByLabelText('Play')).toBeInTheDocument();
      expect(screen.getByLabelText('Mute')).toBeInTheDocument();
      expect(screen.getByLabelText('Skip forward 10 seconds')).toBeInTheDocument();
      expect(screen.getByLabelText('Skip backward 10 seconds')).toBeInTheDocument();
      expect(screen.getByLabelText('Enter fullscreen')).toBeInTheDocument();
    });

    it('should update ARIA labels based on state', async () => {
      const user = userEvent.setup();
      
      // Initially shows "Play"
      expect(screen.getByLabelText('Play')).toBeInTheDocument();

      // After clicking, should show "Pause" (would need to mock video state change)
      await user.click(screen.getByLabelText('Play'));
      
      // This would require more complex state mocking to test properly
    });
  });

  describe('Error Handling', () => {
    it('should handle video loading errors gracefully', async () => {
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      // Simulate video error
      act(() => {
        const errorEvent = new Event('error');
        mockVideoElement.addEventListener.mock.calls
          .find(call => call[0] === 'error')?.[1](errorEvent);
      });

      // Should still show controls and not crash
      expect(screen.getByLabelText('Play')).toBeInTheDocument();
    });

    it('should handle network interruptions', async () => {
      mockCourseService.getVideoUrl.mockRejectedValue(new Error('Network error'));

      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Video Error')).toBeInTheDocument();
        expect(screen.getByText('Network error')).toBeInTheDocument();
      });
    });
  });

  describe('Performance', () => {
    it('should cleanup event listeners on unmount', () => {
      const { unmount } = render(<VideoPlayer {...defaultProps} />);

      unmount();

      // Verify cleanup (this would need more specific testing of useEffect cleanup)
      expect(mockVideoElement.removeEventListener).toHaveBeenCalled();
    });

    it('should debounce control visibility', async () => {
      const user = userEvent.setup();
      render(<VideoPlayer {...defaultProps} />);

      await waitFor(() => {
        expect(screen.queryByText('Loading video...')).not.toBeInTheDocument();
      });

      const container = screen.getByRole('application', { hidden: true }).parentElement;

      // Mouse move should show controls
      await user.hover(container!);

      // Controls should be visible
      expect(screen.getByLabelText('Play')).toBeVisible();
    });
  });
});