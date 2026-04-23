# Healthcare Operations Platform (HOP) REST API Specification

## 1. API Overview

- Base URL: `/api/v1`
- Format: JSON
- Auth: JWT Bearer token
- Time format: ISO 8601
- Soft delete: Records use `is_active` and audit fields instead of permanent deletion

## 2. Roles

- `ADMIN`
- `DOCTOR`
- `RECEPTIONIST`

## 3. Common Conventions

### Headers

```http
Authorization: Bearer <jwt_token>
Content-Type: application/json
```

### Standard Response

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {}
}
```

### Standard Error Response

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "doctor_id",
      "message": "Doctor is not available for the selected time slot"
    }
  ]
}
```

### Pagination

- Query params:
  - `page` default `1`
  - `limit` default `20`
  - `sort_by`
  - `sort_order` (`asc`, `desc`)

Example:

`GET /api/v1/patients?page=1&limit=20&sort_by=created_at&sort_order=desc`

## 4. Authentication & Authorization

### 4.1 Login

`POST /api/v1/auth/login`

Access:
- Public

Request:

```json
{
  "username": "doctor1",
  "password": "SecurePassword123"
}
```

Response:

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "access_token": "jwt-token",
    "token_type": "Bearer",
    "expires_in": 3600,
    "user": {
      "id": 12,
      "username": "doctor1",
      "role": "DOCTOR",
      "full_name": "Dr. Ayesha Khan"
    }
  }
}
```

### 4.2 Current User Profile

`GET /api/v1/auth/me`

Access:
- `ADMIN`
- `DOCTOR`
- `RECEPTIONIST`

Response:

```json
{
  "success": true,
  "data": {
    "id": 12,
    "username": "doctor1",
    "full_name": "Dr. Ayesha Khan",
    "role": "DOCTOR",
    "department": {
      "id": 3,
      "name": "Cardiology"
    }
  }
}
```

## 5. User Management

### 5.1 Create User

`POST /api/v1/users`

Access:
- `ADMIN`

Request:

```json
{
  "username": "reception1",
  "password": "SecurePassword123",
  "full_name": "Sara Ali",
  "email": "sara@example.com",
  "phone": "+923001112233",
  "role": "RECEPTIONIST"
}
```

### 5.2 List Users

`GET /api/v1/users?role=DOCTOR&is_active=true`

Access:
- `ADMIN`

### 5.3 Get User

`GET /api/v1/users/{id}`

Access:
- `ADMIN`

### 5.4 Update User

`PUT /api/v1/users/{id}`

Access:
- `ADMIN`

### 5.5 Deactivate User

`DELETE /api/v1/users/{id}`

Access:
- `ADMIN`

Behavior:
- Soft delete only
- Sets `is_active = false`

## 6. Department Management

### 6.1 Create Department

`POST /api/v1/departments`

Access:
- `ADMIN`

Request:

```json
{
  "name": "Cardiology",
  "description": "Heart and vascular care"
}
```

### 6.2 List Departments

`GET /api/v1/departments`

Access:
- `ADMIN`
- `DOCTOR`
- `RECEPTIONIST`

### 6.3 Get Department

`GET /api/v1/departments/{id}`

Access:
- `ADMIN`
- `DOCTOR`
- `RECEPTIONIST`

### 6.4 Update Department

`PUT /api/v1/departments/{id}`

Access:
- `ADMIN`

### 6.5 Deactivate Department

`DELETE /api/v1/departments/{id}`

Access:
- `ADMIN`

## 7. Doctor Management

### 7.1 Create Doctor

`POST /api/v1/doctors`

Access:
- `ADMIN`

Request:

```json
{
  "user_id": 12,
  "department_id": 3,
  "license_number": "PMC-100234",
  "specialization": "Cardiologist",
  "consultation_fee": 2500
}
```

### 7.2 List Doctors

`GET /api/v1/doctors?department_id=3&is_active=true`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 7.3 Get Doctor

`GET /api/v1/doctors/{id}`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 7.4 Update Doctor

`PUT /api/v1/doctors/{id}`

Access:
- `ADMIN`

### 7.5 Doctor Availability

`POST /api/v1/doctors/{id}/availability`

Access:
- `ADMIN`

Request:

```json
{
  "day_of_week": "MONDAY",
  "start_time": "09:00:00",
  "end_time": "13:00:00",
  "slot_duration_minutes": 20
}
```

`GET /api/v1/doctors/{id}/availability`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

`PUT /api/v1/doctors/{id}/availability/{availability_id}`

Access:
- `ADMIN`

`DELETE /api/v1/doctors/{id}/availability/{availability_id}`

Access:
- `ADMIN`

## 8. Patient Management

### 8.1 Register Patient

`POST /api/v1/patients`

Access:
- `RECEPTIONIST`
- `ADMIN`

Request:

```json
{
  "mrn": "MRN-100001",
  "first_name": "Ahmed",
  "last_name": "Raza",
  "date_of_birth": "1990-08-14",
  "gender": "MALE",
  "phone": "+923001234567",
  "email": "ahmed@example.com",
  "address": "Lahore, Pakistan",
  "blood_group": "O+"
}
```

### 8.2 List/Search Patients

`GET /api/v1/patients?search=ahmed&phone=+923001234567&is_active=true`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

Behavior:
- Search by first name, last name, MRN, and contact number

### 8.3 Get Patient

`GET /api/v1/patients/{id}`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 8.4 Update Patient

`PUT /api/v1/patients/{id}`

Access:
- `ADMIN`
- `RECEPTIONIST`

### 8.5 Deactivate Patient

`DELETE /api/v1/patients/{id}`

Access:
- `ADMIN`

## 9. Dependent Management

### 9.1 Add Dependent

`POST /api/v1/patients/{id}/dependents`

Access:
- `RECEPTIONIST`
- `ADMIN`

Request:

```json
{
  "dependent_patient_id": 25,
  "relationship": "CHILD"
}
```

### 9.2 List Dependents

`GET /api/v1/patients/{id}/dependents`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 9.3 Remove Dependent Link

`DELETE /api/v1/patients/{id}/dependents/{dependent_id}`

Access:
- `ADMIN`
- `RECEPTIONIST`

Behavior:
- Only removes relationship link
- Does not delete patient record

## 10. Appointment Management

### 10.1 Book Appointment

`POST /api/v1/appointments`

Access:
- `RECEPTIONIST`
- `ADMIN`

Request:

```json
{
  "patient_id": 40,
  "doctor_id": 8,
  "department_id": 3,
  "appointment_date": "2026-04-10",
  "start_time": "10:00:00",
  "end_time": "10:20:00",
  "reason": "Chest pain consultation"
}
```

Business rules:
- Doctor must be active
- Patient must be active
- Appointment must fit within doctor availability
- No overlapping appointment allowed for same doctor and time slot

### 10.2 List Appointments

`GET /api/v1/appointments?date=2026-04-10&doctor_id=8&status=SCHEDULED`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

Behavior:
- Doctors only see their own appointments unless admin override exists

### 10.3 Get Appointment

`GET /api/v1/appointments/{id}`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 10.4 Update Appointment

`PUT /api/v1/appointments/{id}`

Access:
- `RECEPTIONIST`
- `ADMIN`

### 10.5 Update Appointment Status

`PATCH /api/v1/appointments/{id}/status`

Access:
- `RECEPTIONIST`
- `ADMIN`
- `DOCTOR`

Request:

```json
{
  "status": "COMPLETED"
}
```

Allowed statuses:
- `SCHEDULED`
- `COMPLETED`
- `CANCELLED`
- `NO_SHOW`

### 10.6 Check Doctor Slot Availability

`GET /api/v1/appointments/availability?doctor_id=8&date=2026-04-10`

Access:
- `ADMIN`
- `RECEPTIONIST`

Response:

```json
{
  "success": true,
  "data": {
    "doctor_id": 8,
    "date": "2026-04-10",
    "available_slots": [
      "09:00:00",
      "09:20:00",
      "10:40:00"
    ]
  }
}
```

## 11. Prescription Management

### 11.1 Create Prescription

`POST /api/v1/prescriptions`

Access:
- `DOCTOR`

Request:

```json
{
  "appointment_id": 105,
  "patient_id": 40,
  "notes": "Patient advised rest and hydration",
  "diagnosis": "Mild viral infection",
  "medicines": [
    {
      "medicine_name": "Paracetamol",
      "dosage": "500mg",
      "frequency": "3 times daily",
      "duration_days": 5,
      "instructions": "After meals"
    }
  ]
}
```

Business rules:
- Prescription must be linked to an existing appointment
- Doctor can only create prescription for own appointment
- One appointment may have one or more prescription entries if revisions are allowed

### 11.2 List Prescriptions

`GET /api/v1/prescriptions?patient_id=40&appointment_id=105`

Access:
- `ADMIN`
- `DOCTOR`

### 11.3 Get Prescription

`GET /api/v1/prescriptions/{id}`

Access:
- `ADMIN`
- `DOCTOR`

### 11.4 Update Prescription

`PUT /api/v1/prescriptions/{id}`

Access:
- `DOCTOR`

### 11.5 Patient Prescription History

`GET /api/v1/patients/{id}/prescriptions`

Access:
- `ADMIN`
- `DOCTOR`

## 12. Medical History

### 12.1 Full Patient History

`GET /api/v1/patients/{id}/history`

Access:
- `ADMIN`
- `DOCTOR`

Response includes:
- Patient profile
- Appointment history
- Diagnoses
- Prescriptions
- Linked dependents

## 13. Audit Logs

### 13.1 List Audit Logs

`GET /api/v1/audit-logs?entity_type=PATIENT&entity_id=40&user_id=12`

Access:
- `ADMIN`

Response fields:
- `id`
- `user_id`
- `action`
- `entity_type`
- `entity_id`
- `old_values`
- `new_values`
- `ip_address`
- `created_at`

## 14. Reference Data

### 14.1 Roles

`GET /api/v1/roles`

Access:
- `ADMIN`

### 14.2 Appointment Statuses

`GET /api/v1/reference/appointment-statuses`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

### 14.3 Genders

`GET /api/v1/reference/genders`

Access:
- `ADMIN`
- `RECEPTIONIST`
- `DOCTOR`

## 15. Suggested Validation Rules

- `username` must be unique
- `email` should be unique where provided
- `phone` should be unique per user record where required by business rules
- `department.name` must be unique
- `patient.mrn` must be unique
- `doctor.license_number` must be unique
- `appointment.end_time` must be greater than `start_time`
- Completed appointments should not be rescheduled directly; create update flow rules explicitly
- Cancelled appointments cannot have active prescriptions

## 16. Suggested HTTP Status Codes

- `200 OK` for successful reads and updates
- `201 Created` for successful creation
- `400 Bad Request` for business or validation failures
- `401 Unauthorized` for missing or invalid token
- `403 Forbidden` for role restriction
- `404 Not Found` for missing records
- `409 Conflict` for duplicate data or slot collision
- `422 Unprocessable Entity` for validation issues
- `500 Internal Server Error` for unexpected errors

## 17. Minimum MVP Endpoint Set

For the first implementation sprint, prioritize:

- `POST /auth/login`
- `GET /auth/me`
- CRUD for `/patients`
- CRUD for `/doctors`
- CRUD for `/departments`
- `POST /appointments`
- `GET /appointments`
- `PATCH /appointments/{id}/status`
- `POST /prescriptions`
- `GET /patients/{id}/history`
