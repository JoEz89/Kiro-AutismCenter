import React from 'react';
import { render } from '@testing-library/react';
import { HelmetProvider } from 'react-helmet-async';
import { 
  StructuredData, 
  generateOrganizationData, 
  generateWebsiteData, 
  generateBreadcrumbData,
  generateProductData,
  generateCourseData,
  generateServiceData
} from '../StructuredData';
import { testAccessibility } from '@/test/accessibility';

// Mock window.location
Object.defineProperty(window, 'location', {
  value: {
    origin: 'https://example.com'
  },
  writable: true
});

const renderWithHelmet = (component: React.ReactElement) => {
  return render(
    <HelmetProvider>
      {component}
    </HelmetProvider>
  );
};

describe('StructuredData', () => {
  beforeEach(() => {
    // Clear any existing head elements
    document.head.innerHTML = '';
  });

  it('renders without accessibility violations', async () => {
    const testData = { '@type': 'Organization', name: 'Test' };
    const result = renderWithHelmet(<StructuredData data={testData} />);
    await testAccessibility(result);
  });

  it('adds structured data script to head', () => {
    const testData = {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      name: 'Test Organization'
    };

    renderWithHelmet(<StructuredData data={testData} />);

    const script = document.querySelector('script[type="application/ld+json"]');
    expect(script).toBeTruthy();
    
    const scriptContent = JSON.parse(script?.textContent || '{}');
    expect(scriptContent).toEqual(testData);
  });

  it('handles complex nested data', () => {
    const complexData = {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      name: 'Test Organization',
      address: {
        '@type': 'PostalAddress',
        streetAddress: '123 Test St',
        addressLocality: 'Test City'
      },
      contactPoint: [
        {
          '@type': 'ContactPoint',
          telephone: '+1-555-123-4567',
          contactType: 'customer service'
        }
      ]
    };

    renderWithHelmet(<StructuredData data={complexData} />);

    const script = document.querySelector('script[type="application/ld+json"]');
    const scriptContent = JSON.parse(script?.textContent || '{}');
    expect(scriptContent).toEqual(complexData);
  });
});

describe('generateOrganizationData', () => {
  it('generates correct organization data for English', () => {
    const data = generateOrganizationData('en');
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('Organization');
    expect(data.name).toBe('Autism Center');
    expect(data.description).toContain('Comprehensive support center');
    expect(data.url).toBe('https://example.com');
    expect(data.contactPoint).toBeDefined();
    expect(data.address).toBeDefined();
    expect(data.sameAs).toBeInstanceOf(Array);
  });

  it('generates correct organization data for Arabic', () => {
    const data = generateOrganizationData('ar');
    
    expect(data.name).toBe('مركز التوحد');
    expect(data.description).toContain('مركز شامل');
  });

  it('includes all required organization properties', () => {
    const data = generateOrganizationData('en');
    
    expect(data).toHaveProperty('name');
    expect(data).toHaveProperty('description');
    expect(data).toHaveProperty('url');
    expect(data).toHaveProperty('logo');
    expect(data).toHaveProperty('contactPoint');
    expect(data).toHaveProperty('address');
    expect(data).toHaveProperty('sameAs');
    expect(data).toHaveProperty('foundingDate');
    expect(data).toHaveProperty('numberOfEmployees');
    expect(data).toHaveProperty('areaServed');
  });
});

describe('generateWebsiteData', () => {
  it('generates correct website data for English', () => {
    const data = generateWebsiteData('en');
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('WebSite');
    expect(data.name).toBe('Autism Center');
    expect(data.inLanguage).toBe('en-US');
    expect(data.potentialAction).toBeDefined();
    expect(data.potentialAction['@type']).toBe('SearchAction');
  });

  it('generates correct website data for Arabic', () => {
    const data = generateWebsiteData('ar');
    
    expect(data.name).toBe('مركز التوحد');
    expect(data.inLanguage).toBe('ar-BH');
    expect(data.description).toContain('منصة شاملة');
  });

  it('includes search action', () => {
    const data = generateWebsiteData('en');
    
    expect(data.potentialAction).toHaveProperty('target');
    expect(data.potentialAction).toHaveProperty('query-input');
    expect(data.potentialAction.target).toContain('/search?q=');
  });
});

describe('generateBreadcrumbData', () => {
  it('generates correct breadcrumb data', () => {
    const breadcrumbs = [
      { name: 'Home', url: 'https://example.com/' },
      { name: 'Products', url: 'https://example.com/products' },
      { name: 'Category', url: 'https://example.com/products/category' }
    ];

    const data = generateBreadcrumbData(breadcrumbs);
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('BreadcrumbList');
    expect(data.itemListElement).toHaveLength(3);
    
    data.itemListElement.forEach((item, index) => {
      expect(item['@type']).toBe('ListItem');
      expect(item.position).toBe(index + 1);
      expect(item.name).toBe(breadcrumbs[index].name);
      expect(item.item).toBe(breadcrumbs[index].url);
    });
  });

  it('handles empty breadcrumbs', () => {
    const data = generateBreadcrumbData([]);
    
    expect(data.itemListElement).toHaveLength(0);
  });
});

describe('generateProductData', () => {
  it('generates correct product data', () => {
    const product = {
      name: 'Test Product',
      description: 'A test product',
      price: 29.99,
      currency: 'USD',
      image: 'https://example.com/product.jpg',
      sku: 'TEST-001',
      availability: 'InStock' as const
    };

    const data = generateProductData(product);
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('Product');
    expect(data.name).toBe(product.name);
    expect(data.description).toBe(product.description);
    expect(data.image).toBe(product.image);
    expect(data.sku).toBe(product.sku);
    
    expect(data.offers).toBeDefined();
    expect(data.offers['@type']).toBe('Offer');
    expect(data.offers.price).toBe(product.price);
    expect(data.offers.priceCurrency).toBe(product.currency);
    expect(data.offers.availability).toBe('https://schema.org/InStock');
    expect(data.offers.seller.name).toBe('Autism Center');
  });

  it('handles out of stock products', () => {
    const product = {
      name: 'Out of Stock Product',
      description: 'Not available',
      price: 19.99,
      currency: 'BHD',
      image: 'https://example.com/product.jpg',
      sku: 'OOS-001',
      availability: 'OutOfStock' as const
    };

    const data = generateProductData(product);
    
    expect(data.offers.availability).toBe('https://schema.org/OutOfStock');
  });
});

describe('generateCourseData', () => {
  it('generates correct course data', () => {
    const course = {
      name: 'Autism Awareness Course',
      description: 'Learn about autism',
      provider: 'Autism Center',
      duration: 'P2W',
      language: 'en'
    };

    const data = generateCourseData(course);
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('Course');
    expect(data.name).toBe(course.name);
    expect(data.description).toBe(course.description);
    expect(data.timeRequired).toBe(course.duration);
    expect(data.inLanguage).toBe(course.language);
    expect(data.provider.name).toBe(course.provider);
    expect(data.educationalLevel).toBe('Beginner');
  });
});

describe('generateServiceData', () => {
  it('generates correct service data', () => {
    const service = {
      name: 'Autism Consultation',
      description: 'Professional consultation service',
      serviceType: 'Consultation',
      provider: 'Autism Center'
    };

    const data = generateServiceData(service);
    
    expect(data['@context']).toBe('https://schema.org');
    expect(data['@type']).toBe('Service');
    expect(data.name).toBe(service.name);
    expect(data.description).toBe(service.description);
    expect(data.serviceType).toBe(service.serviceType);
    expect(data.provider.name).toBe(service.provider);
    expect(data.areaServed.name).toBe('Bahrain');
  });
});