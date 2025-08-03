import apiClient from './api';
import type { 
  Appointment, 
  Doctor, 
  PatientInfo, 
  ApiResponse 
} from '../types';

export interface AppointmentSlot {
  date: string;
  time: string;
  isAvailable: boolean;
  doctorId: string;
}

export interface BookAppointmentRequest {
  doctorId: string;
  appointmentDate: string;
  patientInfo: PatientInfo;
}

export interface AvailabilityRequest {
  doctorId?: string;
  startDate: string;
  endDate: string;
}

export const appointmentService = {
  // Get available appointment slots
  async getAvailableSlots(params: AvailabilityRequest): Promise<AppointmentSlot[]> {
    const response = await apiClient.get<ApiResponse<AppointmentSlot[]>>('/appointments/availability', {
      params
    });
    return response.data.data;
  },

  // Get all doctors
  async getDoctors(): Promise<Doctor[]> {
    const response = await apiClient.get<ApiResponse<Doctor[]>>('/doctors');
    return response.data.data;
  },

  // Get doctor by ID
  async getDoctorById(id: string): Promise<Doctor> {
    const response = await apiClient.get<ApiResponse<Doctor>>(`/doctors/${id}`);
    return response.data.data;
  },

  // Book an appointment
  async bookAppointment(appointmentData: BookAppointmentRequest): Promise<Appointment> {
    const response = await apiClient.post<ApiResponse<Appointment>>('/appointments', appointmentData);
    return response.data.data;
  },

  // Get user's appointments
  async getUserAppointments(): Promise<Appointment[]> {
    const response = await apiClient.get<ApiResponse<Appointment[]>>('/appointments');
    return response.data.data;
  },

  // Get appointment by ID
  async getAppointmentById(id: string): Promise<Appointment> {
    const response = await apiClient.get<ApiResponse<Appointment>>(`/appointments/${id}`);
    return response.data.data;
  },

  // Cancel appointment
  async cancelAppointment(id: string): Promise<void> {
    await apiClient.put(`/appointments/${id}/cancel`);
  },

  // Reschedule appointment
  async rescheduleAppointment(id: string, newDate: string): Promise<Appointment> {
    const response = await apiClient.put<ApiResponse<Appointment>>(`/appointments/${id}/reschedule`, {
      appointmentDate: newDate
    });
    return response.data.data;
  }
};

export default appointmentService;