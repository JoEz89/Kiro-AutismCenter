import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import AppointmentCalendar from '../AppointmentCalendar';
import { useLocalization } from '../../../hooks/useLocalization';
import { useAvailableSlots } from '../../../hooks/useAppointments';
import type { AppointmentSlot } from '../../../services/appointmentService';

// Mock hooks
vi.mock('../../../hooks/useLocalization');
vi.mock('../../../hooks/useAppointments');

const mockUseLocalization = vi.mocked(useLocalization);
const mockUseAvailableSlots = vi.mocked(useAvailableSlots);

const mockSlots: AppointmentSlot[] = [
  {
    date: '2024-01-15',
    time: '09:00',
    isAvailable: true,
    doctorId: 'doctor-1'
  },
  {
    date: '2024-01-15',
    time: '10:00',
    isAvailable: true,
    doctorId: 'doctor-1'
  },
  {
    date: '2024-01-16',
    time: '14:00',
    isAvailable: true,
    doctorId: 'doctor-1'
  }
];

describe('AppointmentCalendar', () => {
  const mockOnSlotSelect = vi.fn();
  const mockFetchAvailableSlots = vi.fn();

  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string) => key,
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn()
    });

    mockUseAvailableSlots.mockReturnValue({
      slots: mockSlots,
      loading: false,
      error: null,
      fetchAvailableSlots: mockFetchAvailableSlots
    });

    vi.clearAllMocks();
  });

  it('renders calendar with available slots', async () => {
    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
      />
    );

    await waitFor(() => {
      expect(mockFetchAvailableSlots).toHaveBeenCalled();
    });

    // Check if calendar navigation is present
    expect(screen.getByLabelText('common.previous')).toBeInTheDocument();
    expect(screen.getByLabelText('common.next')).toBeInTheDocument();
  });

  it('displays loading state', () => {
    mockUseAvailableSlots.mockReturnValue({
      slots: [],
      loading: true,
      error: null,
      fetchAvailableSlots: mockFetchAvailableSlots
    });

    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
      />
    );

    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('displays error state with retry button', () => {
    const errorMessage = 'Failed to load slots';
    mockUseAvailableSlots.mockReturnValue({
      slots: [],
      loading: false,
      error: errorMessage,
      fetchAvailableSlots: mockFetchAvailableSlots
    });

    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
      />
    );

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
    expect(screen.getByText('common.retry')).toBeInTheDocument();
  });

  it('calls onSlotSelect when a slot is clicked', async () => {
    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
      />
    );

    await waitFor(() => {
      expect(mockFetchAvailableSlots).toHaveBeenCalled();
    });

    // Find and click a time slot button
    const timeSlots = screen.getAllByText(/\d{1,2}:\d{2}/);
    if (timeSlots.length > 0) {
      fireEvent.click(timeSlots[0]);
      expect(mockOnSlotSelect).toHaveBeenCalled();
    }
  });

  it('navigates between months', async () => {
    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
      />
    );

    const nextButton = screen.getByLabelText('common.next');
    fireEvent.click(nextButton);

    await waitFor(() => {
      expect(mockFetchAvailableSlots).toHaveBeenCalledTimes(2);
    });
  });

  it('highlights selected slot', () => {
    const selectedSlot = mockSlots[0];
    
    render(
      <AppointmentCalendar
        selectedDoctorId="doctor-1"
        onSlotSelect={mockOnSlotSelect}
        selectedSlot={selectedSlot}
      />
    );

    // The selected slot should have different styling
    // This would need to be tested based on the actual implementation
    expect(screen.getByText('appointments.selected')).toBeInTheDocument();
  });
});