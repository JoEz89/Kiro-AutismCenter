import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { AppointmentBookingForm } from '../AppointmentBookingForm';
import { AuthProvider } from '@/context/AuthContext';
import { LanguageProvider } from '@/context/LanguageContext';
import { appointmentService } from '@/services/appointmentService';
import { testAccessibility } from '@/test/accessibility';

// Mock the appointment service
vi.mock('@/services/appointmentService', () => ({
  appointmentService: {
    getAvailableSlots: vi.fn(),
    bookAppointment: vi.fn(),
    getDoctors: vi.fn(),
  },
}));

const mockAppointmentService = vi.mocked(appointmentService);

const mockDoctors = [
  {
    id: '1',
    nameEn: 'Dr. Sarah Johnson',
    nameAr: 'د. سارة جونسون',
    specialtyEn: 'Autism Specialist',
    specialtyAr: 'أخصائي التوحد',
    availability: [],
  },
  {
    id: '2',
    nameEn: 'Dr. Ahmed Hassan',
    nameAr: 'د. أحمد حسن',
    specialtyEn: 'Child Psychologist',
    specialtyAr: 'طبيب نفسي للأطفال',
    availability: [],
  },
];

const mockAvailableSlots = [
  {
    id: '1',
    doctorId: '1',
    date: '2024-02-15',
    time: '09:00',
    isAvailable: true,
  },
  {
    id: '2',
    doctorId: '1',
    date: '2024-02-15',
    time: '10:00',
    isAvailable: true,
  },
  {
    id: '3',
    doctorId: '1',
    date: '2024-02-16',
    time: '14:00',
    isAvailable: true,
  },
];

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <LanguageProvider>
        <AuthProvider>
          {component}
        </AuthProvider>
      </LanguageProvider>
    </BrowserRouter>
  );
};

