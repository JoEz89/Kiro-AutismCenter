import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { useAuth } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { SEOHead } from '@/components/seo';
import { Navigation } from '@/components/layout';
import { CheckoutForm } from '@/components/checkout';

const CheckoutPage = () => {
  const { t } = useLocalization();
  const { isAuthenticated } = useAuth();
  const { items } = useCart();
  const navigate = useNavigate();

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login?redirect=/checkout');
    }
  }, [isAuthenticated, navigate]);

  // Redirect to cart if empty
  useEffect(() => {
    if (items.length === 0) {
      navigate('/cart');
    }
  }, [items.length, navigate]);

  if (!isAuthenticated || items.length === 0) {
    return null;
  }

  return (
    <>
      <SEOHead
        title={t('checkout.title', 'Checkout')}
        description={t('checkout.description', 'Complete your purchase securely')}
        keywords="checkout, payment, secure, autism, products"
        noIndex={true} // Don't index checkout pages
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Navigation />
        
        <main id="main-content" className="pt-16">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Page Header */}
            <div className="mb-8">
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
                {t('checkout.title', 'Checkout')}
              </h1>
              <p className="mt-2 text-gray-600 dark:text-gray-300">
                {t('checkout.subtitle', 'Complete your order securely')}
              </p>
            </div>

            {/* Checkout Form */}
            <CheckoutForm />
          </div>
        </main>
      </div>
    </>
  );
};

export default CheckoutPage;