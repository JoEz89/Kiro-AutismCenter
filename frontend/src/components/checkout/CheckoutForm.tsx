import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { useCart } from '@/context/CartContext';
import { useAuth } from '@/hooks';
import { checkoutService, type ShippingOption } from '@/services/checkoutService';
import { AddressForm } from './AddressForm';
import { PaymentForm } from './PaymentForm';
import { OrderSummary } from './OrderSummary';
import type { Address } from '@/types';

interface CheckoutFormProps {
  className?: string;
}

export const CheckoutForm = ({ className = '' }: CheckoutFormProps) => {
  const { t } = useLocalization();
  const navigate = useNavigate();
  const { items, totalAmount, currency, clearCart } = useCart();
  const { isAuthenticated } = useAuth();

  const [currentStep, setCurrentStep] = useState(1);
  const [isProcessing, setIsProcessing] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Form data
  const [shippingAddress, setShippingAddress] = useState<Address>({
    street: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'BH',
  });

  const [billingAddress, setBillingAddress] = useState<Address>({
    street: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'BH',
  });

  const [useSameAddress, setUseSameAddress] = useState(true);
  const [paymentMethod, setPaymentMethod] = useState('card');
  const [shippingOptions, setShippingOptions] = useState<ShippingOption[]>([]);
  const [selectedShipping, setSelectedShipping] = useState<ShippingOption | null>(null);
  const [taxAmount, setTaxAmount] = useState(0);
  const [orderNotes, setOrderNotes] = useState('');

  const steps = [
    { id: 1, name: t('checkout.shipping', 'Shipping'), completed: currentStep > 1 },
    { id: 2, name: t('checkout.payment', 'Payment'), completed: currentStep > 2 },
    { id: 3, name: t('checkout.review', 'Review'), completed: false },
  ];

  // Redirect if cart is empty
  useEffect(() => {
    if (items.length === 0) {
      navigate('/cart');
    }
  }, [items.length, navigate]);

  // Update billing address when using same address
  useEffect(() => {
    if (useSameAddress) {
      setBillingAddress(shippingAddress);
    }
  }, [shippingAddress, useSameAddress]);

  // Fetch shipping options when shipping address changes
  useEffect(() => {
    const fetchShippingOptions = async () => {
      if (shippingAddress.street && shippingAddress.city && shippingAddress.country) {
        try {
          const options = await checkoutService.getShippingOptions(shippingAddress);
          setShippingOptions(options);
          if (options.length > 0 && !selectedShipping) {
            setSelectedShipping(options[0]);
          }
        } catch (error) {
          console.error('Failed to fetch shipping options:', error);
        }
      }
    };

    fetchShippingOptions();
  }, [shippingAddress, selectedShipping]);

  // Calculate tax when addresses change
  useEffect(() => {
    const calculateTax = async () => {
      if (billingAddress.street && billingAddress.city && billingAddress.country) {
        try {
          const tax = await checkoutService.calculateTax(billingAddress, totalAmount);
          setTaxAmount(tax.taxAmount);
        } catch (error) {
          console.error('Failed to calculate tax:', error);
          setTaxAmount(0);
        }
      }
    };

    calculateTax();
  }, [billingAddress, totalAmount]);

  const validateStep = (step: number): boolean => {
    const newErrors: Record<string, string> = {};

    if (step === 1) {
      // Validate shipping address
      if (!shippingAddress.street) newErrors.street = t('validation.required');
      if (!shippingAddress.city) newErrors.city = t('validation.required');
      if (!shippingAddress.state) newErrors.state = t('validation.required');
      if (!shippingAddress.postalCode) newErrors.postalCode = t('validation.required');
      if (!shippingAddress.country) newErrors.country = t('validation.required');

      // Validate billing address if different
      if (!useSameAddress) {
        if (!billingAddress.street) newErrors.billingStreet = t('validation.required');
        if (!billingAddress.city) newErrors.billingCity = t('validation.required');
        if (!billingAddress.state) newErrors.billingState = t('validation.required');
        if (!billingAddress.postalCode) newErrors.billingPostalCode = t('validation.required');
        if (!billingAddress.country) newErrors.billingCountry = t('validation.required');
      }
    }

    if (step === 2) {
      // Validate payment method
      if (!paymentMethod) newErrors.paymentMethod = t('validation.required');
      
      if (paymentMethod === 'card') {
        // Card validation would be handled by Stripe in a real implementation
        // For now, we'll just check if fields are present
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNextStep = () => {
    if (validateStep(currentStep)) {
      setCurrentStep(prev => Math.min(prev + 1, 3));
    }
  };

  const handlePrevStep = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  const handleSubmitOrder = async () => {
    if (!validateStep(currentStep)) return;

    setIsProcessing(true);
    try {
      // Create payment intent
      const finalTotal = totalAmount + (selectedShipping?.price || 0) + taxAmount;
      const paymentIntent = await checkoutService.createPaymentIntent(finalTotal, currency);

      // Create order
      const order = await checkoutService.createOrder({
        shippingAddress,
        billingAddress: useSameAddress ? shippingAddress : billingAddress,
        paymentMethodId: paymentIntent.id,
        notes: orderNotes,
      });

      // Clear cart
      await clearCart();

      // Redirect to success page
      navigate(`/order-confirmation/${order.id}`);
    } catch (error) {
      console.error('Order submission failed:', error);
      setErrors({ submit: error instanceof Error ? error.message : t('checkout.orderFailed', 'Order submission failed') });
    } finally {
      setIsProcessing(false);
    }
  };

  if (items.length === 0) {
    return null;
  }

  return (
    <div className={className}>
      {/* Progress Steps */}
      <div className="mb-8">
        <nav aria-label={t('checkout.progress', 'Checkout progress')}>
          <ol className="flex items-center justify-center space-x-8 rtl:space-x-reverse">
            {steps.map((step, index) => (
              <li key={step.id} className="flex items-center">
                <div className={`flex items-center justify-center w-8 h-8 rounded-full border-2 ${
                  step.completed || currentStep === step.id
                    ? 'border-blue-600 bg-blue-600 text-white'
                    : 'border-gray-300 dark:border-gray-600 text-gray-500 dark:text-gray-400'
                }`}>
                  {step.completed ? (
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                  ) : (
                    <span className="text-sm font-medium">{step.id}</span>
                  )}
                </div>
                <span className={`ml-2 text-sm font-medium ${
                  step.completed || currentStep === step.id
                    ? 'text-blue-600 dark:text-blue-400'
                    : 'text-gray-500 dark:text-gray-400'
                }`}>
                  {step.name}
                </span>
                {index < steps.length - 1 && (
                  <div className="ml-8 w-8 h-0.5 bg-gray-300 dark:bg-gray-600"></div>
                )}
              </li>
            ))}
          </ol>
        </nav>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Form */}
        <div className="lg:col-span-2 space-y-8">
          {/* Step 1: Shipping Information */}
          {currentStep === 1 && (
            <div className="space-y-8">
              <AddressForm
                address={shippingAddress}
                onAddressChange={setShippingAddress}
                title={t('checkout.shippingAddress', 'Shipping Address')}
                errors={errors}
              />

              {/* Same as billing address checkbox */}
              <div className="flex items-center">
                <input
                  id="same-address"
                  type="checkbox"
                  checked={useSameAddress}
                  onChange={(e) => setUseSameAddress(e.target.checked)}
                  className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                />
                <label htmlFor="same-address" className="ml-2 text-sm text-gray-700 dark:text-gray-300">
                  {t('checkout.sameAsBilling', 'Use same address for billing')}
                </label>
              </div>

              {/* Billing Address */}
              {!useSameAddress && (
                <AddressForm
                  address={billingAddress}
                  onAddressChange={setBillingAddress}
                  title={t('checkout.billingAddress', 'Billing Address')}
                  errors={errors}
                />
              )}

              {/* Shipping Options */}
              {shippingOptions.length > 0 && (
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                    {t('checkout.shippingMethod', 'Shipping Method')}
                  </h3>
                  <div className="space-y-3">
                    {shippingOptions.map((option) => (
                      <label
                        key={option.id}
                        className={`flex items-center justify-between p-4 border rounded-lg cursor-pointer ${
                          selectedShipping?.id === option.id
                            ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                            : 'border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-800'
                        }`}
                      >
                        <div className="flex items-center">
                          <input
                            type="radio"
                            name="shipping"
                            value={option.id}
                            checked={selectedShipping?.id === option.id}
                            onChange={() => setSelectedShipping(option)}
                            className="sr-only"
                          />
                          <div className={`w-4 h-4 border-2 rounded-full mr-3 flex items-center justify-center ${
                            selectedShipping?.id === option.id
                              ? 'border-blue-500'
                              : 'border-gray-300 dark:border-gray-600'
                          }`}>
                            {selectedShipping?.id === option.id && (
                              <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                            )}
                          </div>
                          <div>
                            <p className="font-medium text-gray-900 dark:text-white">
                              {option.name}
                            </p>
                            <p className="text-sm text-gray-600 dark:text-gray-300">
                              {option.description}
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <p className="font-medium text-gray-900 dark:text-white">
                            {option.price === 0 ? t('checkout.free', 'Free') : `$${option.price.toFixed(2)}`}
                          </p>
                          <p className="text-sm text-gray-600 dark:text-gray-300">
                            {option.estimatedDays} {t('checkout.days', 'days')}
                          </p>
                        </div>
                      </label>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Step 2: Payment Information */}
          {currentStep === 2 && (
            <PaymentForm
              onPaymentMethodChange={setPaymentMethod}
              selectedPaymentMethod={paymentMethod}
              isProcessing={isProcessing}
              errors={errors}
            />
          )}

          {/* Step 3: Review Order */}
          {currentStep === 3 && (
            <div className="space-y-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                {t('checkout.reviewOrder', 'Review Your Order')}
              </h3>

              {/* Order Notes */}
              <div>
                <label htmlFor="order-notes" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t('checkout.orderNotes', 'Order Notes')} ({t('checkout.optional', 'Optional')})
                </label>
                <textarea
                  id="order-notes"
                  rows={3}
                  value={orderNotes}
                  onChange={(e) => setOrderNotes(e.target.value)}
                  placeholder={t('checkout.orderNotesPlaceholder', 'Any special instructions for your order...')}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                />
              </div>

              {/* Terms and Conditions */}
              <div className="flex items-start">
                <input
                  id="terms"
                  type="checkbox"
                  required
                  className="mt-1 rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                />
                <label htmlFor="terms" className="ml-2 text-sm text-gray-700 dark:text-gray-300">
                  {t('checkout.agreeToTerms', 'I agree to the')}{' '}
                  <a href="/terms" className="text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300">
                    {t('checkout.termsOfService', 'Terms of Service')}
                  </a>{' '}
                  {t('common.and', 'and')}{' '}
                  <a href="/privacy" className="text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300">
                    {t('checkout.privacyPolicy', 'Privacy Policy')}
                  </a>
                </label>
              </div>

              {errors.submit && (
                <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md">
                  <p className="text-sm text-red-600 dark:text-red-400">
                    {errors.submit}
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Navigation Buttons */}
          <div className="flex justify-between pt-6 border-t border-gray-200 dark:border-gray-700">
            <button
              onClick={handlePrevStep}
              disabled={currentStep === 1}
              className="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {t('common.back')}
            </button>

            {currentStep < 3 ? (
              <button
                onClick={handleNextStep}
                className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
              >
                {t('common.next')}
              </button>
            ) : (
              <button
                onClick={handleSubmitOrder}
                disabled={isProcessing}
                className="px-8 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isProcessing ? (
                  <div className="flex items-center">
                    <svg className="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    {t('checkout.processing', 'Processing...')}
                  </div>
                ) : (
                  t('checkout.placeOrder', 'Place Order')
                )}
              </button>
            )}
          </div>
        </div>

        {/* Order Summary Sidebar */}
        <div className="lg:col-span-1">
          <div className="sticky top-8">
            <OrderSummary
              shippingOption={selectedShipping || undefined}
              taxAmount={taxAmount}
            />
          </div>
        </div>
      </div>
    </div>
  );
};