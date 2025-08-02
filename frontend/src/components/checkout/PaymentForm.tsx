import { useState } from 'react';
import { useLocalization } from '@/hooks';

interface PaymentFormProps {
  onPaymentMethodChange: (paymentMethod: string) => void;
  selectedPaymentMethod: string;
  isProcessing?: boolean;
  errors?: Record<string, string>;
  className?: string;
}

export const PaymentForm = ({ 
  onPaymentMethodChange, 
  selectedPaymentMethod, 
  isProcessing = false,
  errors = {},
  className = '' 
}: PaymentFormProps) => {
  const { t } = useLocalization();
  const [cardDetails, setCardDetails] = useState({
    number: '',
    expiry: '',
    cvc: '',
    name: '',
  });

  const paymentMethods = [
    {
      id: 'card',
      name: t('checkout.creditCard', 'Credit/Debit Card'),
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
        </svg>
      ),
    },
    {
      id: 'paypal',
      name: t('checkout.paypal', 'PayPal'),
      icon: (
        <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
          <path d="M7.076 21.337H2.47a.641.641 0 0 1-.633-.74L4.944.901C5.026.382 5.474 0 5.998 0h7.46c2.57 0 4.578.543 5.69 1.81 1.01 1.15 1.304 2.42 1.012 4.287-.023.143-.047.288-.077.437-.983 5.05-4.349 6.797-8.647 6.797h-2.19c-.524 0-.968.382-1.05.9l-1.12 7.106zm14.146-14.42a3.35 3.35 0 0 0-.607-.421c-.315-.178-.7-.284-1.139-.284H12.85l-.98 6.218h2.19c2.048 0 3.81-.543 4.98-2.4.89-1.41 1.077-2.96.182-3.113z"/>
        </svg>
      ),
    },
  ];

  const handleCardDetailChange = (field: keyof typeof cardDetails, value: string) => {
    let formattedValue = value;

    // Format card number
    if (field === 'number') {
      formattedValue = value.replace(/\s/g, '').replace(/(.{4})/g, '$1 ').trim();
      if (formattedValue.length > 19) formattedValue = formattedValue.slice(0, 19);
    }

    // Format expiry date
    if (field === 'expiry') {
      formattedValue = value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '$1/$2');
      if (formattedValue.length > 5) formattedValue = formattedValue.slice(0, 5);
    }

    // Format CVC
    if (field === 'cvc') {
      formattedValue = value.replace(/\D/g, '');
      if (formattedValue.length > 4) formattedValue = formattedValue.slice(0, 4);
    }

    setCardDetails(prev => ({
      ...prev,
      [field]: formattedValue,
    }));
  };

  return (
    <div className={`space-y-6 ${className}`}>
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {t('checkout.paymentMethod', 'Payment Method')}
      </h3>

      {/* Payment Method Selection */}
      <div className="space-y-3">
        {paymentMethods.map((method) => (
          <label
            key={method.id}
            className={`flex items-center p-4 border rounded-lg cursor-pointer transition-colors ${
              selectedPaymentMethod === method.id
                ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                : 'border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-800'
            }`}
          >
            <input
              type="radio"
              name="paymentMethod"
              value={method.id}
              checked={selectedPaymentMethod === method.id}
              onChange={(e) => onPaymentMethodChange(e.target.value)}
              className="sr-only"
              disabled={isProcessing}
            />
            <div className="flex items-center">
              <div className={`w-4 h-4 border-2 rounded-full mr-3 flex items-center justify-center ${
                selectedPaymentMethod === method.id
                  ? 'border-blue-500'
                  : 'border-gray-300 dark:border-gray-600'
              }`}>
                {selectedPaymentMethod === method.id && (
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                )}
              </div>
              <div className="text-gray-600 dark:text-gray-300 mr-3">
                {method.icon}
              </div>
              <span className="text-gray-900 dark:text-white font-medium">
                {method.name}
              </span>
            </div>
          </label>
        ))}
      </div>

      {/* Card Details Form */}
      {selectedPaymentMethod === 'card' && (
        <div className="space-y-4 p-4 border border-gray-200 dark:border-gray-700 rounded-lg bg-gray-50 dark:bg-gray-800">
          <h4 className="text-md font-medium text-gray-900 dark:text-white">
            {t('checkout.cardDetails', 'Card Details')}
          </h4>

          {/* Card Number */}
          <div>
            <label htmlFor="card-number" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.cardNumber', 'Card Number')} *
            </label>
            <input
              type="text"
              id="card-number"
              value={cardDetails.number}
              onChange={(e) => handleCardDetailChange('number', e.target.value)}
              placeholder="1234 5678 9012 3456"
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.cardNumber ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              disabled={isProcessing}
              required
            />
            {errors.cardNumber && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.cardNumber}
              </p>
            )}
          </div>

          {/* Expiry and CVC */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="card-expiry" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('checkout.expiryDate', 'Expiry Date')} *
              </label>
              <input
                type="text"
                id="card-expiry"
                value={cardDetails.expiry}
                onChange={(e) => handleCardDetailChange('expiry', e.target.value)}
                placeholder="MM/YY"
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                  errors.cardExpiry ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
                }`}
                disabled={isProcessing}
                required
              />
              {errors.cardExpiry && (
                <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                  {errors.cardExpiry}
                </p>
              )}
            </div>

            <div>
              <label htmlFor="card-cvc" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('checkout.cvc', 'CVC')} *
              </label>
              <input
                type="text"
                id="card-cvc"
                value={cardDetails.cvc}
                onChange={(e) => handleCardDetailChange('cvc', e.target.value)}
                placeholder="123"
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                  errors.cardCvc ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
                }`}
                disabled={isProcessing}
                required
              />
              {errors.cardCvc && (
                <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                  {errors.cardCvc}
                </p>
              )}
            </div>
          </div>

          {/* Cardholder Name */}
          <div>
            <label htmlFor="card-name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t('checkout.cardholderName', 'Cardholder Name')} *
            </label>
            <input
              type="text"
              id="card-name"
              value={cardDetails.name}
              onChange={(e) => handleCardDetailChange('name', e.target.value)}
              placeholder={t('checkout.cardholderNamePlaceholder', 'Name as it appears on card')}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white ${
                errors.cardName ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              disabled={isProcessing}
              required
            />
            {errors.cardName && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                {errors.cardName}
              </p>
            )}
          </div>
        </div>
      )}

      {/* PayPal Notice */}
      {selectedPaymentMethod === 'paypal' && (
        <div className="p-4 border border-blue-200 dark:border-blue-800 rounded-lg bg-blue-50 dark:bg-blue-900/20">
          <div className="flex items-start">
            <svg className="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h4 className="text-sm font-medium text-blue-900 dark:text-blue-100">
                {t('checkout.paypalNotice', 'PayPal Payment')}
              </h4>
              <p className="text-sm text-blue-700 dark:text-blue-300 mt-1">
                {t('checkout.paypalDescription', 'You will be redirected to PayPal to complete your payment securely.')}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Security Notice */}
      <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
        </svg>
        {t('checkout.securePayment', 'Your payment information is encrypted and secure')}
      </div>
    </div>
  );
};