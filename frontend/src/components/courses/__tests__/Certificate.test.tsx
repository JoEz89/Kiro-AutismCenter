import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { Certificate } from '../Certificate';
import { LanguageContext } from '../../../context/LanguageContext';
import type { Course, Enrollment } from '../../../types';

const mockCourse: Course = {
  id: 'course-1',
  titleEn: 'Introduction to Autism',
  titleAr: 'مقدمة في التوحد',
  descriptionEn: 'Learn the basics of autism',
  descriptionAr: 'تعلم أساسيات التوحد',
  duration: 10800, // 3 hours in seconds
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  modules: [],
  isActive: true,
};

const mockCompletedEnrollment: Enrollment = {
  id: 'enrollment-1',
  userId: 'user-1',
  courseId: 'course-1',
  enrollmentDate: new Date('2024-01-01'),
  expiryDate: new Date('2024-02-01'),
  progress: 100,
  completionDate: new Date('2024-01-20'),
  certificateUrl: 'https://example.com/certificate.pdf',
};

const mockIncompleteEnrollment: Enrollment = {
  id: 'enrollment-1',
  userId: 'user-1',
  courseId: 'course-1',
  enrollmentDate: new Date('2024-01-01'),
  expiryDate: new Date('2024-02-01'),
  progress: 75,
  completionDate: undefined,
  certificateUrl: undefined,
};

const mockLanguageContext = {
  language: 'en' as const,
  direction: 'ltr' as const,
  setLanguage: jest.fn(),
  t: (key: string) => {
    const translations: Record<string, string> = {
      'courses.certificate': 'Certificate',
      'courses.certificateOf': 'Certificate of',
      'courses.completion': 'Completion',
      'courses.presentedTo': 'This is to certify that',
      'courses.hasSuccessfullyCompleted': 'has successfully completed the course',
      'courses.completedOn': 'Completed on',
      'courses.duration': 'Duration',
      'courses.hours': 'hours',
      'courses.issuedBy': 'Issued by',
      'courses.autismCenter': 'Autism Center',
      'courses.certificateId': 'Certificate ID',
      'courses.downloadCertificate': 'Download Certificate',
      'courses.shareCertificate': 'Share Certificate',
      'courses.printCertificate': 'Print Certificate',
      'courses.certificateNotAvailable': 'Certificate Not Available',
      'courses.completeCourseForCertificate': 'Complete the course to receive your certificate.',
      'courses.continueCourse': 'Continue Course',
      'courses.generating': 'Generating...',
      'courses.certificateInstructions': 'You can download, print, or share your certificate.',
      'courses.downloadPDF': 'Download PDF',
      'courses.shareOnSocial': 'Share on Social Media',
      'common.back': 'Back',
    };
    return translations[key] || key;
  },
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageContext.Provider value={mockLanguageContext}>
        {component}
      </LanguageContext.Provider>
    </BrowserRouter>
  );
};

describe('Certificate', () => {
  it('renders certificate for completed course', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText('Certificate')).toBeInTheDocument();
    expect(screen.getByText('Certificate of Completion')).toBeInTheDocument();
    expect(screen.getByText('Introduction to Autism')).toBeInTheDocument();
    expect(screen.getByText('This is to certify that')).toBeInTheDocument();
    expect(screen.getByText('has successfully completed the course')).toBeInTheDocument();
  });

  it('shows completion date on certificate', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText(/Completed on/)).toBeInTheDocument();
  });

  it('displays course duration on certificate', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText('3 hours')).toBeInTheDocument();
  });

  it('shows certificate ID', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText('Certificate ID')).toBeInTheDocument();
    // Should show first 8 characters of enrollment ID in uppercase
    expect(screen.getByText('ENROLLME')).toBeInTheDocument();
  });

  it('displays action buttons for completed course', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText('Download Certificate')).toBeInTheDocument();
    expect(screen.getByText('Share Certificate')).toBeInTheDocument();
    expect(screen.getByText('Print Certificate')).toBeInTheDocument();
  });

  it('shows not available message for incomplete course', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockIncompleteEnrollment}
      />
    );

    expect(screen.getByText('Certificate Not Available')).toBeInTheDocument();
    expect(screen.getByText('Complete the course to receive your certificate.')).toBeInTheDocument();
    expect(screen.getByText('Continue Course')).toBeInTheDocument();
  });

  it('handles download button click', () => {
    // Mock createElement and appendChild
    const mockLink = {
      href: '',
      download: '',
      click: jest.fn(),
    };
    
    const createElementSpy = jest.spyOn(document, 'createElement').mockReturnValue(mockLink as any);
    const appendChildSpy = jest.spyOn(document.body, 'appendChild').mockImplementation();
    const removeChildSpy = jest.spyOn(document.body, 'removeChild').mockImplementation();

    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    const downloadButton = screen.getByText('Download Certificate');
    fireEvent.click(downloadButton);

    expect(createElementSpy).toHaveBeenCalledWith('a');
    expect(mockLink.click).toHaveBeenCalled();

    createElementSpy.mockRestore();
    appendChildSpy.mockRestore();
    removeChildSpy.mockRestore();
  });

  it('handles print button click', () => {
    const printSpy = jest.spyOn(window, 'print').mockImplementation();

    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    const printButton = screen.getByText('Print Certificate');
    fireEvent.click(printButton);

    expect(printSpy).toHaveBeenCalled();
    printSpy.mockRestore();
  });

  it('displays Arabic course title when language is Arabic', () => {
    const arabicContext = {
      ...mockLanguageContext,
      language: 'ar' as const,
      direction: 'rtl' as const,
    };

    render(
      <BrowserRouter>
        <LanguageContext.Provider value={arabicContext}>
          <Certificate
            course={mockCourse}
            enrollment={mockCompletedEnrollment}
          />
        </LanguageContext.Provider>
      </BrowserRouter>
    );

    expect(screen.getByText('مقدمة في التوحد')).toBeInTheDocument();
  });

  it('shows certificate instructions', () => {
    renderWithProviders(
      <Certificate
        course={mockCourse}
        enrollment={mockCompletedEnrollment}
      />
    );

    expect(screen.getByText('You can download, print, or share your certificate.')).toBeInTheDocument();
    expect(screen.getByText('Download PDF')).toBeInTheDocument();
    expect(screen.getByText('Share on Social Media')).toBeInTheDocument();
  });
});