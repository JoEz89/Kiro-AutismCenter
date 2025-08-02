import { useParams, Link } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { SEOHead } from '@/components/seo';
import { Navigation } from '@/components/layout';
import { ProductDetail } from '@/components/products';
import { useProduct } from '@/hooks/useProducts';

const ProductDetailPage = () => {
  const { t, language } = useLocalization();
  const { id } = useParams<{ id: string }>();
  const { product } = useProduct(id || '');
  const { addToCart } = useCart();

  const handleAddToCart = async (productId: string, quantity: number) => {
    await addToCart(productId, quantity);
  };

  const productName = product ? (language === 'ar' ? product.nameAr : product.nameEn) : '';
  const productDescription = product ? (language === 'ar' ? product.descriptionAr : product.descriptionEn) : '';

  return (
    <>
      <SEOHead
        title={productName ? `${productName} - ${t('products.title')}` : t('products.title')}
        description={productDescription || t('products.description')}
        keywords="autism, products, resources, therapy, education"
        image={product?.imageUrls?.[0]}
      />
      
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Navigation />
        
        <main id="main-content" className="pt-16">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Breadcrumb */}
            <nav className="flex mb-8" aria-label="Breadcrumb">
              <ol className="inline-flex items-center space-x-1 md:space-x-3">
                <li className="inline-flex items-center">
                  <Link
                    to="/"
                    className="inline-flex items-center text-sm font-medium text-gray-700 hover:text-blue-600 dark:text-gray-400 dark:hover:text-white"
                  >
                    <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"></path>
                    </svg>
                    {t('navigation.home')}
                  </Link>
                </li>
                <li>
                  <div className="flex items-center">
                    <svg className="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd"></path>
                    </svg>
                    <Link
                      to="/products"
                      className="ml-1 text-sm font-medium text-gray-700 hover:text-blue-600 md:ml-2 dark:text-gray-400 dark:hover:text-white"
                    >
                      {t('navigation.products')}
                    </Link>
                  </div>
                </li>
                {productName && (
                  <li aria-current="page">
                    <div className="flex items-center">
                      <svg className="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd"></path>
                      </svg>
                      <span className="ml-1 text-sm font-medium text-gray-500 md:ml-2 dark:text-gray-400 truncate max-w-xs">
                        {productName}
                      </span>
                    </div>
                  </li>
                )}
              </ol>
            </nav>

            {/* Product Detail */}
            {id && <ProductDetail productId={id} onAddToCart={handleAddToCart} />}
          </div>
        </main>
      </div>
    </>
  );
};

export default ProductDetailPage;