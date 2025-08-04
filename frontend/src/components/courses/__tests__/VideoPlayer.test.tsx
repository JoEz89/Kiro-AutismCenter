import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
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
  currentTime: 0,
  isCompleted: false,
};

const TestWrapper = ({ children }: { children: React.ReactNode }) => (
  <LanguageProvider>
    <AuthProvider>
      {children}
    </AuthProvider>
  </LanguageProvider>
);

describe('VideoPlayer', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render video player with controls', () => {
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    expect(screen.getByRole('application', { name: /video player/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /mute/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /fullscreen/i })).toBeInTheDocument();
  });

  it('should play video when play button is clicked', async () => {
    const user = userEvent.setup();
    const mockPlay = vi.fn().mockResolvedValue(undefined);
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    video.play = mockPlay;

    const playButton = screen.getByRole('button', { name: /play/i });
    await user.click(playButton);

    expect(mockPlay).toHaveBeenCalled();
  });

  it('should pause video when pause button is clicked', async () => {
    const user = userEvent.setup();
    const mockPause = vi.fn();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    video.pause = mockPause;
    
    // Simulate video playing state
    Object.defineProperty(video, 'paused', { value: false });

    const pauseButton = screen.getByRole('button', { name: /pause/i });
    await user.click(pauseButton);

    expect(mockPause).toHaveBeenCalled();
  });

  it('should update progress when video time changes', async () => {
    const mockOnProgress = vi.fn();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={mockOnProgress} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    
    // Simulate time update
    Object.defineProperty(video, 'currentTime', { value: 150 });
    fireEvent.timeUpdate(video);

    expect(mockOnProgress).toHaveBeenCalledWith(150);
  });

  it('should handle volume control', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const volumeSlider = screen.getByRole('slider', { name: /volume/i });
    await user.click(volumeSlider);

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    expect(video.volume).toBeDefined();
  });

  it('should toggle mute when mute button is clicked', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const muteButton = screen.getByRole('button', { name: /mute/i });
    await user.click(muteButton);

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    expect(video.muted).toBe(true);

    await user.click(muteButton);
    expect(video.muted).toBe(false);
  });

  it('should handle fullscreen toggle', async () => {
    const user = userEvent.setup();
    
    // Mock fullscreen API
    const mockRequestFullscreen = vi.fn();
    const mockExitFullscreen = vi.fn();
    
    Object.defineProperty(document, 'fullscreenElement', { value: null, writable: true });
    Object.defineProperty(document, 'exitFullscreen', { value: mockExitFullscreen });

    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const container = screen.getByRole('application');
    container.requestFullscreen = mockRequestFullscreen;

    const fullscreenButton = screen.getByRole('button', { name: /fullscreen/i });
    await user.click(fullscreenButton);

    expect(mockRequestFullscreen).toHaveBeenCalled();
  });

  it('should prevent video download and right-click', () => {
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    
    expect(video).toHaveAttribute('controlsList', 'nodownload');
    expect(video).toHaveAttribute('onContextMenu', expect.any(String));
  });

  it('should display video title', () => {
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
  });

  it('should show loading state while video is loading', () => {
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    
    // Simulate loading state
    Object.defineProperty(video, 'readyState', { value: 0 });
    fireEvent.loadStart(video);

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should handle video error gracefully', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    fireEvent.error(video);

    expect(screen.getByText(/error loading video/i)).toBeInTheDocument();
    
    consoleSpy.mockRestore();
  });

  it('should be accessible', async () => {
    const renderResult = render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    await testAccessibility(renderResult);
  });

  it('should support keyboard controls', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    const mockPlay = vi.fn().mockResolvedValue(undefined);
    const mockPause = vi.fn();
    video.play = mockPlay;
    video.pause = mockPause;

    // Focus on video player
    video.focus();

    // Test spacebar for play/pause
    await user.keyboard(' ');
    expect(mockPlay).toHaveBeenCalled();

    // Test arrow keys for seeking
    await user.keyboard('{ArrowRight}');
    expect(video.currentTime).toBeDefined();

    await user.keyboard('{ArrowLeft}');
    expect(video.currentTime).toBeDefined();
  });

  it('should track video completion', async () => {
    const mockOnProgress = vi.fn();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={mockOnProgress} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    
    // Simulate video ending
    Object.defineProperty(video, 'currentTime', { value: 300 });
    Object.defineProperty(video, 'duration', { value: 300 });
    fireEvent.ended(video);

    expect(mockOnProgress).toHaveBeenCalledWith(300, true);
  });

  it('should resume from saved position', () => {
    const videoWithProgress = { ...mockVideoData, currentTime: 150 };
    
    render(
      <TestWrapper>
        <VideoPlayer video={videoWithProgress} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    
    // Should set current time to saved position
    expect(video.currentTime).toBe(150);
  });

  it('should show playback speed controls', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const speedButton = screen.getByRole('button', { name: /playback speed/i });
    await user.click(speedButton);

    expect(screen.getByText('0.5x')).toBeInTheDocument();
    expect(screen.getByText('1x')).toBeInTheDocument();
    expect(screen.getByText('1.25x')).toBeInTheDocument();
    expect(screen.getByText('1.5x')).toBeInTheDocument();
    expect(screen.getByText('2x')).toBeInTheDocument();
  });

  it('should change playback speed when speed option is selected', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <VideoPlayer video={mockVideoData} onProgress={vi.fn()} />
      </TestWrapper>
    );

    const speedButton = screen.getByRole('button', { name: /playback speed/i });
    await user.click(speedButton);

    const speed125 = screen.getByText('1.25x');
    await user.click(speed125);

    const video = screen.getByRole('application').querySelector('video') as HTMLVideoElement;
    expect(video.playbackRate).toBe(1.25);
  });
});