describe('AppointmentBookingForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    mockAppointmentService.getDoctors.mockResolvedValue({
      data: mockDoctors,
      success: true,
    });

    mockAppointmentService.getAvailableSlots.mockResolvedValue({
      data: mockAvailableSlots,
      success: true,
    });
  });

  it('should render appointment booking form correctly', async () => {
    renderWithProviders(<AppointmentBookingForm />);

    expect(screen.getByText(/book an appointment/i)).toBeInTheDocument();
    expect(screen.getByText(/select doctor/i)).toBeInTheDocument();
    expect(screen.getByText(/patient information/i)).toBeInTheDocument();

    // Wait for doctors to load
    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
      expect(screen.getByText('Dr. Ahmed Hassan')).toBeInTheDocument();
    });
  });

  it('should handle doctor selection', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Select a doctor
    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    // Should show selected state
    expect(doctorCard).toHaveClass('selected');

    // Should load available slots
    await waitFor(() => {
      expect(mockAppointmentService.getAvailableSlots).toHaveBeenCalledWith('1');
    });
  });

  it('should display available time slots', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Select doctor
    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    // Should show calendar with available slots
    await waitFor(() => {
      expect(screen.getByText(/february 2024/i)).toBeInTheDocument();
      expect(screen.getByText('15')).toBeInTheDocument();
      expect(screen.getByText('16')).toBeInTheDocument();
    });

    // Click on a date
    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    // Should show time slots for that date
    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
      expect(screen.getByText('10:00')).toBeInTheDocument();
    });
  });

  it('should handle time slot selection', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Select doctor and date
    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });

    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
    });

    // Select time slot
    const timeSlot = screen.getByText('09:00');
    await user.click(timeSlot);

    // Should show selected state
    expect(timeSlot.closest('[data-testid="time-slot"]')).toHaveClass('selected');

    // Should enable patient information form
    expect(screen.getByLabelText(/patient name/i)).toBeEnabled();
  });

  it('should validate patient information form', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    // Complete doctor and time selection first
    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });

    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
    });

    const timeSlot = screen.getByText('09:00');
    await user.click(timeSlot);

    // Try to submit without patient information
    const submitButton = screen.getByRole('button', { name: /book appointment/i });
    await user.click(submitButton);

    // Should show validation errors
    await waitFor(() => {
      expect(screen.getByText(/patient name is required/i)).toBeInTheDocument();
      expect(screen.getByText(/phone number is required/i)).toBeInTheDocument();
      expect(screen.getByText(/reason for visit is required/i)).toBeInTheDocument();
    });
  });

  it('should handle successful appointment booking', async () => {
    const user = userEvent.setup();
    
    mockAppointmentService.bookAppointment.mockResolvedValue({
      data: {
        id: 'apt-123',
        appointmentNumber: 'APT-2024-001234',
        doctorId: '1',
        date: '2024-02-15',
        time: '09:00',
        zoomLink: 'https://zoom.us/j/123456789',
        status: 'confirmed',
      },
      success: true,
    });

    renderWithProviders(<AppointmentBookingForm />);

    // Complete the booking flow
    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Select doctor
    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    // Select date
    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });
    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    // Select time
    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
    });
    const timeSlot = screen.getByText('09:00');
    await user.click(timeSlot);

    // Fill patient information
    const patientNameInput = screen.getByLabelText(/patient name/i);
    const phoneInput = screen.getByLabelText(/phone number/i);
    const reasonInput = screen.getByLabelText(/reason for visit/i);

    await user.type(patientNameInput, 'John Doe');
    await user.type(phoneInput, '+973 1234 5678');
    await user.type(reasonInput, 'Initial consultation for autism assessment');

    // Submit form
    const submitButton = screen.getByRole('button', { name: /book appointment/i });
    await user.click(submitButton);

    // Should show success message
    await waitFor(() => {
      expect(screen.getByText(/appointment booked successfully/i)).toBeInTheDocument();
      expect(screen.getByText('APT-2024-001234')).toBeInTheDocument();
      expect(screen.getByText(/zoom meeting link/i)).toBeInTheDocument();
    });

    expect(mockAppointmentService.bookAppointment).toHaveBeenCalledWith({
      doctorId: '1',
      slotId: '1',
      patientInfo: {
        name: 'John Doe',
        phone: '+973 1234 5678',
        reason: 'Initial consultation for autism assessment',
      },
    });
  });

  it('should handle booking errors', async () => {
    const user = userEvent.setup();
    
    mockAppointmentService.bookAppointment.mockRejectedValue(
      new Error('Appointment slot no longer available')
    );

    renderWithProviders(<AppointmentBookingForm />);

    // Complete booking flow
    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });
    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
    });
    const timeSlot = screen.getByText('09:00');
    await user.click(timeSlot);

    // Fill form and submit
    await user.type(screen.getByLabelText(/patient name/i), 'John Doe');
    await user.type(screen.getByLabelText(/phone number/i), '+973 1234 5678');
    await user.type(screen.getByLabelText(/reason for visit/i), 'Consultation');

    const submitButton = screen.getByRole('button', { name: /book appointment/i });
    await user.click(submitButton);

    // Should show error message
    await waitFor(() => {
      expect(screen.getByText(/appointment slot no longer available/i)).toBeInTheDocument();
    });
  });

  it('should show loading states during booking', async () => {
    const user = userEvent.setup();
    
    // Mock slow booking response
    mockAppointmentService.bookAppointment.mockImplementation(
      () => new Promise(resolve => setTimeout(() => resolve({
        data: { id: 'apt-123' },
        success: true,
      }), 100))
    );

    renderWithProviders(<AppointmentBookingForm />);

    // Complete booking flow quickly
    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });
    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
    });
    const timeSlot = screen.getByText('09:00');
    await user.click(timeSlot);

    await user.type(screen.getByLabelText(/patient name/i), 'John Doe');
    await user.type(screen.getByLabelText(/phone number/i), '+973 1234 5678');
    await user.type(screen.getByLabelText(/reason for visit/i), 'Consultation');

    const submitButton = screen.getByRole('button', { name: /book appointment/i });
    await user.click(submitButton);

    // Should show loading state
    expect(screen.getByText(/booking appointment/i)).toBeInTheDocument();
    expect(submitButton).toBeDisabled();
  });

  it('should handle calendar navigation', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText(/february 2024/i)).toBeInTheDocument();
    });

    // Navigate to next month
    const nextMonthButton = screen.getByRole('button', { name: /next month/i });
    await user.click(nextMonthButton);

    expect(screen.getByText(/march 2024/i)).toBeInTheDocument();

    // Navigate to previous month
    const prevMonthButton = screen.getByRole('button', { name: /previous month/i });
    await user.click(prevMonthButton);

    expect(screen.getByText(/february 2024/i)).toBeInTheDocument();
  });

  it('should disable past dates and unavailable slots', async () => {
    const user = userEvent.setup();
    
    // Mock slots with some unavailable
    const slotsWithUnavailable = [
      ...mockAvailableSlots,
      {
        id: '4',
        doctorId: '1',
        date: '2024-02-15',
        time: '11:00',
        isAvailable: false,
      },
    ];

    mockAppointmentService.getAvailableSlots.mockResolvedValue({
      data: slotsWithUnavailable,
      success: true,
    });

    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    const doctorCard = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    await user.click(doctorCard!);

    await waitFor(() => {
      expect(screen.getByText('15')).toBeInTheDocument();
    });

    const dateButton = screen.getByText('15');
    await user.click(dateButton);

    await waitFor(() => {
      expect(screen.getByText('09:00')).toBeInTheDocument();
      expect(screen.getByText('11:00')).toBeInTheDocument();
    });

    // Available slot should be clickable
    const availableSlot = screen.getByText('09:00').closest('[data-testid="time-slot"]');
    expect(availableSlot).not.toHaveClass('disabled');

    // Unavailable slot should be disabled
    const unavailableSlot = screen.getByText('11:00').closest('[data-testid="time-slot"]');
    expect(unavailableSlot).toHaveClass('disabled');
  });

  it('should be accessible', async () => {
    const renderResult = renderWithProviders(<AppointmentBookingForm />);
    await testAccessibility(renderResult);
  });

  it('should support keyboard navigation', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Should be able to navigate doctors with keyboard
    const firstDoctor = screen.getByText('Dr. Sarah Johnson').closest('[data-testid="doctor-card"]');
    firstDoctor?.focus();
    expect(firstDoctor).toHaveFocus();

    // Arrow keys should navigate between doctors
    await user.keyboard('{ArrowDown}');
    const secondDoctor = screen.getByText('Dr. Ahmed Hassan').closest('[data-testid="doctor-card"]');
    expect(secondDoctor).toHaveFocus();

    // Enter should select doctor
    await user.keyboard('{Enter}');
    expect(secondDoctor).toHaveClass('selected');
  });

  it('should handle timezone display correctly', async () => {
    renderWithProviders(<AppointmentBookingForm />);

    await waitFor(() => {
      expect(screen.getByText('Dr. Sarah Johnson')).toBeInTheDocument();
    });

    // Should show timezone information
    expect(screen.getByText(/all times shown in/i)).toBeInTheDocument();
    expect(screen.getByText(/bahrain time/i)).toBeInTheDocument();
  });
});