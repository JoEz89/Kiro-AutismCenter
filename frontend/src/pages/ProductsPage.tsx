import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { SEOHead } from '@/components/seo';
import { Navigation } from '@/components/layout';
import { ProductCatalog } from '@/components/products';

const ProductsPage = () => {
  const { t } = useLocalization();
  const { addToCart } = useCart();

  const handleAddToCart = async (productId: string) => {
    await addToCart(productId, 1);
  };

  return (
    <>
      <SEOHead
        title={t('products.title')}
        description={t('products.description')}
        keywords="autism, products, resources, therapy, education"
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Navigation />
        
        <main id="main-content" className="pt-16">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Page Header */}
            <div className="mb-8">
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
                {t('products.title')}
              </h1>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                {t('products.description')}
              </p>
            </div>

            {/* Product Catalog */}
            <ProductCatalog onAddToCart={handleAddToCart} />
          </div>
        </main>
      </div>
    </>
  );
};

export default ProductsPage;