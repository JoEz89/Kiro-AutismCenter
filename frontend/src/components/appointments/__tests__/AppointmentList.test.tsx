import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import AppointmentList from '../AppointmentList';
import { useLocalization } from '../../../hooks/useLocalization';
import { useAppointments } from '../../../hooks/useAppointments';
import type { Appointment } from '../../../types';

// Mock hooks
vi.mock('../../../hooks/useLocalization');
vi.mock('../../../hooks/useAppointments');

const mockUseLocalization = vi.mocked(useLocalization);
const mockUseAppointments = vi.mocked(useAppointments);

const mockAppointments: Appointment[] = [
  {
    id: 'apt-1',
    userId: 'user-1',
    doctorId: 'doctor-1',
    appointmentDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), // Tomorrow
    status: 'confirmed',
    zoomLink: 'https://zoom.us/j/123456789',
    patientInfo: {
      firstName: 'John',
      lastName: 'Doe',
      age: 25,
      phone: '+973 1234 5678',
      notes: 'Test notes'
    },
    notes: 'Doctor notes'
  },
  {
    id: 'apt-2',
    userId: 'user-1',
    doctorId: 'doctor-1',
    appointmentDate: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // Yesterday
    status: 'completed',
    zoomLink: 'https://zoom.us/j/987654321',
    patientInfo: {
      firstName: 'Jane',
      lastName: 'Smith',
      age: 30,
      phone: '+973 9876 5432'
    }
  }
];

describe('AppointmentList', () => {
  const mockCancelAppointment = vi.fn();
  const mockOnReschedule = vi.fn();
  const mockOnCancel = vi.fn();

  beforeEach(() => {
    mockUseLocalization.mockReturnValue({
      t: (key: string, params?: any) => {
        if (params) {
          return key.replace(/\{\{(\w+)\}\}/g, (match, param) => params[param] || match);
        }
        return key;
      },
      language: 'en',
      direction: 'ltr',
      setLanguage: vi.fn()
    });

    mockUseAppointments.mockReturnValue({
      appointments: mockAppointments,
      loading: false,
      error: null,
      bookAppointment: vi.fn(),
      cancelAppointment: mockCancelAppointment,
      rescheduleAppointment: vi.fn(),
      refetch: vi.fn()
    });

    vi.clearAllMocks();
  });

  it('renders upcoming appointments when filter is upcoming', () => {
    render(
      <AppointmentList
        filter="upcoming"
        onReschedule={mockOnReschedule}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument();
  });

  it('renders past appointments when filter is past', () => {
    render(
      <AppointmentList
        filter="past"
        onReschedule={mockOnReschedule}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
    expect(screen.queryByText('John Doe')).not.toBeInTheDocument();
  });

  it('renders all appointments when filter is all', () => {
    render(
      <AppointmentList
        filter="all"
        onReschedule={mockOnReschedule}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
  });

  it('displays loading state', () => {
    mockUseAppointments.mockReturnValue({
      appointments: [],
      loading: true,
      error: null,
      bookAppointment: vi.fn(),
      cancelAppointment: mockCancelAppointment,
      rescheduleAppointment: vi.fn(),
      refetch: vi.fn()
    });

    render(<AppointmentList />);

    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('displays error state', () => {
    const errorMessage = 'Failed to load appointments';
    mockUseAppointments.mockReturnValue({
      appointments: [],
      loading: false,
      error: errorMessage,
      bookAppointment: vi.fn(),
      cancelAppointment: mockCancelAppointment,
      rescheduleAppointment: vi.fn(),
      refetch: vi.fn()
    });

    render(<AppointmentList />);

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
    expect(screen.getByText('common.retry')).toBeInTheDocument();
  });

  it('displays no appointments message when list is empty', () => {
    mockUseAppointments.mockReturnValue({
      appointments: [],
      loading: false,
      error: null,
      bookAppointment: vi.fn(),
      cancelAppointment: mockCancelAppointment,
      rescheduleAppointment: vi.fn(),
      refetch: vi.fn()
    });

    render(<AppointmentList />);

    expect(screen.getByText('appointments.noAppointments')).toBeInTheDocument();
  });

  it('shows reschedule button for upcoming appointments', () => {
    render(
      <AppointmentList
        filter="upcoming"
        onReschedule={mockOnReschedule}
      />
    );

    expect(screen.getByText('appointments.reschedule')).toBeInTheDocument();
  });

  it('calls onReschedule when reschedule button is clicked', () => {
    render(
      <AppointmentList
        filter="upcoming"
        onReschedule={mockOnReschedule}
      />
    );

    const rescheduleButton = screen.getByText('appointments.reschedule');
    fireEvent.click(rescheduleButton);

    expect(mockOnReschedule).toHaveBeenCalledWith(mockAppointments[0]);
  });

  it('shows cancel button for upcoming appointments', () => {
    render(
      <AppointmentList
        filter="upcoming"
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText('appointments.cancel')).toBeInTheDocument();
  });

  it('calls cancelAppointment when cancel button is clicked and confirmed', async () => {
    // Mock window.confirm to return true
    const originalConfirm = window.confirm;
    window.confirm = vi.fn(() => true);

    mockCancelAppointment.mockResolvedValue(undefined);

    render(
      <AppointmentList
        filter="upcoming"
        onCancel={mockOnCancel}
      />
    );

    const cancelButton = screen.getByText('appointments.cancel');
    fireEvent.click(cancelButton);

    await waitFor(() => {
      expect(mockCancelAppointment).toHaveBeenCalledWith('apt-1');
    });

    // Restore original confirm
    window.confirm = originalConfirm;
  });

  it('displays patient information correctly', () => {
    render(<AppointmentList />);

    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText(/appointments.age: 25/)).toBeInTheDocument();
    expect(screen.getByText(/appointments.phone: \+973 1234 5678/)).toBeInTheDocument();
  });

  it('displays notes when available', () => {
    render(<AppointmentList />);

    expect(screen.getByText(/Test notes/)).toBeInTheDocument();
    expect(screen.getByText(/Doctor notes/)).toBeInTheDocument();
  });

  it('shows join meeting button for appointments ready to join', () => {
    // Create an appointment that's ready to join (within 15 minutes)
    const readyAppointment: Appointment = {
      ...mockAppointments[0],
      appointmentDate: new Date(Date.now() + 10 * 60 * 1000).toISOString(), // 10 minutes from now
      status: 'confirmed'
    };

    mockUseAppointments.mockReturnValue({
      appointments: [readyAppointment],
      loading: false,
      error: null,
      bookAppointment: vi.fn(),
      cancelAppointment: mockCancelAppointment,
      rescheduleAppointment: vi.fn(),
      refetch: vi.fn()
    });

    render(<AppointmentList />);

    expect(screen.getByText('appointments.joinMeeting')).toBeInTheDocument();
  });
});