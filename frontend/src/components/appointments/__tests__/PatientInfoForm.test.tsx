import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import PatientInfoForm from '../PatientInfoForm';
import { useLocalization } from '../../../hooks/useLocalization';
import type { PatientInfo } from '../../../types';

// Mock hooks
vi.mock('../../../hooks/useLocalization');

const mockUseLocalization = vi.mocked(useLocalization);

describe('PatientInfoForm', () => {
  const mockPatientInfo: PatientInfo = {
    firstName: '',
    lastName: '',
    age: 0,
    phone: '',
    notes: ''
  };

  const mockOnPatientInfoChange = vi.fn();

  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string) => key,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn()
    });

    vi.clearAllMocks();
  });

  it('renders all form fields', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    expect(screen.getByText('appointments.patientInfo')).toBeInTheDocument();
    expect(screen.getByLabelText(/auth.firstName/)).toBeInTheDocument();
    expect(screen.getByLabelText(/auth.lastName/)).toBeInTheDocument();
    expect(screen.getByLabelText(/appointments.age/)).toBeInTheDocument();
    expect(screen.getByLabelText(/appointments.phone/)).toBeInTheDocument();
    expect(screen.getByLabelText(/appointments.additionalNotes/)).toBeInTheDocument();
  });

  it('calls onPatientInfoChange when first name changes', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    const firstNameInput = screen.getByLabelText(/auth.firstName/);
    fireEvent.change(firstNameInput, { target: { value: 'John' } });

    expect(mockOnPatientInfoChange).toHaveBeenCalledWith({
      ...mockPatientInfo,
      firstName: 'John'
    });
  });

  it('calls onPatientInfoChange when last name changes', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    const lastNameInput = screen.getByLabelText(/auth.lastName/);
    fireEvent.change(lastNameInput, { target: { value: 'Doe' } });

    expect(mockOnPatientInfoChange).toHaveBeenCalledWith({
      ...mockPatientInfo,
      lastName: 'Doe'
    });
  });

  it('calls onPatientInfoChange when age changes', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    const ageInput = screen.getByLabelText(/appointments.age/);
    fireEvent.change(ageInput, { target: { value: '25' } });

    expect(mockOnPatientInfoChange).toHaveBeenCalledWith({
      ...mockPatientInfo,
      age: 25
    });
  });

  it('calls onPatientInfoChange when phone changes', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    const phoneInput = screen.getByLabelText(/appointments.phone/);
    fireEvent.change(phoneInput, { target: { value: '+973 1234 5678' } });

    expect(mockOnPatientInfoChange).toHaveBeenCalledWith({
      ...mockPatientInfo,
      phone: '+973 1234 5678'
    });
  });

  it('calls onPatientInfoChange when notes change', () => {
    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    const notesTextarea = screen.getByLabelText(/appointments.additionalNotes/);
    fireEvent.change(notesTextarea, { target: { value: 'Some notes' } });

    expect(mockOnPatientInfoChange).toHaveBeenCalledWith({
      ...mockPatientInfo,
      notes: 'Some notes'
    });
  });

  it('displays validation errors', () => {
    const errors = {
      firstName: 'First name is required',
      age: 'Invalid age'
    };

    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
        errors={errors}
      />
    );

    expect(screen.getByText('First name is required')).toBeInTheDocument();
    expect(screen.getByText('Invalid age')).toBeInTheDocument();
  });

  it('applies error styling to fields with errors', () => {
    const errors = {
      firstName: 'First name is required'
    };

    render(
      <PatientInfoForm
        patientInfo={mockPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
        errors={errors}
      />
    );

    const firstNameInput = screen.getByLabelText(/auth.firstName/);
    expect(firstNameInput).toHaveClass('border-red-300');
  });

  it('displays filled form values', () => {
    const filledPatientInfo: PatientInfo = {
      firstName: 'John',
      lastName: 'Doe',
      age: 25,
      phone: '+973 1234 5678',
      notes: 'Some notes'
    };

    render(
      <PatientInfoForm
        patientInfo={filledPatientInfo}
        onPatientInfoChange={mockOnPatientInfoChange}
      />
    );

    expect(screen.getByDisplayValue('John')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Doe')).toBeInTheDocument();
    expect(screen.getByDisplayValue('25')).toBeInTheDocument();
    expect(screen.getByDisplayValue('+973 1234 5678')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Some notes')).toBeInTheDocument();
  });
});