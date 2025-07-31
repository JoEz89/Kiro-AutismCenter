// TypeScript type definitions

// Common types
export interface ApiResponse<T = unknown> {
  data: T;
  message?: string;
  success: boolean;
}

export interface ApiError {
  message: string;
  code: string;
  details?: Record<string, string[]>;
  timestamp: string;
  traceId: string;
}

// User types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  preferredLanguage: 'en' | 'ar';
  isEmailVerified: boolean;
  createdAt: Date;
}

export enum UserRole {
  USER = 'user',
  ADMIN = 'admin',
  DOCTOR = 'doctor',
}

// Auth types
export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  preferredLanguage: 'en' | 'ar';
}

// Language and theme types
export type Language = 'en' | 'ar';
export type Direction = 'ltr' | 'rtl';

export interface LanguageContextType {
  language: Language;
  direction: Direction;
  setLanguage: (lang: Language) => void;
  t: (key: string) => string;
}

// Product types
export interface Product {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn: string;
  descriptionAr: string;
  price: number;
  currency: 'USD' | 'BHD';
  stockQuantity: number;
  categoryId: string;
  imageUrls: string[];
  isActive: boolean;
}

export interface CartItem {
  productId: string;
  quantity: number;
  price: number;
}

// Order types
export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  items: CartItem[];
  totalAmount: number;
  currency: 'USD' | 'BHD';
  status: OrderStatus;
  paymentId: string;
  paymentStatus: PaymentStatus;
  shippingAddress: Address;
  billingAddress: Address;
  createdAt: Date;
  updatedAt: Date;
  shippedAt?: Date;
  deliveredAt?: Date;
}

export enum OrderStatus {
  PENDING = 'pending',
  CONFIRMED = 'confirmed',
  PROCESSING = 'processing',
  SHIPPED = 'shipped',
  DELIVERED = 'delivered',
  CANCELLED = 'cancelled',
  REFUNDED = 'refunded',
}

export enum PaymentStatus {
  PENDING = 'pending',
  COMPLETED = 'completed',
  FAILED = 'failed',
  REFUNDED = 'refunded',
}

export interface Address {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

// Course types
export interface Course {
  id: string;
  titleEn: string;
  titleAr: string;
  descriptionEn: string;
  descriptionAr: string;
  duration: number;
  thumbnailUrl: string;
  modules: CourseModule[];
  isActive: boolean;
}

export interface CourseModule {
  id: string;
  titleEn: string;
  titleAr: string;
  videoUrl: string;
  duration: number;
  order: number;
}

export interface Enrollment {
  id: string;
  userId: string;
  courseId: string;
  enrollmentDate: Date;
  expiryDate: Date;
  progress: number;
  completionDate?: Date;
  certificateUrl?: string;
}

// Appointment types
export interface Appointment {
  id: string;
  userId: string;
  doctorId: string;
  appointmentDate: Date;
  status: AppointmentStatus;
  zoomLink: string;
  patientInfo: PatientInfo;
  notes?: string;
}

export enum AppointmentStatus {
  SCHEDULED = 'scheduled',
  CONFIRMED = 'confirmed',
  CANCELLED = 'cancelled',
  COMPLETED = 'completed',
  NO_SHOW = 'no_show',
}

export interface PatientInfo {
  firstName: string;
  lastName: string;
  age: number;
  phone: string;
  notes?: string;
}

export interface Doctor {
  id: string;
  nameEn: string;
  nameAr: string;
  specialtyEn: string;
  specialtyAr: string;
  availability: DoctorAvailability[];
}

export interface DoctorAvailability {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}
