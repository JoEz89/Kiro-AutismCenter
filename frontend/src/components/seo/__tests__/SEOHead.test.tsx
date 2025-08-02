import React from 'react';
import { render } from '@testing-library/react';
import { HelmetProvider } from 'react-helmet-async';
import { SEOHead } from '../SEOHead';
import { testAccessibility } from '@/test/accessibility';

// Mock the localization hook
vi.mock('@/hooks', () => ({
  useLocalization: () => ({
    t: (key: string, fallback?: string) => fallback || key,
    language: 'en',
    isRTL: false
  })
}));

// Mock window.location
Object.defineProperty(window, 'location', {
  value: {
    href: 'https://example.com',
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

describe('SEOHead', () => {
  beforeEach(() => {
    // Clear any existing head elements
    document.head.innerHTML = '';
  });

  it('renders without accessibility violations', async () => {
    const result = renderWithHelmet(<SEOHead />);
    await testAccessibility(result);
  });

  it('sets basic meta tags correctly', () => {
    renderWithHelmet(<SEOHead title="Test Page" description="Test description" />);

    expect(document.title).toBe('Test Page | home.welcome');
    
    const descriptionMeta = document.querySelector('meta[name="description"]');
    expect(descriptionMeta).toHaveAttribute('content', 'Test description');
    
    const viewportMeta = document.querySelector('meta[name="viewport"]');
    expect(viewportMeta).toHaveAttribute('content', 'width=device-width, initial-scale=1.0');
  });

  it('sets language and direction attributes correctly', () => {
    renderWithHelmet(<SEOHead />);
    
    const htmlElement = document.documentElement;
    expect(htmlElement).toHaveAttribute('lang', 'en');
    expect(htmlElement).toHaveAttribute('dir', 'ltr');
  });

  it('includes structured data for organization', () => {
    renderWithHelmet(<SEOHead />);
    
    const structuredDataScripts = document.querySelectorAll('script[type="application/ld+json"]');
    expect(structuredDataScripts.length).toBeGreaterThan(0);
    
    const organizationScript = Array.from(structuredDataScripts).find(script => {
      const data = JSON.parse(script.textContent || '{}');
      return data['@type'] === 'Organization';
    });
    
    expect(organizationScript).toBeTruthy();
  });

  it('includes Open Graph meta tags', () => {
    renderWithHelmet(<SEOHead title="Test Page" description="Test description" />);
    
    const ogTitle = document.querySelector('meta[property="og:title"]');
    expect(ogTitle).toHaveAttribute('content', 'Test Page | home.welcome');
    
    const ogDescription = document.querySelector('meta[property="og:description"]');
    expect(ogDescription).toHaveAttribute('content', 'Test description');
    
    const ogType = document.querySelector('meta[property="og:type"]');
    expect(ogType).toHaveAttribute('content', 'website');
  });

  it('includes Twitter Card meta tags', () => {
    renderWithHelmet(<SEOHead title="Test Page" description="Test description" />);
    
    const twitterCard = document.querySelector('meta[name="twitter:card"]');
    expect(twitterCard).toHaveAttribute('content', 'summary_large_image');
    
    const twitterTitle = document.querySelector('meta[name="twitter:title"]');
    expect(twitterTitle).toHaveAttribute('content', 'Test Page | home.welcome');
  });

  it('sets canonical URL correctly', () => {
    renderWithHelmet(<SEOHead url="https://example.com/test" />);
    
    const canonical = document.querySelector('link[rel="canonical"]');
    expect(canonical).toHaveAttribute('href', 'https://example.com/test');
  });

  it('includes alternate language links', () => {
    renderWithHelmet(<SEOHead />);
    
    const alternateEn = document.querySelector('link[rel="alternate"][hrefLang="en"]');
    expect(alternateEn).toBeTruthy();
    
    const alternateAr = document.querySelector('link[rel="alternate"][hrefLang="ar"]');
    expect(alternateAr).toBeTruthy();
    
    const alternateDefault = document.querySelector('link[rel="alternate"][hrefLang="x-default"]');
    expect(alternateDefault).toBeTruthy();
  });

  it('includes favicon and icon links', () => {
    renderWithHelmet(<SEOHead />);
    
    const favicon = document.querySelector('link[rel="icon"][type="image/x-icon"]');
    expect(favicon).toHaveAttribute('href', '/favicon.ico');
    
    const appleTouchIcon = document.querySelector('link[rel="apple-touch-icon"]');
    expect(appleTouchIcon).toHaveAttribute('href', '/apple-touch-icon.png');
    
    const manifest = document.querySelector('link[rel="manifest"]');
    expect(manifest).toHaveAttribute('href', '/site.webmanifest');
  });

  it('sets robots meta tag correctly', () => {
    renderWithHelmet(<SEOHead noIndex={false} />);
    
    const robots = document.querySelector('meta[name="robots"]');
    expect(robots).toHaveAttribute('content', 'index, follow');
  });

  it('sets noindex when specified', () => {
    renderWithHelmet(<SEOHead noIndex={true} />);
    
    const robots = document.querySelector('meta[name="robots"]');
    expect(robots).toHaveAttribute('content', 'noindex, nofollow');
  });

  it('includes preconnect and dns-prefetch links', () => {
    renderWithHelmet(<SEOHead />);
    
    const preconnectGoogle = document.querySelector('link[rel="preconnect"][href="https://fonts.googleapis.com"]');
    expect(preconnectGoogle).toBeTruthy();
    
    const preconnectGstatic = document.querySelector('link[rel="preconnect"][href="https://fonts.gstatic.com"]');
    expect(preconnectGstatic).toBeTruthy();
    
    const dnsPrefetchGoogle = document.querySelector('link[rel="dns-prefetch"][href="//fonts.googleapis.com"]');
    expect(dnsPrefetchGoogle).toBeTruthy();
  });

  it('includes website structured data', () => {
    renderWithHelmet(<SEOHead />);
    
    const structuredDataScripts = document.querySelectorAll('script[type="application/ld+json"]');
    const websiteScript = Array.from(structuredDataScripts).find(script => {
      const data = JSON.parse(script.textContent || '{}');
      return data['@type'] === 'WebSite';
    });
    
    expect(websiteScript).toBeTruthy();
    
    const websiteData = JSON.parse(websiteScript?.textContent || '{}');
    expect(websiteData.potentialAction).toBeDefined();
    expect(websiteData.potentialAction['@type']).toBe('SearchAction');
  });

  it('sets theme color correctly', () => {
    renderWithHelmet(<SEOHead />);
    
    const themeColor = document.querySelector('meta[name="theme-color"]');
    expect(themeColor).toHaveAttribute('content', '#2563eb');
  });

  it('handles custom keywords', () => {
    const customKeywords = 'custom, keywords, test';
    renderWithHelmet(<SEOHead keywords={customKeywords} />);
    
    const keywordsMeta = document.querySelector('meta[name="keywords"]');
    expect(keywordsMeta).toHaveAttribute('content', customKeywords);
  });

  it('handles custom image', () => {
    const customImage = '/custom-image.jpg';
    renderWithHelmet(<SEOHead image={customImage} />);
    
    const ogImage = document.querySelector('meta[property="og:image"]');
    expect(ogImage).toHaveAttribute('content', customImage);
    
    const twitterImage = document.querySelector('meta[name="twitter:image"]');
    expect(twitterImage).toHaveAttribute('content', customImage);
  });

  it('handles different content types', () => {
    renderWithHelmet(<SEOHead type="article" />);
    
    const ogType = document.querySelector('meta[property="og:type"]');
    expect(ogType).toHaveAttribute('content', 'article');
  });
});