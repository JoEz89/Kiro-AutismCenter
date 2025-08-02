import { render, screen, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import { AddressForm } from '../AddressForm';
import { LanguageProvider } from '@/context';
import type { Address } from '@/types';

const mockAddress: Address = {
  street: '',
  city: '',
  state: '',
  postalCode: '',
  country: '',
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <LanguageProvider>
      {component}
    </LanguageProvider>
  );
};

describe('AddressForm', () => {
  it('renders address form fields', () => {
    const mockOnAddressChange = vi.fn();
    
    renderWithProviders(
      <AddressForm
        address={mockAddress}
        onAddressChange={mockOnAddressChange}
        title="Shipping Address"
      />
    );
    
    expect(screen.getByText('Shipping Address')).toBeInTheDocument();
    expect(screen.getByLabelText(/street address/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/city/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/state/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/postal code/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/country/i)).toBeInTheDocument();
  });

  it('calls onAddressChange when fields are updated', () => {
    const mockOnAddressChange = vi.fn();
    
    renderWithProviders(
      <AddressForm
        address={mockAddress}
        onAddressChange={mockOnAddressChange}
        title="Shipping Address"
      />
    );
    
    const streetInput = screen.getByLabelText(/street address/i);
    fireEvent.change(streetInput, { target: { value: '123 Main St' } });
    
    expect(mockOnAddressChange).toHaveBeenCalledWith({
      ...mockAddress,
      street: '123 Main St',
    });
  });

  it('displays validation errors', () => {
    const mockOnAddressChange = vi.fn();
    const errors = { street: 'Street address is required' };
    
    renderWithProviders(
      <AddressForm
        address={mockAddress}
        onAddressChange={mockOnAddressChange}
        title="Shipping Address"
        errors={errors}
      />
    );
    
    expect(screen.getByText('Street address is required')).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    const mockOnAddressChange = vi.fn();
    
    renderWithProviders(
      <AddressForm
        address={mockAddress}
        onAddressChange={mockOnAddressChange}
        title="Shipping Address"
      />
    );
    
    const streetInput = screen.getByLabelText(/street address/i);
    expect(streetInput).toHaveAttribute('required');
  });
});