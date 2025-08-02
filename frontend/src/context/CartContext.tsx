import React, { createContext, useContext, useReducer, useEffect } from 'react';
import { cartService, type CartResponse } from '@/services/cartService';
import { useAuth } from '@/hooks';
import type { Product } from '@/types';

interface CartState {
  items: (CartItem & { product: Product })[];
  totalAmount: number;
  totalItems: number;
  currency: string;
  isLoading: boolean;
  error: string | null;
}

interface CartItem {
  productId: string;
  quantity: number;
  price: number;
}

type CartAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'SET_CART'; payload: CartResponse }
  | { type: 'CLEAR_CART' }
  | { type: 'ADD_ITEM'; payload: { productId: string; quantity: number } }
  | { type: 'UPDATE_ITEM'; payload: { productId: string; quantity: number } }
  | { type: 'REMOVE_ITEM'; payload: string };

interface CartContextType extends CartState {
  addToCart: (productId: string, quantity?: number) => Promise<void>;
  updateCartItem: (productId: string, quantity: number) => Promise<void>;
  removeFromCart: (productId: string) => Promise<void>;
  clearCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
}

const initialState: CartState = {
  items: [],
  totalAmount: 0,
  totalItems: 0,
  currency: 'USD',
  isLoading: false,
  error: null,
};

const CartContext = createContext<CartContextType | undefined>(undefined);

function cartReducer(state: CartState, action: CartAction): CartState {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    
    case 'SET_ERROR':
      return { ...state, error: action.payload, isLoading: false };
    
    case 'SET_CART':
      return {
        ...state,
        items: action.payload.items,
        totalAmount: action.payload.totalAmount,
        totalItems: action.payload.totalItems,
        currency: action.payload.currency,
        isLoading: false,
        error: null,
      };
    
    case 'CLEAR_CART':
      return {
        ...state,
        items: [],
        totalAmount: 0,
        totalItems: 0,
        isLoading: false,
        error: null,
      };
    
    default:
      return state;
  }
}

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(cartReducer, initialState);
  const { isAuthenticated, user } = useAuth();

  // Load cart on mount and when authentication changes
  useEffect(() => {
    loadCart();
  }, [isAuthenticated, user]);

  const loadCart = async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      
      if (isAuthenticated) {
        // Load from server and sync with local cart
        const cart = await cartService.syncCart();
        dispatch({ type: 'SET_CART', payload: cart });
        cartService.saveCartToLocal(cart);
      } else {
        // Load from local storage
        const localCart = cartService.getCartFromLocal();
        if (localCart) {
          dispatch({ type: 'SET_CART', payload: localCart });
        } else {
          dispatch({ type: 'CLEAR_CART' });
        }
      }
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: error instanceof Error ? error.message : 'Failed to load cart' });
    }
  };

  const addToCart = async (productId: string, quantity: number = 1) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      if (isAuthenticated) {
        const cart = await cartService.addToCart({ productId, quantity });
        dispatch({ type: 'SET_CART', payload: cart });
        cartService.saveCartToLocal(cart);
      } else {
        // Handle local cart for non-authenticated users
        const localCart = cartService.getCartFromLocal() || {
          items: [],
          totalAmount: 0,
          totalItems: 0,
          currency: 'USD',
        };

        // Find existing item
        const existingItemIndex = localCart.items.findIndex(item => item.productId === productId);
        
        if (existingItemIndex >= 0) {
          // Update existing item
          localCart.items[existingItemIndex].quantity += quantity;
        } else {
          // Add new item (we'll need to fetch product details)
          // For now, we'll add a placeholder - this should be improved to fetch product details
          localCart.items.push({
            productId,
            quantity,
            price: 0, // This should be fetched from the product
            product: {} as Product, // This should be fetched from the product service
          });
        }

        // Recalculate totals
        localCart.totalItems = localCart.items.reduce((sum, item) => sum + item.quantity, 0);
        localCart.totalAmount = localCart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);

        dispatch({ type: 'SET_CART', payload: localCart });
        cartService.saveCartToLocal(localCart);
      }
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: error instanceof Error ? error.message : 'Failed to add item to cart' });
    }
  };

  const updateCartItem = async (productId: string, quantity: number) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      if (quantity <= 0) {
        await removeFromCart(productId);
        return;
      }

      if (isAuthenticated) {
        const cart = await cartService.updateCartItem({ productId, quantity });
        dispatch({ type: 'SET_CART', payload: cart });
        cartService.saveCartToLocal(cart);
      } else {
        // Handle local cart
        const localCart = cartService.getCartFromLocal();
        if (localCart) {
          const itemIndex = localCart.items.findIndex(item => item.productId === productId);
          if (itemIndex >= 0) {
            localCart.items[itemIndex].quantity = quantity;
            
            // Recalculate totals
            localCart.totalItems = localCart.items.reduce((sum, item) => sum + item.quantity, 0);
            localCart.totalAmount = localCart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);

            dispatch({ type: 'SET_CART', payload: localCart });
            cartService.saveCartToLocal(localCart);
          }
        }
      }
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: error instanceof Error ? error.message : 'Failed to update cart item' });
    }
  };

  const removeFromCart = async (productId: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      if (isAuthenticated) {
        const cart = await cartService.removeFromCart(productId);
        dispatch({ type: 'SET_CART', payload: cart });
        cartService.saveCartToLocal(cart);
      } else {
        // Handle local cart
        const localCart = cartService.getCartFromLocal();
        if (localCart) {
          localCart.items = localCart.items.filter(item => item.productId !== productId);
          
          // Recalculate totals
          localCart.totalItems = localCart.items.reduce((sum, item) => sum + item.quantity, 0);
          localCart.totalAmount = localCart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);

          dispatch({ type: 'SET_CART', payload: localCart });
          cartService.saveCartToLocal(localCart);
        }
      }
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: error instanceof Error ? error.message : 'Failed to remove item from cart' });
    }
  };

  const clearCart = async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      if (isAuthenticated) {
        await cartService.clearCart();
      }
      
      cartService.clearLocalCart();
      dispatch({ type: 'CLEAR_CART' });
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: error instanceof Error ? error.message : 'Failed to clear cart' });
    }
  };

  const refreshCart = async () => {
    await loadCart();
  };

  const value: CartContextType = {
    ...state,
    addToCart,
    updateCartItem,
    removeFromCart,
    clearCart,
    refreshCart,
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = (): CartContextType => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};