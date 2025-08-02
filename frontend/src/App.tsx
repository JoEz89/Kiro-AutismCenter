import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { HelmetProvider } from 'react-helmet-async';
import { AuthProvider, LanguageProvider, ThemeProvider, CartProvider } from '@/context';
import { SkipLink } from '@/components';
import { useLocalization } from '@/hooks';
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';
import ProductDetailPage from './pages/ProductDetailPage';
import CartPage from './pages/CartPage';
import CheckoutPage from './pages/CheckoutPage';
import OrderConfirmationPage from './pages/OrderConfirmationPage';

function AppContent() {
  const { t } = useLocalization();

  return (
    <Router>
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
        {/* Skip Links for Accessibility */}
        <SkipLink href="#main-content">
          {t('accessibility.skipToMain', 'Skip to main content')}
        </SkipLink>
        <SkipLink href="#navigation">
          {t('accessibility.skipToNavigation', 'Skip to navigation')}
        </SkipLink>
        
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/order-confirmation/:orderId" element={<OrderConfirmationPage />} />
        </Routes>
      </div>
    </Router>
  );
}

function App() {
  return (
    <HelmetProvider>
      <ThemeProvider>
        <LanguageProvider>
          <AuthProvider>
            <CartProvider>
              <AppContent />
            </CartProvider>
          </AuthProvider>
        </LanguageProvider>
      </ThemeProvider>
    </HelmetProvider>
  );
}

export default App;
