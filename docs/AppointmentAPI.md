# Appointment Scheduling API Documentation

This document describes the appointment scheduling API endpoints implemented in the Autism Center Website.

## Base URL
All endpoints are prefixed with `/api/appointments`

## Authentication
All endpoints require authentication via JWT token, except where noted. Admin-only endpoints require the "Admin" role.

## Endpoints

### 1. Book Appointment
**POST** `/api/appointments`

Books a new appointment for a patient.

**Request Body:**
```json
{
  "userId": "guid",
  "doctorId": "guid", 
  "appointmentDate": "2024-01-15T10:00:00Z",
  "durationInMinutes": 60,
  "patientName": "John Doe",
  "patientAge": 25,
  "medicalHistory": "No significant medical history",
  "currentConcerns": "Behavioral concerns",
  "emergencyContact": "Jane Doe",
  "emergencyPhone": "+1234567890"
}
```

**Response:**
```json
{
  "id": "guid",
  "appointmentNumber": "APT-2024-001234",
  "userId": "guid",
  "doctorId": "guid",
  "doctorNameEn": "Dr. John Smith",
  "doctorNameAr": "د. جون سميث",
  "appointmentDate": "2024-01-15T10:00:00Z",
  "durationInMinutes": 60,
  "status": "Scheduled",
  "patientName": "John Doe",
  "zoomJoinUrl": null
}
```

### 2. Get Appointments
**GET** `/api/appointments`

Retrieves appointments with optional filtering.

**Query Parameters:**
- `userId` (optional): Filter by user ID
- `doctorId` (optional): Filter by doctor ID
- `startDate` (optional): Filter appointments from this date
- `endDate` (optional): Filter appointments until this date
- `upcomingOnly` (optional): Show only upcoming appointments (default: false)

**Response:**
```json
{
  "appointments": [
    {
      "id": "guid",
      "appointmentNumber": "APT-2024-001234",
      "userId": "guid",
      "doctorId": "guid",
      "doctorNameEn": "Dr. John Smith",
      "doctorNameAr": "د. جون سميث",
      "doctorSpecialtyEn": "Autism Specialist",
      "doctorSpecialtyAr": "أخصائي التوحد",
      "appointmentDate": "2024-01-15T10:00:00Z",
      "endTime": "2024-01-15T11:00:00Z",
      "durationInMinutes": 60,
      "status": "Scheduled",
      "patientName": "John Doe",
      "patientAge": 25,
      "medicalHistory": "No significant medical history",
      "currentConcerns": "Behavioral concerns",
      "emergencyContact": "Jane Doe",
      "emergencyPhone": "+1234567890",
      "zoomMeetingId": null,
      "zoomJoinUrl": null,
      "notes": null,
      "createdAt": "2024-01-10T08:00:00Z"
    }
  ]
}
```

### 3. Get Appointment by ID
**GET** `/api/appointments/{id}`

Retrieves a specific appointment by ID.

**Response:** Single appointment object (same structure as in Get Appointments)

### 4. Cancel Appointment
**POST** `/api/appointments/{id}/cancel`

Cancels an existing appointment.

**Request Body:**
```json
{
  "userId": "guid"
}
```

**Response:** 204 No Content

### 5. Reschedule Appointment
**PUT** `/api/appointments/{id}/reschedule`

Reschedules an existing appointment to a new date/time.

**Request Body:**
```json
{
  "userId": "guid",
  "newAppointmentDate": "2024-01-16T14:00:00Z"
}
```

**Response:**
```json
{
  "id": "guid",
  "appointmentNumber": "APT-2024-001234",
  "oldAppointmentDate": "2024-01-15T10:00:00Z",
  "newAppointmentDate": "2024-01-16T14:00:00Z",
  "status": "Scheduled",
  "zoomJoinUrl": null
}
```

### 6. Get Available Slots
**GET** `/api/appointments/available-slots`

Retrieves available appointment slots for booking.

**Query Parameters:**
- `doctorId` (optional): Filter by specific doctor
- `startDate` (optional): Start date for availability search
- `endDate` (optional): End date for availability search
- `durationInMinutes` (optional): Duration of appointment (default: 60)

**Response:**
```json
{
  "doctorSlots": [
    {
      "doctorId": "guid",
      "doctorNameEn": "Dr. John Smith",
      "doctorNameAr": "د. جون سميث",
      "specialtyEn": "Autism Specialist",
      "specialtyAr": "أخصائي التوحد",
      "availableSlots": [
        {
          "startTime": "2024-01-15T10:00:00Z",
          "endTime": "2024-01-15T11:00:00Z",
          "durationInMinutes": 60,
          "isAvailable": true
        }
      ]
    }
  ]
}
```

### 7. Get Calendar View
**GET** `/api/appointments/calendar`

