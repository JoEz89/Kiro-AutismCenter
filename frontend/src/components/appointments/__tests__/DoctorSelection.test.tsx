import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import DoctorSelection from '../DoctorSelection';
import { useLocalization } from '../../../hooks/useLocalization';
import { useDoctors } from '../../../hooks/useAppointments';
import type { Doctor } from '../../../types';

// Mock hooks
vi.mock('../../../hooks/useLocalization');
vi.mock('../../../hooks/useAppointments');

const mockUseLocalization = vi.mocked(useLocalization);
const mockUseDoctors = vi.mocked(useDoctors);

const mockDoctors: Doctor[] = [
  {
    id: 'doctor-1',
    nameEn: 'Dr. John Smith',
    nameAr: 'د. جون سميث',
    specialtyEn: 'Autism Specialist',
    specialtyAr: 'أخصائي التوحد',
    availability: []
  },
  {
    id: 'doctor-2',
    nameEn: 'Dr. Sarah Johnson',
    nameAr: 'د. سارة جونسون',
    specialtyEn: 'Child Psychologist',
    specialtyAr: 'أخصائي نفسي للأطفال',
    availability: []
  }
];

describe('DoctorSelection', () => {
  const mockOnDoctorSelect = vi.fn();

  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string) => key,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn()
    });

    mockUseDoctors.mockReturnValue({
      doctors: mockDoctors,
      loading: false,
      error: null,
      refetch: vi.fn()
    });

    vi.clearAllMocks();
  });

  it('renders list of doctors', () => {
    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    expect(screen.getByText('appointments.selectDoctor')).toBeInTheDocument();
    expect(screen.getByText('Dr. John Smith')).toBeInTheDocument();
    expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    expect(screen.getByText('Autism Specialist')).toBeInTheDocument();
    expect(screen.getByText('Child Psychologist')).toBeInTheDocument();
  });

  it('displays Arabic names when language is Arabic', () => {
    mockUseLocalization.mockReturnValue({
      t: (key: string) => key,
      language: 'ar',
      direction: 'rtl',
      setLanguage: vi.fn()
    });

    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    expect(screen.getByText('د. جون سميث')).toBeInTheDocument();
    expect(screen.getByText('د. سارة جونسون')).toBeInTheDocument();
    expect(screen.getByText('أخصائي التوحد')).toBeInTheDocument();
    expect(screen.getByText('أخصائي نفسي للأطفال')).toBeInTheDocument();
  });

  it('calls onDoctorSelect when a doctor is clicked', () => {
    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    const doctorCard = screen.getByText('Dr. John Smith').closest('div');
    if (doctorCard) {
      fireEvent.click(doctorCard);
      expect(mockOnDoctorSelect).toHaveBeenCalledWith(mockDoctors[0]);
    }
  });

  it('highlights selected doctor', () => {
    render(
      <DoctorSelection
        selectedDoctorId="doctor-1"
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    const selectedDoctorCard = screen.getByText('Dr. John Smith').closest('div');
    expect(selectedDoctorCard).toHaveClass('border-blue-500');
  });

  it('displays loading state', () => {
    mockUseDoctors.mockReturnValue({
      doctors: [],
      loading: true,
      error: null,
      refetch: vi.fn()
    });

    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('displays error state', () => {
    const errorMessage = 'Failed to load doctors';
    mockUseDoctors.mockReturnValue({
      doctors: [],
      loading: false,
      error: errorMessage,
      refetch: vi.fn()
    });

    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
  });

  it('displays no doctors message when list is empty', () => {
    mockUseDoctors.mockReturnValue({
      doctors: [],
      loading: false,
      error: null,
      refetch: vi.fn()
    });

    render(
      <DoctorSelection
        selectedDoctorId={undefined}
        onDoctorSelect={mockOnDoctorSelect}
      />
    );

    expect(screen.getByText('appointments.noDoctorsAvailable')).toBeInTheDocument();
  });
});