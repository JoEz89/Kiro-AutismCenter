import { useState } from 'react';
import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { CartSidebar } from './CartSidebar';

interface CartIconProps {
  className?: string;
}

export const CartIcon = ({ className = '' }: CartIconProps) => {
  const { t } = useLocalization();
  const { totalItems } = useCart();
  const [isCartOpen, setIsCartOpen] = useState(false);

  const openCart = () => setIsCartOpen(true);
  const closeCart = () => setIsCartOpen(false);

  return (
    <>
      <button
        onClick={openCart}
        className={`relative p-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors ${className}`}
        aria-label={`${t('navigation.cart')} (${totalItems} ${t('cart.items', 'items')})`}
      >
        <svg
          className="w-6 h-6"
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
        
        {/* Cart Item Count Badge */}
        {totalItems > 0 && (
          <span className="absolute -top-1 -right-1 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full min-w-[1.25rem] h-5">
            {totalItems > 99 ? '99+' : totalItems}
          </span>
        )}
      </button>

      <CartSidebar isOpen={isCartOpen} onClose={closeCart} />
    </>
  );
};