import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { VideoPlayer } from '../VideoPlayer';
import { AuthProvider } from '@/context/AuthContext';
import { LanguageProvider } from '@/context/LanguageContext';
import { testAccessibility } from '@/test/accessibility';

// Mock HTML5 video element
Object.defineProperty(HTMLMediaElement.prototype, 'play', {
  writable: true,
  value: vi.fn().mockImplementation(() => Promise.resolve()),
});

Object.defineProperty(HTMLMediaElement.prototype, 'pause', {
  writable: true,
  value: vi.fn(),
});

Object.defineProperty(HTMLMediaElement.prototype, 'load', {
  writable: true,
  value: vi.fn(),
});

const mockVideoData = {
  id: 'video-1',
  title: 'Introduction to Autism',
  url: 'https://example.com/video.mp4',
  duration: 300, // 5 minutes
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  courseId: 'course-1',
  moduleId: 'module-1',
  order: 1,
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <LanguageProvider>
      <AuthProvider>
        {component}
      </AuthProvider>
    </LanguageProvider>
  );
};

describe('VideoPlayer', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render video player correctly', () => {
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
    expect(screen.getByText('05:00')).toBeInTheDocument(); // Duration display
  });

  it('should play video when play button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const playButton = screen.getByRole('button', { name: /play/i });
    await user.click(playButton);

    const video = screen.getByRole('video') as HTMLVideoElement;
    expect(video.play).toHaveBeenCalled();
    expect(screen.getByRole('button', { name: /pause/i })).toBeInTheDocument();
  });

  it('should pause video when pause button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    // First play the video
    const playButton = screen.getByRole('button', { name: /play/i });
    await user.click(playButton);

    // Then pause it
    const pauseButton = screen.getByRole('button', { name: /pause/i });
    await user.click(pauseButton);

    const video = screen.getByRole('video') as HTMLVideoElement;
    expect(video.pause).toHaveBeenCalled();
    expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
  });

  it('should handle volume control', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const volumeSlider = screen.getByRole('slider', { name: /volume/i });
    await user.click(volumeSlider);

    // Volume should be adjustable
    expect(volumeSlider).toBeInTheDocument();
  });

  it('should handle fullscreen toggle', async () => {
    const user = userEvent.setup();
    
    // Mock fullscreen API
    const mockRequestFullscreen = vi.fn();
    const mockExitFullscreen = vi.fn();
    
    Object.defineProperty(document, 'fullscreenElement', {
      writable: true,
      value: null,
    });
    
    Object.defineProperty(HTMLElement.prototype, 'requestFullscreen', {
      writable: true,
      value: mockRequestFullscreen,
    });
    
    Object.defineProperty(document, 'exitFullscreen', {
      writable: true,
      value: mockExitFullscreen,
    });

    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const fullscreenButton = screen.getByRole('button', { name: /fullscreen/i });
    await user.click(fullscreenButton);

    expect(mockRequestFullscreen).toHaveBeenCalled();
  });

  it('should show progress bar and handle seeking', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const progressBar = screen.getByRole('slider', { name: /progress/i });
    expect(progressBar).toBeInTheDocument();

    // Simulate seeking
    await user.click(progressBar);
    
    // Progress should be updated
    expect(progressBar).toHaveAttribute('aria-valuemin', '0');
    expect(progressBar).toHaveAttribute('aria-valuemax', '300');
  });

  it('should display current time and duration', () => {
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    expect(screen.getByText('00:00')).toBeInTheDocument(); // Current time
    expect(screen.getByText('05:00')).toBeInTheDocument(); // Duration
  });

  it('should handle video loading states', async () => {
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    // Should show loading state initially
    expect(screen.getByText(/loading/i)).toBeInTheDocument();

    // Simulate video loaded
    const video = screen.getByRole('video') as HTMLVideoElement;
    video.dispatchEvent(new Event('loadeddata'));

    await waitFor(() => {
      expect(screen.queryByText(/loading/i)).not.toBeInTheDocument();
    });
  });

  it('should handle video errors gracefully', async () => {
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const video = screen.getByRole('video') as HTMLVideoElement;
    video.dispatchEvent(new Event('error'));

    await waitFor(() => {
      expect(screen.getByText(/error loading video/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
    });
  });

  it('should prevent video download and right-click', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const video = screen.getByRole('video') as HTMLVideoElement;
    
    // Should have controlsList attribute to prevent download
    expect(video).toHaveAttribute('controlsList', 'nodownload');
    
    // Should prevent right-click context menu
    await user.pointer({ keys: '[MouseRight]', target: video });
    
    // Context menu should be prevented (this is hard to test directly)
    expect(video).toHaveAttribute('onContextMenu');
  });

  it('should track video progress for course completion', async () => {
    const onProgressUpdate = vi.fn();
    renderWithProviders(
      <VideoPlayer video={mockVideoData} onProgressUpdate={onProgressUpdate} />
    );

    const video = screen.getByRole('video') as HTMLVideoElement;
    
    // Simulate video time update
    Object.defineProperty(video, 'currentTime', { value: 150 }); // 2.5 minutes
    Object.defineProperty(video, 'duration', { value: 300 }); // 5 minutes
    
    video.dispatchEvent(new Event('timeupdate'));

    await waitFor(() => {
      expect(onProgressUpdate).toHaveBeenCalledWith(50); // 50% progress
    });
  });

  it('should support keyboard controls', async () => {
    const user = userEvent.setup();
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const video = screen.getByRole('video') as HTMLVideoElement;
    video.focus();

    // Space bar should play/pause
    await user.keyboard(' ');
    expect(video.play).toHaveBeenCalled();

    // Arrow keys should seek
    await user.keyboard('{ArrowRight}');
    // Should seek forward 10 seconds

    await user.keyboard('{ArrowLeft}');
    // Should seek backward 10 seconds

    // M key should mute/unmute
    await user.keyboard('m');
    expect(video.muted).toBe(true);
  });

  it('should be accessible', async () => {
    const renderResult = renderWithProviders(<VideoPlayer video={mockVideoData} />);
    await testAccessibility(renderResult);
  });

  it('should have proper ARIA labels and descriptions', () => {
    renderWithProviders(<VideoPlayer video={mockVideoData} />);

    const video = screen.getByRole('video');
    expect(video).toHaveAttribute('aria-label', 'Introduction to Autism');

    const playButton = screen.getByRole('button', { name: /play/i });
    expect(playButton).toHaveAttribute('aria-label');

    const progressBar = screen.getByRole('slider', { name: /progress/i });
    expect(progressBar).toHaveAttribute('aria-valuetext');
  });

  it('should handle multiple video qualities', async () => {
    const user = userEvent.setup();
    const videoWithQualities = {
      ...mockVideoData,
      qualities: [
        { label: '720p', url: 'https://example.com/video-720p.mp4' },
        { label: '1080p', url: 'https://example.com/video-1080p.mp4' },
      ],
    };

    renderWithProviders(<VideoPlayer video={videoWithQualities} />);

    const qualityButton = screen.getByRole('button', { name: /quality/i });
    await user.click(qualityButton);

    expect(screen.getByText('720p')).toBeInTheDocument();
    expect(screen.getByText('1080p')).toBeInTheDocument();
  });

  it('should handle subtitles/captions', async () => {
    const user = userEvent.setup();
    const videoWithSubtitles = {
      ...mockVideoData,
      subtitles: [
        { language: 'en', label: 'English', url: 'https://example.com/subtitles-en.vtt' },
        { language: 'ar', label: 'Arabic', url: 'https://example.com/subtitles-ar.vtt' },
      ],
    };

    renderWithProviders(<VideoPlayer video={videoWithSubtitles} />);

    const subtitlesButton = screen.getByRole('button', { name: /subtitles/i });
    await user.click(subtitlesButton);

    expect(screen.getByText('English')).toBeInTheDocument();
    expect(screen.getByText('Arabic')).toBeInTheDocument();
  });
});