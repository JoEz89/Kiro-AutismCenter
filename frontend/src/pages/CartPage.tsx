import { useLocalization } from '@/hooks';
import { SEOHead } from '@/components/seo';
import { Navigation } from '@/components/layout';
import { CartPage as CartPageComponent } from '@/components/cart';

const CartPage = () => {
  const { t } = useLocalization();

  return (
    <>
      <SEOHead
        title={t('cart.title')}
        description={t('cart.description', 'Review and manage items in your shopping cart')}
        keywords="cart, shopping, checkout, autism, products"
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Navigation />
        
        <main id="main-content" className="pt-16">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Page Header */}
            <div className="mb-8">
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
                {t('cart.title')}
              </h1>
            </div>

            {/* Cart Content */}
            <CartPageComponent />
          </div>
        </main>
      </div>
    </>
  );
};

export default CartPage;