import React from 'react';
import { Helmet } from 'react-helmet-async';
import { useLocalization } from '@/hooks';

interface SEOHeadProps {
  title?: string;
  description?: string;
  keywords?: string;
  image?: string;
  url?: string;
  type?: 'website' | 'article' | 'product';
  noIndex?: boolean;
}

export const SEOHead: React.FC<SEOHeadProps> = ({
  title,
  description,
  keywords,
  image,
  url,
  type = 'website',
  noIndex = false
}) => {
  const { t, language } = useLocalization();

  // Default values
  const defaultTitle = t('home.welcome');
  const defaultDescription = t('home.mission');
  const defaultKeywords = language === 'ar' 
    ? 'التوحد، مركز التوحد، البحرين، دعم الأطفال، تعليم خاص، علاج النطق، العلاج الوظيفي'
    : 'autism, autism center, Bahrain, children support, special education, speech therapy, occupational therapy';
  
  const siteTitle = defaultTitle;
  const pageTitle = title ? `${title} | ${siteTitle}` : siteTitle;
  const pageDescription = description || defaultDescription;
  const pageKeywords = keywords || defaultKeywords;
  const pageImage = image || '/images/og-image.jpg';
  const pageUrl = url || window.location.href;

  // Structured data for the organization
  const organizationStructuredData = {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: language === 'ar' ? 'مركز التوحد' : 'Autism Center',
    description: pageDescription,
    url: pageUrl,
    logo: `${window.location.origin}/images/logo.png`,
    contactPoint: {
      '@type': 'ContactPoint',
      telephone: '+973-XXXX-XXXX',
      contactType: 'customer service',
      availableLanguage: ['English', 'Arabic']
    },
    address: {
      '@type': 'PostalAddress',
      addressCountry: 'BH',
      addressLocality: 'Manama'
    },
    sameAs: [
      'https://facebook.com/autismcenter',
      'https://twitter.com/autismcenter',
      'https://instagram.com/autismcenter'
    ]
  };

  // Website structured data
  const websiteStructuredData = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: siteTitle,
    description: pageDescription,
    url: pageUrl,
    potentialAction: {
      '@type': 'SearchAction',
      target: `${window.location.origin}/search?q={search_term_string}`,
      'query-input': 'required name=search_term_string'
    },
    inLanguage: language === 'ar' ? 'ar-BH' : 'en-US'
  };

  return (
    <Helmet>
      {/* Basic Meta Tags */}
      <html lang={language === 'ar' ? 'ar' : 'en'} dir={language === 'ar' ? 'rtl' : 'ltr'} />
      <title>{pageTitle}</title>
      <meta name="description" content={pageDescription} />
      <meta name="keywords" content={pageKeywords} />
      <meta name="author" content="Autism Center" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <meta name="theme-color" content="#2563eb" />
      
      {/* Robots */}
      {noIndex && <meta name="robots" content="noindex, nofollow" />}
      {!noIndex && <meta name="robots" content="index, follow" />}
      
      {/* Open Graph / Facebook */}
      <meta property="og:type" content={type} />
      <meta property="og:title" content={pageTitle} />
      <meta property="og:description" content={pageDescription} />
      <meta property="og:image" content={pageImage} />
      <meta property="og:url" content={pageUrl} />
      <meta property="og:site_name" content={siteTitle} />
      <meta property="og:locale" content={language === 'ar' ? 'ar_BH' : 'en_US'} />
      
      {/* Twitter */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={pageTitle} />
      <meta name="twitter:description" content={pageDescription} />
      <meta name="twitter:image" content={pageImage} />
      <meta name="twitter:site" content="@autismcenter" />
      
      {/* Canonical URL */}
      <link rel="canonical" href={pageUrl} />
      
      {/* Alternate Language Links */}
      <link rel="alternate" hrefLang="en" href={pageUrl.replace(/\/ar\//, '/en/')} />
      <link rel="alternate" hrefLang="ar" href={pageUrl.replace(/\/en\//, '/ar/')} />
      <link rel="alternate" hrefLang="x-default" href={pageUrl} />
      
      {/* Favicon and Icons */}
      <link rel="icon" type="image/x-icon" href="/favicon.ico" />
      <link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
      <link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
      <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
      <link rel="manifest" href="/site.webmanifest" />
      
      {/* Structured Data */}
      <script type="application/ld+json">
        {JSON.stringify(organizationStructuredData)}
      </script>
      <script type="application/ld+json">
        {JSON.stringify(websiteStructuredData)}
      </script>
      
      {/* Preconnect to external domains */}
      <link rel="preconnect" href="https://fonts.googleapis.com" />
      <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
      
      {/* DNS Prefetch */}
      <link rel="dns-prefetch" href="//fonts.googleapis.com" />
      <link rel="dns-prefetch" href="//fonts.gstatic.com" />
    </Helmet>
  );
};

export default SEOHead;