import React from 'react';
import { Helmet } from 'react-helmet-async';

interface StructuredDataProps {
  data: Record<string, any>;
}

export const StructuredData: React.FC<StructuredDataProps> = ({ data }) => {
  return (
    <Helmet>
      <script type="application/ld+json">
        {JSON.stringify(data)}
      </script>
    </Helmet>
  );
};

// Predefined structured data generators
export const generateOrganizationData = (language: 'en' | 'ar') => ({
  '@context': 'https://schema.org',
  '@type': 'Organization',
  name: language === 'ar' ? 'مركز التوحد' : 'Autism Center',
  description: language === 'ar' 
    ? 'مركز شامل لدعم الأطفال المصابين بالتوحد وأسرهم في البحرين'
    : 'Comprehensive support center for children with autism and their families in Bahrain',
  url: window.location.origin,
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
    addressLocality: 'Manama',
    addressRegion: 'Capital Governorate'
  },
  sameAs: [
    'https://facebook.com/autismcenter',
    'https://twitter.com/autismcenter',
    'https://instagram.com/autismcenter'
  ],
  foundingDate: '2020',
  numberOfEmployees: '10-50',
  areaServed: {
    '@type': 'Country',
    name: 'Bahrain'
  }
});

export const generateWebsiteData = (language: 'en' | 'ar') => ({
  '@context': 'https://schema.org',
  '@type': 'WebSite',
  name: language === 'ar' ? 'مركز التوحد' : 'Autism Center',
  description: language === 'ar' 
    ? 'منصة شاملة لخدمات التوحد تشمل الدورات التعليمية والمنتجات وحجز المواعيد'
    : 'Comprehensive autism services platform including educational courses, products, and appointment booking',
  url: window.location.origin,
  potentialAction: {
    '@type': 'SearchAction',
    target: `${window.location.origin}/search?q={search_term_string}`,
    'query-input': 'required name=search_term_string'
  },
  inLanguage: language === 'ar' ? 'ar-BH' : 'en-US',
  publisher: {
    '@type': 'Organization',
    name: language === 'ar' ? 'مركز التوحد' : 'Autism Center'
  }
});

export const generateBreadcrumbData = (breadcrumbs: Array<{ name: string; url: string }>) => ({
  '@context': 'https://schema.org',
  '@type': 'BreadcrumbList',
  itemListElement: breadcrumbs.map((crumb, index) => ({
    '@type': 'ListItem',
    position: index + 1,
    name: crumb.name,
    item: crumb.url
  }))
});

export const generateProductData = (product: {
  name: string;
  description: string;
  price: number;
  currency: string;
  image: string;
  sku: string;
  availability: 'InStock' | 'OutOfStock';
}) => ({
  '@context': 'https://schema.org',
  '@type': 'Product',
  name: product.name,
  description: product.description,
  image: product.image,
  sku: product.sku,
  offers: {
    '@type': 'Offer',
    price: product.price,
    priceCurrency: product.currency,
    availability: `https://schema.org/${product.availability}`,
    seller: {
      '@type': 'Organization',
      name: 'Autism Center'
    }
  }
});

export const generateCourseData = (course: {
  name: string;
  description: string;
  provider: string;
  duration: string;
  language: string;
}) => ({
  '@context': 'https://schema.org',
  '@type': 'Course',
  name: course.name,
  description: course.description,
  provider: {
    '@type': 'Organization',
    name: course.provider
  },
  timeRequired: course.duration,
  inLanguage: course.language,
  educationalLevel: 'Beginner',
  teaches: 'Autism awareness and support techniques'
});

export const generateServiceData = (service: {
  name: string;
  description: string;
  serviceType: string;
  provider: string;
}) => ({
  '@context': 'https://schema.org',
  '@type': 'Service',
  name: service.name,
  description: service.description,
  serviceType: service.serviceType,
  provider: {
    '@type': 'Organization',
    name: service.provider
  },
  areaServed: {
    '@type': 'Country',
    name: 'Bahrain'
  }
});

export default StructuredData;