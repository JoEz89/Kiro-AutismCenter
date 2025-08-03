import React, { useState, useRef, useEffect, useCallback } from 'react';
import {
  PlayIcon,
  PauseIcon,
  SpeakerWaveIcon,
  SpeakerXMarkIcon,
  ArrowsPointingOutIcon,
  ArrowsPointingInIcon,
  Cog6ToothIcon,
  ForwardIcon,
  BackwardIcon,
  ShieldCheckIcon,
} from '@heroicons/react/24/outline';
import { useAuth } from '../../hooks/useAuth';
import { useLocalization } from '../../hooks/useLocalization';
import { courseService } from '../../services/courseService';
import { LoadingSpinner } from '../ui/LoadingSpinner';

interface VideoPlayerProps {
  enrollmentId: string;
  moduleId: string;
  onProgress?: (progress: number) => void;
  onComplete?: () => void;
  onTimeUpdate?: (currentTime: number, duration: number) => void;
  className?: string;
  autoPlay?: boolean;
  startTime?: number;
}

interface VideoPlayerState {
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  volume: number;
  isMuted: boolean;
  isFullscreen: boolean;
  isLoading: boolean;
  playbackRate: number;
  buffered: number;
  watchTime: number;
  lastProgressUpdate: number;
}

export const VideoPlayer: React.FC<VideoPlayerProps> = ({
  enrollmentId,
  moduleId,
  onProgress,
  onComplete,
  onTimeUpdate,
  className = '',
  autoPlay = false,
  startTime = 0,
}) => {
  const { t } = useLocalization();
  const { isAuthenticated } = useAuth();
  const videoRef = useRef<HTMLVideoElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const progressRef = useRef<HTMLDivElement>(null);
  const [videoUrl, setVideoUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showControls, setShowControls] = useState(true);
  const [controlsTimeout, setControlsTimeout] = useState<NodeJS.Timeout | null>(null);

  const [state, setState] = useState<VideoPlayerState>({
    isPlaying: false,
    currentTime: startTime,
    duration: 0,
    volume: 1,
    isMuted: false,
    isFullscreen: false,
    isLoading: true,
    playbackRate: 1,
    buffered: 0,
    watchTime: 0,
    lastProgressUpdate: Date.now(),
  });

  // Fetch secure video URL with enhanced security
  useEffect(() => {
    const fetchVideoUrl = async () => {
      if (!isAuthenticated || !enrollmentId || !moduleId) {
        setError(t('courses.accessDenied'));
        return;
      }

      try {
        setState(prev => ({ ...prev, isLoading: true }));
        setError(null);

        const response = await courseService.getVideoUrl(enrollmentId, moduleId);
        
        if (response.success) {
          setVideoUrl(response.data.videoUrl);
          
          // Set up URL expiration check
          const expiresAt = new Date(response.data.expiresAt);
          const timeUntilExpiry = expiresAt.getTime() - Date.now();
          
          if (timeUntilExpiry > 0) {
            setTimeout(() => {
              setError(t('courses.videoExpired'));
              setVideoUrl(null);
            }, timeUntilExpiry);
          }
        } else {
          setError(t('courses.videoLoadError'));
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : t('courses.videoLoadError'));
      } finally {
        setState(prev => ({ ...prev, isLoading: false }));
      }
    };

    fetchVideoUrl();
  }, [enrollmentId, moduleId, isAuthenticated, t]);

  // Video event handlers
  const handleLoadedMetadata = useCallback(() => {
    if (videoRef.current) {
      setState(prev => ({
        ...prev,
        duration: videoRef.current!.duration,
        isLoading: false,
      }));
      
      // Set start time if provided
      if (startTime > 0) {
        videoRef.current.currentTime = startTime;
      }
      
      // Auto-play if enabled
      if (autoPlay) {
        videoRef.current.play().catch(console.error);
      }
    }
  }, [startTime, autoPlay]);

  const handleTimeUpdate = useCallback(() => {
    if (videoRef.current) {
      const currentTime = videoRef.current.currentTime;
      const duration = videoRef.current.duration;
      const progress = duration > 0 ? (currentTime / duration) * 100 : 0;
      const now = Date.now();

      setState(prev => {
        const timeDiff = now - prev.lastProgressUpdate;
        const newWatchTime = prev.isPlaying ? prev.watchTime + timeDiff : prev.watchTime;
        
        return {
          ...prev,
          currentTime,
          watchTime: newWatchTime,
          lastProgressUpdate: now,
        };
      });
      
      // Report progress to parent component
      if (onProgress) {
        onProgress(progress);
      }
      
      // Report time update to parent component
      if (onTimeUpdate) {
        onTimeUpdate(currentTime, duration);
      }

      // Check if video is completed (95% watched)
      if (progress >= 95 && onComplete) {
        onComplete();
      }
    }
  }, [onProgress, onComplete, onTimeUpdate]);

  const handleProgress = useCallback(() => {
    if (videoRef.current && videoRef.current.buffered.length > 0) {
      const buffered = (videoRef.current.buffered.end(0) / videoRef.current.duration) * 100;
      setState(prev => ({ ...prev, buffered }));
    }
  }, []);

  const handlePlay = useCallback(() => {
    setState(prev => ({ 
      ...prev, 
      isPlaying: true,
      lastProgressUpdate: Date.now(),
    }));
  }, []);

  const handlePause = useCallback(() => {
    setState(prev => ({ ...prev, isPlaying: false }));
  }, []);

  const handleVolumeChange = useCallback(() => {
    if (videoRef.current) {
      setState(prev => ({
        ...prev,
        volume: videoRef.current!.volume,
        isMuted: videoRef.current!.muted,
      }));
    }
  }, []);

  // Control handlers
  const togglePlayPause = useCallback(() => {
    if (videoRef.current) {
      if (state.isPlaying) {
        videoRef.current.pause();
      } else {
        videoRef.current.play();
      }
    }
  }, [state.isPlaying]);

  const handleSeek = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (videoRef.current && progressRef.current) {
      const rect = progressRef.current.getBoundingClientRect();
      const clickX = e.clientX - rect.left;
      const width = rect.width;
      const percentage = clickX / width;
      const newTime = percentage * state.duration;
      
      videoRef.current.currentTime = newTime;
    }
  }, [state.duration]);

  const handleVolumeSeek = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (videoRef.current) {
      const rect = e.currentTarget.getBoundingClientRect();
      const clickX = e.clientX - rect.left;
      const width = rect.width;
      const volume = Math.max(0, Math.min(1, clickX / width));
      
      videoRef.current.volume = volume;
      videoRef.current.muted = volume === 0;
    }
  }, []);

  const toggleMute = useCallback(() => {
    if (videoRef.current) {
      videoRef.current.muted = !videoRef.current.muted;
    }
  }, []);

  const skip = useCallback((seconds: number) => {
    if (videoRef.current) {
      const newTime = Math.max(0, Math.min(state.duration, state.currentTime + seconds));
      videoRef.current.currentTime = newTime;
    }
  }, [state.currentTime, state.duration]);

  const changePlaybackRate = useCallback((rate: number) => {
    if (videoRef.current) {
      videoRef.current.playbackRate = rate;
      setState(prev => ({ ...prev, playbackRate: rate }));
    }
  }, []);

  const toggleFullscreen = useCallback(async () => {
    if (!containerRef.current) return;

    try {
      if (!document.fullscreenElement) {
        await containerRef.current.requestFullscreen();
        setState(prev => ({ ...prev, isFullscreen: true }));
      } else {
        await document.exitFullscreen();
        setState(prev => ({ ...prev, isFullscreen: false }));
      }
    } catch (err) {
      console.error('Fullscreen error:', err);
    }
  }, []);

  // Handle fullscreen change
  useEffect(() => {
    const handleFullscreenChange = () => {
      setState(prev => ({ ...prev, isFullscreen: !!document.fullscreenElement }));
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
  }, []);

  // Auto-hide controls
  const resetControlsTimeout = useCallback(() => {
    if (controlsTimeout) {
      clearTimeout(controlsTimeout);
    }

    setShowControls(true);
    
    if (state.isPlaying) {
      const timeout = setTimeout(() => {
        setShowControls(false);
      }, 3000);
      setControlsTimeout(timeout);
    }
  }, [controlsTimeout, state.isPlaying]);

  useEffect(() => {
    resetControlsTimeout();
    return () => {
      if (controlsTimeout) {
        clearTimeout(controlsTimeout);
      }
    };
  }, [resetControlsTimeout]);

  // Keyboard controls
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!videoRef.current) return;

      switch (e.code) {
        case 'Space':
          e.preventDefault();
          togglePlayPause();
          break;
        case 'ArrowLeft':
          e.preventDefault();
          skip(-10);
          break;
        case 'ArrowRight':
          e.preventDefault();
          skip(10);
          break;
        case 'ArrowUp':
          e.preventDefault();
          videoRef.current.volume = Math.min(1, videoRef.current.volume + 0.1);
          break;
        case 'ArrowDown':
          e.preventDefault();
          videoRef.current.volume = Math.max(0, videoRef.current.volume - 0.1);
          break;
        case 'KeyM':
          e.preventDefault();
          toggleMute();
          break;
        case 'KeyF':
          e.preventDefault();
          toggleFullscreen();
          break;
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [togglePlayPause, skip, toggleMute, toggleFullscreen]);

  // Enhanced security measures
  const handleContextMenu = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
  }, []);

  // Prevent video download attempts
  const handleVideoLoad = useCallback(() => {
    if (videoRef.current) {
      // Disable right-click on video element
      videoRef.current.oncontextmenu = (e) => e.preventDefault();
      
      // Prevent drag and drop
      videoRef.current.ondragstart = (e) => e.preventDefault();
      
      // Disable text selection
      videoRef.current.onselectstart = (e) => e.preventDefault();
    }
  }, []);

  // Detect developer tools (basic detection)
  useEffect(() => {
    const detectDevTools = () => {
      const threshold = 160;
      if (window.outerHeight - window.innerHeight > threshold || 
          window.outerWidth - window.innerWidth > threshold) {
        // Developer tools might be open - pause video
        if (videoRef.current && !videoRef.current.paused) {
          videoRef.current.pause();
        }
      }
    };

    const interval = setInterval(detectDevTools, 1000);
    return () => clearInterval(interval);
  }, []);

  // Prevent screenshot attempts (basic)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Prevent common screenshot shortcuts
      if ((e.ctrlKey || e.metaKey) && (e.key === 's' || e.key === 'S')) {
        e.preventDefault();
      }
      if (e.key === 'PrintScreen') {
        e.preventDefault();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Format time
  const formatTime = useCallback((time: number) => {
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }, []);

  if (error) {
    return (
      <div className={`bg-gray-900 rounded-lg flex items-center justify-center aspect-video ${className}`}>
        <div className="text-center text-white">
          <div className="text-red-400 mb-2">⚠️</div>
          <p className="text-lg font-medium mb-2">{t('courses.videoError')}</p>
          <p className="text-sm opacity-75">{error}</p>
        </div>
      </div>
    );
  }

  if (state.isLoading || !videoUrl) {
    return (
      <div className={`bg-gray-900 rounded-lg flex items-center justify-center aspect-video ${className}`}>
        <div className="text-center text-white">
          <LoadingSpinner size="lg" className="mb-4" />
          <p className="text-lg">{t('courses.loadingVideo')}</p>
        </div>
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className={`relative bg-black rounded-lg overflow-hidden aspect-video group ${className}`}
      onMouseMove={resetControlsTimeout}
      onMouseLeave={() => setShowControls(false)}
      onContextMenu={handleContextMenu}
    >
      {/* Video Element */}
      <video
        ref={videoRef}
        src={videoUrl}
        className="w-full h-full object-contain select-none"
        onLoadedMetadata={handleLoadedMetadata}
        onTimeUpdate={handleTimeUpdate}
        onProgress={handleProgress}
        onPlay={handlePlay}
        onPause={handlePause}
        onVolumeChange={handleVolumeChange}
        onContextMenu={handleContextMenu}
        onLoad={handleVideoLoad}
        controlsList="nodownload nofullscreen noremoteplayback noplaybackrate"
        disablePictureInPicture
        disableRemotePlayback
        playsInline
        preload="metadata"
        crossOrigin="anonymous"
        style={{ pointerEvents: 'none' }}
        onDragStart={(e) => e.preventDefault()}
        onSelectStart={(e) => e.preventDefault()}
      />

      {/* Loading Overlay */}
      {state.isLoading && (
        <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
          <LoadingSpinner size="lg" />
        </div>
      )}

      {/* Security Indicator */}
      <div className="absolute top-4 right-4 z-10">
        <div className="flex items-center gap-2 bg-black/50 backdrop-blur-sm rounded-lg px-3 py-2">
          <ShieldCheckIcon className="w-4 h-4 text-green-400" />
          <span className="text-xs text-white font-medium">
            {t('courses.secureVideo')}
          </span>
        </div>
      </div>

      {/* Controls Overlay */}
      <div
        className={`absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent transition-opacity duration-300 ${
          showControls ? 'opacity-100' : 'opacity-0'
        }`}
      >
        {/* Play/Pause Button (Center) */}
        <div className="absolute inset-0 flex items-center justify-center">
          <button
            onClick={togglePlayPause}
            className="w-16 h-16 bg-white/20 hover:bg-white/30 rounded-full flex items-center justify-center transition-colors"
            aria-label={state.isPlaying ? t('courses.pause') : t('courses.play')}
          >
            {state.isPlaying ? (
              <PauseIcon className="w-8 h-8 text-white" />
            ) : (
              <PlayIcon className="w-8 h-8 text-white ml-1" />
            )}
          </button>
        </div>

        {/* Bottom Controls */}
        <div className="absolute bottom-0 left-0 right-0 p-4">
          {/* Progress Bar */}
          <div
            ref={progressRef}
            className="w-full h-2 bg-white/20 rounded-full cursor-pointer mb-4 group/progress"
            onClick={handleSeek}
          >
            {/* Buffered Progress */}
            <div
              className="absolute h-full bg-white/30 rounded-full"
              style={{ width: `${state.buffered}%` }}
            />
            {/* Current Progress */}
            <div
              className="relative h-full bg-blue-500 rounded-full"
              style={{ width: `${(state.currentTime / state.duration) * 100}%` }}
            >
              <div className="absolute right-0 top-1/2 transform -translate-y-1/2 w-4 h-4 bg-blue-500 rounded-full opacity-0 group-hover/progress:opacity-100 transition-opacity" />
            </div>
          </div>

          {/* Control Buttons */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              {/* Play/Pause */}
              <button
                onClick={togglePlayPause}
                className="p-2 hover:bg-white/20 rounded transition-colors"
                aria-label={state.isPlaying ? t('courses.pause') : t('courses.play')}
              >
                {state.isPlaying ? (
                  <PauseIcon className="w-5 h-5 text-white" />
                ) : (
                  <PlayIcon className="w-5 h-5 text-white" />
                )}
              </button>

              {/* Skip Backward */}
              <button
                onClick={() => skip(-10)}
                className="p-2 hover:bg-white/20 rounded transition-colors"
                aria-label={t('courses.skipBackward')}
              >
                <BackwardIcon className="w-5 h-5 text-white" />
              </button>

              {/* Skip Forward */}
              <button
                onClick={() => skip(10)}
                className="p-2 hover:bg-white/20 rounded transition-colors"
                aria-label={t('courses.skipForward')}
              >
                <ForwardIcon className="w-5 h-5 text-white" />
              </button>

              {/* Volume */}
              <div className="flex items-center gap-2 group/volume">
                <button
                  onClick={toggleMute}
                  className="p-2 hover:bg-white/20 rounded transition-colors"
                  aria-label={state.isMuted ? t('courses.unmute') : t('courses.mute')}
                >
                  {state.isMuted || state.volume === 0 ? (
                    <SpeakerXMarkIcon className="w-5 h-5 text-white" />
                  ) : (
                    <SpeakerWaveIcon className="w-5 h-5 text-white" />
                  )}
                </button>
                
                <div
                  className="w-20 h-1 bg-white/20 rounded-full cursor-pointer opacity-0 group-hover/volume:opacity-100 transition-opacity"
                  onClick={handleVolumeSeek}
                >
                  <div
                    className="h-full bg-white rounded-full"
                    style={{ width: `${state.volume * 100}%` }}
                  />
                </div>
              </div>

              {/* Time Display */}
              <span className="text-white text-sm font-mono">
                {formatTime(state.currentTime)} / {formatTime(state.duration)}
              </span>
            </div>

            <div className="flex items-center gap-2">
              {/* Playback Speed */}
              <div className="relative group/speed">
                <button className="p-2 hover:bg-white/20 rounded transition-colors text-white text-sm">
                  {state.playbackRate}x
                </button>
                <div className="absolute bottom-full right-0 mb-2 bg-black/80 rounded p-2 opacity-0 group-hover/speed:opacity-100 transition-opacity">
                  <div className="flex flex-col gap-1">
                    {[0.5, 0.75, 1, 1.25, 1.5, 2].map(rate => (
                      <button
                        key={rate}
                        onClick={() => changePlaybackRate(rate)}
                        className={`px-3 py-1 text-sm rounded transition-colors ${
                          state.playbackRate === rate
                            ? 'bg-blue-500 text-white'
                            : 'text-white hover:bg-white/20'
                        }`}
                      >
                        {rate}x
                      </button>
                    ))}
                  </div>
                </div>
              </div>

              {/* Settings */}
              <button className="p-2 hover:bg-white/20 rounded transition-colors">
                <Cog6ToothIcon className="w-5 h-5 text-white" />
              </button>

              {/* Fullscreen */}
              <button
                onClick={toggleFullscreen}
                className="p-2 hover:bg-white/20 rounded transition-colors"
                aria-label={state.isFullscreen ? t('courses.exitFullscreen') : t('courses.enterFullscreen')}
              >
                {state.isFullscreen ? (
                  <ArrowsPointingInIcon className="w-5 h-5 text-white" />
                ) : (
                  <ArrowsPointingOutIcon className="w-5 h-5 text-white" />
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VideoPlayer;