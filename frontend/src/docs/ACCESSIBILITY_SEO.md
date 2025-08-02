# SEO and Accessibility Implementation

This document outlines the comprehensive SEO and accessibility features implemented for the Autism Center website.

## Accessibility Features (WCAG 2.1 Compliance)

### 1. Skip Links
- **Component**: `SkipLink`
- **Purpose**: Allows keyboard users to skip to main content
- **Features**:
  - Visually hidden by default, visible on focus
  - Smooth scrolling to target elements
  - RTL language support
  - Keyboard navigation (Enter/Space)

### 2. Screen Reader Support
- **Component**: `ScreenReaderOnly`
- **Purpose**: Content visible only to screen readers
- **Features**:
  - Visually hidden but accessible to assistive technology
  - Flexible element types (span, div, etc.)
  - Proper clipping for cross-browser compatibility

### 3. Live Regions
- **Component**: `LiveRegion`
- **Purpose**: Announces dynamic content changes to screen readers
- **Features**:
  - Configurable politeness levels (polite, assertive, off)
  - Automatic message clearing
  - Proper ARIA attributes

### 4. Focus Management
- **Component**: `FocusTrap`
- **Purpose**: Traps focus within modal dialogs and overlays
- **Features**:
  - Circular tab navigation
  - Focus restoration on deactivation
  - Handles disabled elements
  - Supports contenteditable elements

### 5. Keyboard Navigation
- **Component**: `KeyboardNavigation`
- **Hook**: `useKeyboardNavigation`, `useAccessibility`
- **Purpose**: Enhanced keyboard interaction support
- **Features**:
  - Arrow key navigation
  - Escape key handling
  - Enter key activation
  - Home/End key support
  - Focus trapping capabilities

### 6. Accessibility Testing
- **Utility**: `testAccessibility`
- **Purpose**: Automated accessibility testing with axe-core
- **Features**:
  - WCAG 2.1 rule validation
  - Custom rule configuration
  - Integration with Jest/Vitest
  - Screen reader compatibility testing

## SEO Features

### 1. Meta Tags and HTML Structure
- **Component**: `SEOHead`
- **Features**:
  - Dynamic title and description
  - Open Graph meta tags
  - Twitter Card support
  - Canonical URLs
  - Language and direction attributes
  - Theme color and viewport settings

### 2. Structured Data (Schema.org)
- **Component**: `StructuredData`
- **Generators**: Multiple structured data generators
- **Features**:
  - Organization schema
  - Website schema with search action
  - Product schema for e-commerce
  - Course schema for educational content
  - Service schema for appointments
  - Breadcrumb navigation schema

### 3. Internationalization SEO
- **Features**:
  - Alternate language links (hreflang)
  - Language-specific structured data
  - RTL/LTR direction support
  - Locale-specific formatting

### 4. Performance Optimization
- **Features**:
  - DNS prefetch for external resources
  - Preconnect to font providers
  - Optimized favicon and icon links
  - Proper resource hints

## Implementation Examples

### Basic SEO Setup
```tsx
import { SEOHead } from '@/components/seo';

function MyPage() {
  return (
    <>
      <SEOHead
        title="Page Title"
        description="Page description"
        keywords="keyword1, keyword2"
        type="website"
      />
      <main>
        {/* Page content */}
      </main>
    </>
  );
}
```

### Accessibility Implementation
```tsx
import { SkipLink, ScreenReaderOnly, LiveRegion } from '@/components/accessibility';

function AccessiblePage() {
  const [announcement, setAnnouncement] = useState('');

  return (
    <>
      <SkipLink href="#main-content">Skip to main content</SkipLink>
      
      <main id="main-content">
        <ScreenReaderOnly>
          <h1>Page Title for Screen Readers</h1>
        </ScreenReaderOnly>
        
        {/* Page content */}
        
        <LiveRegion message={announcement} />
      </main>
    </>
  );
}
```

### Keyboard Navigation
```tsx
import { KeyboardNavigation, useAccessibility } from '@/components/accessibility';

function InteractiveComponent() {
  const { announce } = useAccessibility();

  const handleEscape = () => {
    announce('Dialog closed');
    // Close dialog logic
  };

  return (
    <KeyboardNavigation onEscape={handleEscape} trapFocus>
      <button>Interactive Element</button>
    </KeyboardNavigation>
  );
}
```

### Structured Data
```tsx
import { StructuredData, generateProductData } from '@/components/seo';

function ProductPage({ product }) {
  const productSchema = generateProductData({
    name: product.name,
    description: product.description,
    price: product.price,
    currency: 'BHD',
    image: product.imageUrl,
    sku: product.sku,
    availability: product.inStock ? 'InStock' : 'OutOfStock'
  });

  return (
    <>
      <StructuredData data={productSchema} />
      {/* Product content */}
    </>
  );
}
```

## Testing

### Accessibility Testing
```tsx
import { testAccessibility } from '@/test/accessibility';

test('component is accessible', async () => {
  const result = render(<MyComponent />);
  await testAccessibility(result);
});
```

### Keyboard Navigation Testing
```tsx
import { testKeyboardNavigation } from '@/test/accessibility';

test('supports keyboard navigation', () => {
  render(<InteractiveComponent />);
  const element = screen.getByRole('button');
  testKeyboardNavigation(element);
});
```

## Browser Support

### Accessibility
- Screen readers: NVDA, JAWS, VoiceOver, TalkBack
- Keyboard navigation: All modern browsers
- Focus management: IE11+, all modern browsers

### SEO
- Structured data: All search engines supporting Schema.org
- Meta tags: All browsers and social platforms
- Language support: All modern browsers

## Performance Impact

- **Bundle size**: ~15KB gzipped for all accessibility components
- **Runtime overhead**: Minimal, most features are passive
- **SEO impact**: Positive impact on search rankings and social sharing

## Compliance

- **WCAG 2.1 AA**: Full compliance for implemented features
- **Section 508**: Compatible
- **Schema.org**: Latest vocabulary support
- **Open Graph**: v1.0.1 specification
- **Twitter Cards**: Summary and large image support

## Future Enhancements

1. **Color contrast validation**: Automated contrast checking
2. **Voice navigation**: Support for voice commands
3. **Advanced structured data**: Event, FAQ, and Review schemas
4. **Performance monitoring**: Core Web Vitals tracking
5. **A11y automation**: Automated accessibility testing in CI/CD