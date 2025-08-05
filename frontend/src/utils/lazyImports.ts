import { lazy } from 'react';

// Lazy load page components for better code splitting
export const LazyHome = lazy(() => import('../pages/Home'));
export const LazyLogin = lazy(() => import('../pages/auth/Login'));
export const LazyRegister = lazy(() => import('../pages/auth/Register'));
export const LazyForgotPassword = lazy(() => import('../pages/auth/ForgotPassword'));
export const LazyResetPassword = lazy(() => import('../pages/auth/ResetPassword'));

// E-commerce pages
export const LazyProducts = lazy(() => import('../pages/products/Products'));
export const LazyProductDetail = lazy(() => import('../pages/products/ProductDetail'));
export const LazyCart = lazy(() => import('../pages/cart/Cart'));
export const LazyCheckout = lazy(() => import('../pages/checkout/Checkout'));
export const LazyOrderHistory = lazy(() => import('../pages/orders/OrderHistory'));
export const LazyOrderDetail = lazy(() => import('../pages/orders/OrderDetail'));

// Course pages
export const LazyCourses = lazy(() => import('../pages/courses/Courses'));
export const LazyCourseDetail = lazy(() => import('../pages/courses/CourseDetail'));
export const LazyVideoPlayer = lazy(() => import('../pages/courses/VideoPlayer'));
export const LazyMyCourses = lazy(() => import('../pages/courses/MyCourses'));
export const LazyCertificates = lazy(() => import('../pages/courses/Certificates'));

// Appointment pages
export const LazyAppointments = lazy(() => import('../pages/appointments/Appointments'));
export const LazyBookAppointment = lazy(() => import('../pages/appointments/BookAppointment'));
export const LazyMyAppointments = lazy(() => import('../pages/appointments/MyAppointments'));

// Profile pages
export const LazyProfile = lazy(() => import('../pages/profile/Profile'));
export const LazySettings = lazy(() => import('../pages/profile/Settings'));

// Admin pages
export const LazyAdminDashboard = lazy(() => import('../pages/admin/Dashboard'));
export const LazyAdminProducts = lazy(() => import('../pages/admin/Products'));
export const LazyAdminOrders = lazy(() => import('../pages/admin/Orders'));
export const LazyAdminUsers = lazy(() => import('../pages/admin/Users'));
export const LazyAdminCourses = lazy(() => import('../pages/admin/Courses'));
export const LazyAdminAppointments = lazy(() => import('../pages/admin/Appointments'));
export const LazyAdminReports = lazy(() => import('../pages/admin/Reports'));

// Error pages
export const LazyNotFound = lazy(() => import('../pages/errors/NotFound'));
export const LazyUnauthorized = lazy(() => import('../pages/errors/Unauthorized'));
export const LazyServerError = lazy(() => import('../pages/errors/ServerError'));