Retrieves a calendar view of appointments and availability.

**Query Parameters:**
- `startDate` (required): Start date for calendar view
- `endDate` (required): End date for calendar view
- `doctorId` (optional): Filter by specific doctor

**Response:**
```json
{
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": "2024-01-21T00:00:00Z",
  "days": [
    {
      "date": "2024-01-15T00:00:00Z",
      "appointments": [
        {
          "id": "guid",
          "appointmentNumber": "APT-2024-001234",
          "userId": "guid",
          "patientName": "John Doe",
          "doctorId": "guid",
          "doctorNameEn": "Dr. John Smith",
          "doctorNameAr": "د. جون سميث",
          "startTime": "2024-01-15T10:00:00Z",
          "endTime": "2024-01-15T11:00:00Z",
          "status": "Scheduled",
          "hasZoomMeeting": false
        }
      ],
      "availability": [
        {
          "doctorId": "guid",
          "doctorNameEn": "Dr. John Smith",
          "doctorNameAr": "د. جون سميث",
          "startTime": "09:00:00",
          "endTime": "17:00:00",
          "isActive": true
        }
      ]
    }
  ]
}
```

### 8. Create Zoom Meeting
**POST** `/api/appointments/{id}/zoom-meeting`

Creates a Zoom meeting for an existing appointment.

**Response:**
```json
{
  "appointmentId": "guid",
  "meetingId": "123456789",
  "joinUrl": "https://zoom.us/j/123456789",
  "topic": "Appointment with Dr. John Smith",
  "startTime": "2024-01-15T10:00:00Z",
  "durationInMinutes": 60
}
```

### 9. Get Doctor Availability
**GET** `/api/appointments/doctors/{doctorId}/availability`

Retrieves availability schedule for a specific doctor.

**Response:**
```json
{
  "doctorId": "guid",
  "doctorNameEn": "Dr. John Smith",
  "doctorNameAr": "د. جون سميث",
  "availability": [
    {
      "id": "guid",
      "dayOfWeek": "Monday",
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "isActive": true
    }
  ]
}
```

## Admin-Only Endpoints

### 10. Create Doctor Availability
**POST** `/api/appointments/doctors/{doctorId}/availability`

Creates availability schedule for a doctor. **Requires Admin role.**

**Request Body:**
```json
{
  "dayOfWeek": "Monday",
  "startTime": "09:00:00",
  "endTime": "17:00:00"
}
```

**Response:**
```json
{
  "id": "guid",
  "doctorId": "guid",
  "dayOfWeek": "Monday",
  "startTime": "09:00:00",
  "endTime": "17:00:00",
  "isActive": true
}
```

### 11. Update Doctor Availability
**PUT** `/api/appointments/doctors/{doctorId}/availability/{availabilityId}`

Updates existing doctor availability. **Requires Admin role.**

**Request Body:**
```json
{
  "startTime": "10:00:00",
  "endTime": "18:00:00",
  "isActive": true
}
```

**Response:** Same structure as Create Doctor Availability

### 12. Remove Doctor Availability
**DELETE** `/api/appointments/doctors/{doctorId}/availability/{availabilityId}`

Removes doctor availability schedule. **Requires Admin role.**

**Response:** 204 No Content

### 13. Send Appointment Reminder
**POST** `/api/appointments/{id}/send-reminder`

Sends a reminder notification for an appointment. **Requires Admin role.**

**Response:**
```json
{
  "message": "Reminder sent successfully",
  "appointmentId": "guid"
}
```

### 14. Send Appointment Notification
**POST** `/api/appointments/{id}/send-notification`

Sends a custom notification for an appointment. **Requires Admin role.**

**Request Body:**
```json
{
  "notificationType": "reminder",
  "customMessage": "Your appointment is tomorrow at 10 AM"
}
```

**Response:**
```json
{
  "message": "Notification sent successfully",
  "appointmentId": "guid",
  "notificationType": "reminder",
  "customMessage": "Your appointment is tomorrow at 10 AM"
}
```

## Error Responses

All endpoints return appropriate HTTP status codes:

- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `204 No Content` - Successful request with no response body
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

Error responses include a JSON object with error details:

```json
{
  "message": "Error description",
  "code": "ERROR_CODE",
  "details": {},
  "timestamp": "2024-01-15T10:00:00Z",
  "traceId": "trace-id"
}
```

## Requirements Fulfilled

This API implementation fulfills the following requirements:

- **5.1**: Display available appointment slots in calendar view
- **5.2**: Prevent double-booking of time slots
- **5.3**: Integrate with Zoom for video meeting links
- **5.4**: Send confirmation emails and appointment details
- **5.5**: Allow cancellation and rescheduling with notice
- **5.6**: Send appointment reminders and notifications