describe('Course Learning Flow', () => {
  beforeEach(() => {
    // Login as a user with course access
    cy.loginAsUser();
    cy.visit('/');
  });

  describe('Course Enrollment and Access', () => {
    it('should allow user to browse and enroll in courses', () => {
      // Navigate to courses page
      cy.get('[data-testid="courses-nav-link"]').click();
      cy.url().should('include', '/courses');

      // Should display course catalog
      cy.get('[data-testid="course-catalog"]').should('be.visible');
      cy.get('[data-testid="course-card"]').should('have.length.at.least', 1);

      // Click on a course to view details
      cy.get('[data-testid="course-card"]').first().click();
      cy.url().should('match', /\/courses\/[^\/]+$/);

      // Should show course details
      cy.get('[data-testid="course-title"]').should('be.visible');
      cy.get('[data-testid="course-description"]').should('be.visible');
      cy.get('[data-testid="course-modules"]').should('be.visible');

      // Enroll in course
      cy.get('[data-testid="enroll-button"]').click();

      // Should show enrollment confirmation
      cy.get('[data-testid="enrollment-success"]')
        .should('be.visible')
        .and('contain', 'Successfully enrolled');

      // Should now show "Start Learning" button
      cy.get('[data-testid="start-learning-button"]').should('be.visible');
    });

    it('should prevent access to non-enrolled courses', () => {
      // Try to access course learning page directly without enrollment
      cy.visit('/courses/non-enrolled-course/learn');

      // Should show enrollment required message
      cy.get('[data-testid="enrollment-required"]')
        .should('be.visible')
        .and('contain', 'You are not enrolled');

      // Should show enroll button
      cy.get('[data-testid="enroll-button"]').should('be.visible');

      // Should not show video player
      cy.get('[data-testid="video-player"]').should('not.exist');
    });

    it('should handle expired course access', () => {
      // Mock expired enrollment
      cy.window().then((win) => {
        win.localStorage.setItem('mockExpiredEnrollment', 'true');
      });

      cy.visit('/courses/expired-course/learn');

      // Should show access expired message
      cy.get('[data-testid="access-expired"]')
        .should('be.visible')
        .and('contain', 'Course access has expired');

      // Should show renew access button
      cy.get('[data-testid="renew-access-button"]').should('be.visible');
    });
  });

  describe('Video Learning Experience', () => {
    beforeEach(() => {
      // Ensure user is enrolled in a course
      cy.visit('/courses/test-course/learn');
      cy.get('[data-testid="video-player"]').should('be.visible');
    });

    it('should play video and track progress', () => {
      // Should show video player with controls
      cy.get('[data-testid="video-player"]').should('be.visible');
      cy.get('[data-testid="play-button"]').should('be.visible');
      cy.get('[data-testid="progress-bar"]').should('be.visible');

      // Play video
      cy.get('[data-testid="play-button"]').click();

      // Should show pause button
      cy.get('[data-testid="pause-button"]').should('be.visible');

      // Should track video progress
      cy.get('[data-testid="current-time"]').should('not.contain', '00:00');

      // Should update course progress
      cy.get('[data-testid="course-progress"]').should('be.visible');
    });

    it('should support video controls and seeking', () => {
      // Test volume control
      cy.get('[data-testid="volume-button"]').click();
      cy.get('[data-testid="volume-slider"]').should('be.visible');
      cy.get('[data-testid="volume-slider"]').click();

      // Test seeking
      cy.get('[data-testid="progress-bar"]').click('center');
      cy.get('[data-testid="current-time"]').should('not.contain', '00:00');

      // Test fullscreen
      cy.get('[data-testid="fullscreen-button"]').click();
      // Note: Fullscreen testing in Cypress is limited due to browser restrictions

      // Test playback speed
      cy.get('[data-testid="speed-button"]').click();
      cy.get('[data-testid="speed-option-1.5"]').click();
    });

    it('should prevent video download and right-click', () => {
      const video = cy.get('[data-testid="video-element"]');

      // Should have download prevention attributes
      video.should('have.attr', 'controlsList', 'nodownload');

      // Right-click should be prevented
      video.rightclick();
      cy.get('.context-menu').should('not.exist');

      // Should not allow direct video URL access
      video.should('have.attr', 'src').and('include', 'signed-url');
    });

    it('should support keyboard controls', () => {
      cy.get('[data-testid="video-element"]').focus();

      // Space bar should play/pause
      cy.get('[data-testid="video-element"]').type(' ');
      cy.get('[data-testid="pause-button"]').should('be.visible');

      cy.get('[data-testid="video-element"]').type(' ');
      cy.get('[data-testid="play-button"]').should('be.visible');

      // Arrow keys should seek
      cy.get('[data-testid="video-element"]').type('{rightarrow}');
      cy.get('[data-testid="current-time"]').should('not.contain', '00:00');

      // M key should mute
      cy.get('[data-testid="video-element"]').type('m');
      cy.get('[data-testid="volume-button"]').should('have.class', 'muted');
    });
  });

  describe('Course Navigation and Progress', () => {
    beforeEach(() => {
      cy.visit('/courses/test-course/learn');
    });

    it('should navigate between course modules and videos', () => {
      // Should show course navigation
      cy.get('[data-testid="course-navigation"]').should('be.visible');
      cy.get('[data-testid="module-list"]').should('be.visible');

      // Should show current video highlighted
      cy.get('[data-testid="current-video"]').should('have.class', 'active');

      // Click next video
      cy.get('[data-testid="next-video-button"]').click();

      // Should load next video
      cy.get('[data-testid="video-title"]').should('not.contain', 'Previous Video Title');

      // Should update navigation
      cy.get('[data-testid="current-video"]').should('have.class', 'active');

      // Click previous video
      cy.get('[data-testid="previous-video-button"]').click();

      // Should go back to previous video
      cy.get('[data-testid="video-title"]').should('contain', 'Previous Video Title');
    });

    it('should track and display course progress', () => {
      // Should show overall course progress
      cy.get('[data-testid="course-progress-bar"]').should('be.visible');
      cy.get('[data-testid="progress-percentage"]').should('be.visible');

      // Should show module progress
      cy.get('[data-testid="module-progress"]').should('be.visible');

      // Complete a video (simulate watching to end)
      cy.get('[data-testid="video-element"]').then(($video) => {
        const video = $video[0] as HTMLVideoElement;
        video.currentTime = video.duration - 1;
        video.dispatchEvent(new Event('timeupdate'));
      });

      // Should update progress
      cy.get('[data-testid="video-completed"]').should('be.visible');
      cy.get('[data-testid="progress-percentage"]').should('not.contain', '0%');
    });

    it('should handle bookmark functionality', () => {
      // Should show bookmark button
      cy.get('[data-testid="bookmark-button"]').should('be.visible');

      // Click bookmark
      cy.get('[data-testid="bookmark-button"]').click();

      // Should show bookmark confirmation
      cy.get('[data-testid="bookmark-success"]')
        .should('be.visible')
        .and('contain', 'Bookmarked');

      // Button should show active state
      cy.get('[data-testid="bookmark-button"]').should('have.class', 'active');

      // Should appear in bookmarks list
      cy.get('[data-testid="bookmarks-button"]').click();
      cy.get('[data-testid="bookmarks-list"]').should('contain', 'Current Video Title');

      // Remove bookmark
      cy.get('[data-testid="remove-bookmark"]').click();
      cy.get('[data-testid="bookmark-button"]').should('not.have.class', 'active');
    });
  });

  describe('Course Completion and Certificates', () => {
    it('should handle course completion and certificate generation', () => {
      // Navigate to course with high progress
      cy.visit('/courses/nearly-complete-course/learn');

      // Complete the final video
      cy.get('[data-testid="video-element"]').then(($video) => {
        const video = $video[0] as HTMLVideoElement;
        video.currentTime = video.duration;
        video.dispatchEvent(new Event('ended'));
      });

      // Should show course completion
      cy.get('[data-testid="course-completed"]')
        .should('be.visible')
        .and('contain', 'Congratulations');

      // Should show certificate download
      cy.get('[data-testid="download-certificate"]').should('be.visible');

      // Click download certificate
      cy.get('[data-testid="download-certificate"]').click();

      // Should trigger download
      cy.get('[data-testid="certificate-downloading"]').should('be.visible');

      // Should show certificate preview
      cy.get('[data-testid="certificate-preview"]').should('be.visible');
    });

    it('should display course completion in progress dashboard', () => {
      // Complete a course first
      cy.visit('/courses/test-course/learn');
      
      // Simulate course completion
      cy.window().then((win) => {
        win.localStorage.setItem('courseCompleted', 'true');
      });

      // Navigate to progress dashboard
      cy.visit('/courses/progress');

      // Should show completed course
      cy.get('[data-testid="completed-courses"]').should('be.visible');
      cy.get('[data-testid="completion-badge"]').should('be.visible');
      cy.get('[data-testid="certificate-link"]').should('be.visible');

      // Should show completion date
      cy.get('[data-testid="completion-date"]').should('be.visible');
    });
  });

  describe('Multiple Device Prevention', () => {
    it('should detect and prevent multiple device access', () => {
      // Start video on first "device"
      cy.visit('/courses/test-course/learn');
      cy.get('[data-testid="play-button"]').click();

      // Simulate access from another device
      cy.window().then((win) => {
        win.localStorage.setItem('multipleDeviceDetected', 'true');
      });

      cy.reload();

      // Should show multiple device warning
      cy.get('[data-testid="multiple-device-warning"]')
        .should('be.visible')
        .and('contain', 'Multiple device access detected');

      // Should pause video
      cy.get('[data-testid="play-button"]').should('be.visible');

      // Should show device management options
      cy.get('[data-testid="manage-devices"]').should('be.visible');
    });

    it('should allow device switching with confirmation', () => {
      // Trigger multiple device scenario
      cy.visit('/courses/test-course/learn');
      cy.window().then((win) => {
        win.localStorage.setItem('multipleDeviceDetected', 'true');
      });
      cy.reload();

      // Should show device switching option
      cy.get('[data-testid="switch-device"]').should('be.visible');

      // Click switch device
      cy.get('[data-testid="switch-device"]').click();

      // Should show confirmation dialog
      cy.get('[data-testid="switch-device-confirm"]').should('be.visible');

      // Confirm switch
      cy.get('[data-testid="confirm-switch"]').click();

      // Should allow video access
      cy.get('[data-testid="video-player"]').should('be.visible');
      cy.get('[data-testid="multiple-device-warning"]').should('not.exist');
    });
  });

  describe('Accessibility in Course Learning', () => {
    beforeEach(() => {
      cy.visit('/courses/test-course/learn');
    });

    it('should be accessible with keyboard navigation', () => {
      // Should be able to navigate with keyboard
      cy.get('body').tab();
      cy.focused().should('have.attr', 'data-testid', 'skip-to-content');

      // Navigate to video controls
      cy.focused().tab();
      cy.focused().should('have.attr', 'data-testid', 'play-button');

      // Navigate to course navigation
      cy.get('[data-testid="course-navigation"]').within(() => {
        cy.get('[data-testid="next-video"]').focus();
        cy.focused().should('be.visible');
      });
    });

    it('should support screen readers', () => {
      // Check for proper ARIA labels
      cy.get('[data-testid="video-player"]').should('have.attr', 'aria-label');
      cy.get('[data-testid="play-button"]').should('have.attr', 'aria-label');
      cy.get('[data-testid="progress-bar"]').should('have.attr', 'role', 'slider');

      // Check for live regions
      cy.get('[data-testid="video-status"]').should('have.attr', 'aria-live');
      cy.get('[data-testid="progress-status"]').should('have.attr', 'aria-live');
    });

    it('should support captions and transcripts', () => {
      // Should show captions button
      cy.get('[data-testid="captions-button"]').should('be.visible');

      // Click captions
      cy.get('[data-testid="captions-button"]').click();

      // Should show captions menu
      cy.get('[data-testid="captions-menu"]').should('be.visible');
      cy.get('[data-testid="captions-english"]').should('be.visible');
      cy.get('[data-testid="captions-arabic"]').should('be.visible');

      // Enable captions
      cy.get('[data-testid="captions-english"]').click();

      // Should show captions on video
      cy.get('[data-testid="video-captions"]').should('be.visible');

      // Should show transcript option
      cy.get('[data-testid="transcript-button"]').should('be.visible');
      cy.get('[data-testid="transcript-button"]').click();
      cy.get('[data-testid="transcript-panel"]').should('be.visible');
    });

    it('should pass accessibility audit', () => {
      cy.checkA11y('[data-testid="course-learning-page"]');
    });
  });

  describe('Error Handling in Course Learning', () => {
    it('should handle video loading errors gracefully', () => {
      // Mock video error
      cy.visit('/courses/test-course/learn');
      
      cy.get('[data-testid="video-element"]').then(($video) => {
        const video = $video[0] as HTMLVideoElement;
        video.dispatchEvent(new Event('error'));
      });

      // Should show error message
      cy.get('[data-testid="video-error"]')
        .should('be.visible')
        .and('contain', 'Error loading video');

      // Should show retry button
      cy.get('[data-testid="retry-video"]').should('be.visible');

      // Click retry
      cy.get('[data-testid="retry-video"]').click();

      // Should attempt to reload video
      cy.get('[data-testid="video-loading"]').should('be.visible');
    });

    it('should handle network connectivity issues', () => {
      // Simulate network error
      cy.intercept('GET', '**/api/courses/**', { forceNetworkError: true });

      cy.visit('/courses/test-course/learn');

      // Should show network error
      cy.get('[data-testid="network-error"]')
        .should('be.visible')
        .and('contain', 'Network connection error');

      // Should show retry button
      cy.get('[data-testid="retry-connection"]').should('be.visible');
    });

    it('should handle session expiration during learning', () => {
      // Start learning
      cy.visit('/courses/test-course/learn');
      cy.get('[data-testid="play-button"]').click();

      // Simulate session expiration
      cy.window().then((win) => {
        win.localStorage.removeItem('authToken');
      });

      // Try to update progress (should fail)
      cy.get('[data-testid="video-element"]').then(($video) => {
        const video = $video[0] as HTMLVideoElement;
        video.currentTime = video.duration / 2;
        video.dispatchEvent(new Event('timeupdate'));
      });

      // Should show session expired message
      cy.get('[data-testid="session-expired"]')
        .should('be.visible')
        .and('contain', 'Session expired');

      // Should redirect to login
      cy.get('[data-testid="login-redirect"]').click();
      cy.url().should('include', '/login');
    });
  });
});