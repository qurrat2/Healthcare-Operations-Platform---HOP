# Functional Requirements Specification (FRS)

## Healthcare Operations Platform (HOP)

## 1. Introduction

### 1.1 Purpose

The purpose of this system is to provide a centralized healthcare management platform to manage:

- Patients and dependents
- Doctors and departments
- Appointments (OPD)
- Prescriptions and medical history

### 1.2 Scope

The system will:

- Provide REST APIs for healthcare operations
- Support role-based access
- Maintain historical medical data
- Enable appointment scheduling and tracking

### 1.3 Intended Users

- Admin
- Doctor
- Receptionist

## 2. User Roles and Permissions

### 2.1 Admin

- Manage users, including doctors and staff
- Configure departments
- View system-wide data
- Have full access to the platform

### 2.2 Doctor

- View assigned patients
- Write prescriptions
- View patient history

### 2.3 Receptionist

- Register patients
- Book appointments
- Manage schedules

## 3. Functional Requirements

### 3.1 Authentication and Authorization

**Features**

- User login using username and password
- JWT-based authentication
- Role-based access control (RBAC)

**Functional Requirements**

- `FR-1`: The system shall authenticate users using secure credentials.
- `FR-2`: The system shall issue JWT tokens upon successful login.
- `FR-3`: The system shall restrict access based on user roles.

### 3.2 Patient Management

**Features**

- Create, update, and view patients
- Manage dependents
- Store medical history

**Functional Requirements**

- `FR-4`: The system shall allow a receptionist to register new patients.
- `FR-5`: The system shall store patient demographics including name, age, gender, and contact information.
- `FR-6`: The system shall allow linking dependents to a primary patient.
- `FR-7`: The system shall allow viewing patient history.

### 3.3 Doctor and Department Management

**Features**

- Manage doctors
- Assign departments
- Define availability

**Functional Requirements**

- `FR-8`: The system shall allow an admin to create and manage doctors.
- `FR-9`: The system shall assign doctors to departments.
- `FR-10`: The system shall store doctor availability schedules.

### 3.4 Appointment (OPD) Management

**Features**

- Book appointments
- Assign doctors
- Track appointment status

**Functional Requirements**

- `FR-11`: The system shall allow a receptionist to book appointments.
- `FR-12`: The system shall assign appointments to available doctors.
- `FR-13`: The system shall track appointment status as Scheduled, Completed, or Cancelled.
- `FR-14`: The system shall prevent double-booking for the same time slot.

### 3.5 Prescription Management

**Features**

- Create prescriptions
- Store medicines
- Link prescriptions with appointments

**Functional Requirements**

- `FR-15`: The system shall allow doctors to create prescriptions.
- `FR-16`: The system shall store prescribed medicines and dosage instructions.
- `FR-17`: The system shall link prescriptions to appointments.
- `FR-18`: The system shall maintain prescription history.

### 3.6 Medical History

**Features**

- Centralized patient records
- Visit history

**Functional Requirements**

- `FR-19`: The system shall store all past appointments and prescriptions.
- `FR-20`: The system shall allow doctors to view complete patient history.

### 3.7 Audit Logging

**Features**

- Track changes
- Record user actions

**Functional Requirements**

- `FR-21`: The system shall log all critical actions, including create, update, and delete operations.
- `FR-22`: The system shall store the user, timestamp, and action details for each log entry.

### 3.8 Search and Filtering

**Features**

- Search patients
- Filter appointments

**Functional Requirements**

- `FR-23`: The system shall allow searching patients by name or contact information.
- `FR-24`: The system shall allow filtering appointments by date, doctor, and status.

### 3.9 Soft Delete and Data Integrity

**Features**

- Logical deletion of records

**Functional Requirements**

- `FR-25`: The system shall not permanently delete records.
- `FR-26`: The system shall mark deleted records as inactive.

## 4. Data Entities (High-Level)

**Core Entities**

- Patient
- Dependent
- Doctor
- Department
- Appointment
- Prescription
- Medicine
- User
- Role
- AuditLog

## 5. Key Workflows

### 5.1 Patient Registration Flow

1. Receptionist creates a patient record.
2. The system stores the patient record.
3. Optionally, dependents are added and linked to the primary patient.

### 5.2 Appointment Booking Flow

1. Receptionist selects a patient.
2. Receptionist selects a doctor and time slot.
3. The system validates doctor availability.
4. The appointment is created.

### 5.3 Prescription Flow

1. Doctor views the appointment.
2. Doctor adds a prescription.
3. The system stores prescribed medicines.
4. The prescription is linked to the patient's medical history.

## 6. Non-Functional Requirements

### 6.1 Performance

- API response time shall be less than 500 ms for standard operations.

### 6.2 Security

- JWT authentication
- Password hashing
- Role-based authorization

### 6.3 Scalability

- Modular architecture
- API-first design

### 6.4 Reliability

- Error-handling middleware
- Logging mechanism

## 7. Out of Scope

- Billing system
- Insurance integration
- Mobile application
- Advanced analytics

## 8. Assumptions

- The system is API-based, with no heavy frontend required initially.
- The system will support a single clinic or hospital in the first phase.
- Internet connectivity is available.
