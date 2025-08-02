import React, { useState, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  ArrowDownTrayIcon,
  ShareIcon,
  PrinterIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
} from '@heroicons/react/24/outline';
import type { Course, Enrollment } from '../../types';
import { useLocalization } from '../../hooks/useLocalization';
import { formatDate } from '../../utils';
import { LoadingSpinner } from '../ui/LoadingSpinner';

interface CertificateProps {
  course: Course;
  enrollment: Enrollment;
  className?: string;
}

export const Certificate: React.FC<CertificateProps> = ({
  course,
  enrollment,
  className = '',
}) => {
  const { t, language } = useLocalization();
  const certificateRef = useRef<HTMLDivElement>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [shareUrl, setShareUrl] = useState<string | null>(null);

  const title = language === 'ar' ? course.titleAr : course.titleEn;
  const isCompleted = enrollment.completionDate !== undefined;

  const handleDownload = async () => {
    if (!enrollment.certificateUrl) {
      await generateCertificate();
    }
    
    // Download the certificate PDF
    if (enrollment.certificateUrl) {
      const link = document.createElement('a');
      link.href = enrollment.certificateUrl;
      link.download = `certificate-${course.id}-${enrollment.id}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  const handleShare = async () => {
    if (navigator.share && enrollment.certificateUrl) {
      try {
        await navigator.share({
          title: t('courses.certificateOf') + ' ' + t('courses.completion'),
          text: t('courses.certificateShareText', { course: title }),
          url: enrollment.certificateUrl,
        });
      } catch (error) {
        // Fallback to copying URL
        copyToClipboard();
      }
    } else {
      copyToClipboard();
    }
  };

  const copyToClipboard = () => {
    if (enrollment.certificateUrl) {
      navigator.clipboard.writeText(enrollment.certificateUrl);
      // Show success message (you might want to add a toast notification here)
    }
  };

  const generateCertificate = async () => {
    setIsGenerating(true);
    try {
      // This would call the certificate generation API
      // const response = await courseService.generateCertificate(enrollment.id);
      // Update enrollment with certificate URL
      setTimeout(() => {
        setIsGenerating(false);
      }, 2000);
    } catch (error) {
      setIsGenerating(false);
      console.error('Failed to generate certificate:', error);
    }
  };

  if (!isCompleted) {
    return (
      <div className="max-w-4xl mx-auto text-center py-12">
        <div className="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-8">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('courses.certificateNotAvailable')}
          </h2>
          <p className="text-gray-600 dark:text-gray-300 mb-6">
            {t('courses.completeCourseForCertificate')}
          </p>
          <Link
            to={`/courses/${course.id}/learn`}
            className="btn btn-primary"
          >
            {t('courses.continueCourse')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={`max-w-6xl mx-auto ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-4">
          <Link
            to={`/courses/${course.id}/progress`}
            className="inline-flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200 transition-colors"
          >
            <ArrowLeftIcon className="w-4 h-4" />
            {t('common.back')}
          </Link>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t('courses.certificate')}
          </h1>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={handleShare}
            className="btn btn-outline"
            disabled={!enrollment.certificateUrl}
          >
            <ShareIcon className="w-5 h-5 mr-2" />
            {t('courses.shareCertificate')}
          </button>
          <button
            onClick={handlePrint}
            className="btn btn-outline"
          >
            <PrinterIcon className="w-5 h-5 mr-2" />
            {t('courses.printCertificate')}
          </button>
          <button
            onClick={handleDownload}
            className="btn btn-primary"
            disabled={isGenerating}
          >
            {isGenerating ? (
              <>
                <LoadingSpinner size="sm" className="mr-2" />
                {t('courses.generating')}
              </>
            ) : (
              <>
                <ArrowDownTrayIcon className="w-5 h-5 mr-2" />
                {t('courses.downloadCertificate')}
              </>
            )}
          </button>
        </div>
      </div>

      {/* Certificate */}
      <div className="bg-white shadow-2xl rounded-lg overflow-hidden print:shadow-none print:rounded-none">
        <div
          ref={certificateRef}
          className="certificate-content p-12 bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-800 dark:to-gray-900 print:bg-white"
        >
          {/* Certificate Header */}
          <div className="text-center mb-12">
            <div className="flex items-center justify-center mb-6">
              <div className="w-16 h-16 bg-blue-600 dark:bg-blue-500 rounded-full flex items-center justify-center">
                <CheckCircleIcon className="w-10 h-10 text-white" />
              </div>
            </div>
            
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
              {t('courses.certificateOf')} {t('courses.completion')}
            </h1>
            
            <div className="w-24 h-1 bg-blue-600 dark:bg-blue-500 mx-auto"></div>
          </div>

          {/* Certificate Body */}
          <div className="text-center mb-12">
            <p className="text-lg text-gray-700 dark:text-gray-300 mb-8">
              {t('courses.presentedTo')}
            </p>
            
            <div className="mb-8">
              <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
                {enrollment.userId} {/* This should be the actual user name */}
              </h2>
              <div className="w-48 h-0.5 bg-gray-400 dark:bg-gray-600 mx-auto"></div>
            </div>
            
            <p className="text-lg text-gray-700 dark:text-gray-300 mb-4">
              {t('courses.hasSuccessfullyCompleted')}
            </p>
            
            <h3 className="text-2xl font-semibold text-blue-600 dark:text-blue-400 mb-8">
              "{title}"
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-2xl mx-auto">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
                  {t('courses.completedOn')}
                </p>
                <p className="text-lg font-medium text-gray-900 dark:text-white">
                  {enrollment.completionDate && formatDate(enrollment.completionDate)}
                </p>
              </div>
              
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
                  {t('courses.duration')}
                </p>
                <p className="text-lg font-medium text-gray-900 dark:text-white">
                  {Math.floor(course.duration / 60)} {t('courses.hours')}
                </p>
              </div>
            </div>
          </div>

          {/* Certificate Footer */}
          <div className="border-t border-gray-200 dark:border-gray-700 pt-8">
            <div className="flex items-center justify-between">
              <div className="text-center">
                <div className="w-32 h-0.5 bg-gray-400 dark:bg-gray-600 mb-2"></div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {t('courses.issuedBy')}
                </p>
                <p className="font-medium text-gray-900 dark:text-white">
                  {t('courses.autismCenter')}
                </p>
              </div>
              
              <div className="text-center">
                <div className="w-20 h-20 bg-gray-200 dark:bg-gray-700 rounded-full flex items-center justify-center mb-2">
                  <span className="text-xs text-gray-500 dark:text-gray-400">SEAL</span>
                </div>
              </div>
              
              <div className="text-center">
                <div className="w-32 h-0.5 bg-gray-400 dark:bg-gray-600 mb-2"></div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {t('courses.certificateId')}
                </p>
                <p className="font-mono text-sm text-gray-900 dark:text-white">
                  {enrollment.id.slice(0, 8).toUpperCase()}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Certificate Actions (Print Hidden) */}
      <div className="mt-8 text-center print:hidden">
        <p className="text-gray-600 dark:text-gray-300 mb-4">
          {t('courses.certificateInstructions')}
        </p>
        
        <div className="flex items-center justify-center gap-4">
          <button
            onClick={handleDownload}
            className="btn btn-primary"
            disabled={isGenerating}
          >
            {isGenerating ? (
              <>
                <LoadingSpinner size="sm" className="mr-2" />
                {t('courses.generating')}
              </>
            ) : (
              <>
                <ArrowDownTrayIcon className="w-5 h-5 mr-2" />
                {t('courses.downloadPDF')}
              </>
            )}
          </button>
          
          <button
            onClick={handleShare}
            className="btn btn-outline"
          >
            <ShareIcon className="w-5 h-5 mr-2" />
            {t('courses.shareOnSocial')}
          </button>
        </div>
      </div>
    </div>
  );
};

export default Certificate;