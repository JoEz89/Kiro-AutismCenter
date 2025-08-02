import { Fragment } from 'react';
import { Link } from 'react-router-dom';
import { Dialog, Transition } from '@headlessui/react';
import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { CartItem } from './CartItem';

interface CartSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CartSidebar = ({ isOpen, onClose }: CartSidebarProps) => {
  const { t, language } = useLocalization();
  const { items, totalAmount, totalItems, currency, isLoading, updateCartItem, removeFromCart } = useCart();

  const formatPrice = (amount: number, curr: string) => {
    return new Intl.NumberFormat(language === 'ar' ? 'ar-BH' : 'en-US', {
      style: 'currency',
      currency: curr,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const handleUpdateQuantity = async (productId: string, quantity: number) => {
    await updateCartItem(productId, quantity);
  };

  const handleRemoveItem = async (productId: string) => {
    await removeFromCart(productId);
  };

  return (
    <Transition.Root show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-50" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-in-out duration-500"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in-out duration-500"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-hidden">
          <div className="absolute inset-0 overflow-hidden">
            <div className={`pointer-events-none fixed inset-y-0 flex max-w-full ${language === 'ar' ? 'left-0' : 'right-0'}`}>
              <Transition.Child
                as={Fragment}
                enter="transform transition ease-in-out duration-500 sm:duration-700"
                enterFrom={language === 'ar' ? '-translate-x-full' : 'translate-x-full'}
                enterTo="translate-x-0"
                leave="transform transition ease-in-out duration-500 sm:duration-700"
                leaveFrom="translate-x-0"
                leaveTo={language === 'ar' ? '-translate-x-full' : 'translate-x-full'}
              >
                <Dialog.Panel className="pointer-events-auto w-screen max-w-md">
                  <div className="flex h-full flex-col overflow-y-scroll bg-white dark:bg-gray-900 shadow-xl">
                    {/* Header */}
                    <div className="flex-1 overflow-y-auto px-4 py-6 sm:px-6">
                      <div className="flex items-start justify-between">
                        <Dialog.Title className="text-lg font-medium text-gray-900 dark:text-white">
                          {t('cart.title')}
                          {totalItems > 0 && (
                            <span className="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                              {totalItems}
                            </span>
                          )}
                        </Dialog.Title>
                        <div className="ml-3 flex h-7 items-center">
                          <button
                            type="button"
                            className="relative -m-2 p-2 text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
                            onClick={onClose}
                          >
                            <span className="absolute -inset-0.5" />
                            <span className="sr-only">{t('common.close')}</span>
                            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </div>
                      </div>

                      {/* Cart Items */}
                      <div className="mt-8">
                        {isLoading && items.length === 0 ? (
                          <div className="space-y-4">
                            {Array.from({ length: 3 }).map((_, index) => (
                              <div key={index} className="animate-pulse">
                                <div className="flex items-center gap-4 p-4 bg-gray-100 dark:bg-gray-800 rounded-lg">
                                  <div className="w-16 h-16 bg-gray-200 dark:bg-gray-700 rounded"></div>
                                  <div className="flex-1">
                                    <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded mb-2"></div>
                                    <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-20"></div>
                                  </div>
                                  <div className="w-20 h-8 bg-gray-200 dark:bg-gray-700 rounded"></div>
                                </div>
                              </div>
                            ))}
                          </div>
                        ) : items.length === 0 ? (
                          <div className="text-center py-12">
                            <svg
                              className="mx-auto h-16 w-16 text-gray-400 mb-4"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                              aria-hidden="true"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"
                              />
                            </svg>
                            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                              {t('cart.empty')}
                            </h3>
                            <p className="text-gray-600 dark:text-gray-300 mb-6">
                              {t('cart.emptyDescription', 'Add some products to get started.')}
                            </p>
                            <Link
                              to="/products"
                              onClick={onClose}
                              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 transition-colors"
                            >
                              {t('cart.continueShopping')}
                            </Link>
                          </div>
                        ) : (
                          <div className="space-y-4">
                            {items.map((item) => (
                              <CartItem
                                key={item.productId}
                                productId={item.productId}
                                product={item.product}
                                quantity={item.quantity}
                                price={item.price}
                                onUpdateQuantity={handleUpdateQuantity}
                                onRemove={handleRemoveItem}
                                isUpdating={isLoading}
                              />
                            ))}
                          </div>
                        )}
                      </div>
                    </div>

                    {/* Footer */}
                    {items.length > 0 && (
                      <div className="border-t border-gray-200 dark:border-gray-700 px-4 py-6 sm:px-6">
                        {/* Subtotal */}
                        <div className="flex justify-between text-base font-medium text-gray-900 dark:text-white mb-4">
                          <p>{t('cart.subtotal')}</p>
                          <p>{formatPrice(totalAmount, currency)}</p>
                        </div>
                        
                        <p className="mt-0.5 text-sm text-gray-500 dark:text-gray-400 mb-6">
                          {t('cart.shippingCalculated', 'Shipping and taxes calculated at checkout.')}
                        </p>
                        
                        {/* Action Buttons */}
                        <div className="space-y-3">
                          <Link
                            to="/checkout"
                            onClick={onClose}
                            className="flex w-full items-center justify-center rounded-md border border-transparent bg-blue-600 px-6 py-3 text-base font-medium text-white shadow-sm hover:bg-blue-700 transition-colors"
                          >
                            {t('cart.checkout')}
                          </Link>
                          
                          <div className="text-center">
                            <button
                              type="button"
                              className="font-medium text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300 transition-colors"
                              onClick={onClose}
                            >
                              {t('cart.continueShopping')}
                              <span aria-hidden="true"> &rarr;</span>
                            </button>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </Dialog.Panel>
              </Transition.Child>
            </div>
          </div>
        </div>
      </Dialog>
    </Transition.Root>
  );
};