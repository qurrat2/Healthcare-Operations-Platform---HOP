# Healthcare Operations Platform (HOP) Database Design

## 1. Database Approach

- Recommended database: PostgreSQL
- Design style: normalized relational schema
- Soft delete supported with `is_active`
- Auditing supported with dedicated audit log table
- Primary keys: `BIGSERIAL` or UUID, depending on implementation preference

This design below uses numeric IDs for readability.

## 2. Core Tables

### 2.1 roles

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| name | VARCHAR(50) | UNIQUE NOT NULL | `ADMIN`, `DOCTOR`, `RECEPTIONIST` |
| description | VARCHAR(255) | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |

### 2.2 users

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| role_id | BIGINT | FK -> roles.id NOT NULL | |
| username | VARCHAR(100) | UNIQUE NOT NULL | |
| password_hash | VARCHAR(255) | NOT NULL | |
| full_name | VARCHAR(150) | NOT NULL | |
| email | VARCHAR(150) | UNIQUE NULL | |
| phone | VARCHAR(30) | NULL | |
| last_login_at | TIMESTAMP | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

### 2.3 departments

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| name | VARCHAR(100) | UNIQUE NOT NULL | |
| description | TEXT | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

### 2.4 doctors

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| user_id | BIGINT | FK -> users.id UNIQUE NOT NULL | doctor user profile |
| department_id | BIGINT | FK -> departments.id NOT NULL | |
| license_number | VARCHAR(100) | UNIQUE NOT NULL | |
| specialization | VARCHAR(150) | NULL | |
| consultation_fee | NUMERIC(10,2) | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

### 2.5 doctor_availability

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| doctor_id | BIGINT | FK -> doctors.id NOT NULL | |
| day_of_week | VARCHAR(20) | NOT NULL | `MONDAY` to `SUNDAY` |
| start_time | TIME | NOT NULL | |
| end_time | TIME | NOT NULL | |
| slot_duration_minutes | INTEGER | NOT NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

Suggested check constraints:

- `end_time > start_time`
- `slot_duration_minutes > 0`

### 2.6 patients

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| mrn | VARCHAR(50) | UNIQUE NOT NULL | medical record number |
| first_name | VARCHAR(100) | NOT NULL | |
| last_name | VARCHAR(100) | NOT NULL | |
| date_of_birth | DATE | NOT NULL | |
| gender | VARCHAR(20) | NOT NULL | |
| phone | VARCHAR(30) | NULL | |
| email | VARCHAR(150) | NULL | |
| address | TEXT | NULL | |
| blood_group | VARCHAR(10) | NULL | |
| emergency_contact_name | VARCHAR(150) | NULL | |
| emergency_contact_phone | VARCHAR(30) | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

### 2.7 patient_dependents

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| primary_patient_id | BIGINT | FK -> patients.id NOT NULL | |
| dependent_patient_id | BIGINT | FK -> patients.id NOT NULL | |
| relationship | VARCHAR(50) | NOT NULL | `CHILD`, `SPOUSE`, `PARENT`, etc. |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

Suggested constraints:

- unique (`primary_patient_id`, `dependent_patient_id`)
- check (`primary_patient_id <> dependent_patient_id`)

### 2.8 appointments

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| patient_id | BIGINT | FK -> patients.id NOT NULL | |
| doctor_id | BIGINT | FK -> doctors.id NOT NULL | |
| department_id | BIGINT | FK -> departments.id NOT NULL | |
| appointment_date | DATE | NOT NULL | |
| start_time | TIME | NOT NULL | |
| end_time | TIME | NOT NULL | |
| status | VARCHAR(30) | NOT NULL DEFAULT 'SCHEDULED' | |
| reason | TEXT | NULL | |
| remarks | TEXT | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | receptionist/admin |
| updated_by | BIGINT | FK -> users.id NULL | |

Suggested constraints:

- `end_time > start_time`
- status in (`SCHEDULED`, `COMPLETED`, `CANCELLED`, `NO_SHOW`)

Recommended unique protection against double-booking:

- unique index on (`doctor_id`, `appointment_date`, `start_time`) where `is_active = true` and `status in ('SCHEDULED')`

If overlapping ranges must be blocked beyond exact start-time duplicates, enforce in application logic or use PostgreSQL exclusion constraints.

### 2.9 prescriptions

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| appointment_id | BIGINT | FK -> appointments.id NOT NULL | |
| patient_id | BIGINT | FK -> patients.id NOT NULL | |
| doctor_id | BIGINT | FK -> doctors.id NOT NULL | |
| diagnosis | TEXT | NULL | |
| notes | TEXT | NULL | |
| issued_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| created_by | BIGINT | FK -> users.id NULL | |
| updated_by | BIGINT | FK -> users.id NULL | |

### 2.10 prescription_items

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| prescription_id | BIGINT | FK -> prescriptions.id NOT NULL | |
| medicine_name | VARCHAR(150) | NOT NULL | |
| dosage | VARCHAR(100) | NOT NULL | |
| frequency | VARCHAR(100) | NOT NULL | |
| duration_days | INTEGER | NULL | |
| instructions | TEXT | NULL | |
| is_active | BOOLEAN | NOT NULL DEFAULT TRUE | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |
| updated_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |

Suggested check constraint:

- `duration_days IS NULL OR duration_days > 0`

### 2.11 audit_logs

| Column | Type | Constraints | Notes |
|---|---|---|---|
| id | BIGSERIAL | PK | |
| user_id | BIGINT | FK -> users.id NULL | |
| action | VARCHAR(50) | NOT NULL | `CREATE`, `UPDATE`, `DELETE`, `LOGIN`, `STATUS_CHANGE` |
| entity_type | VARCHAR(100) | NOT NULL | e.g. `PATIENT`, `APPOINTMENT` |
| entity_id | BIGINT | NOT NULL | target record ID |
| old_values | JSONB | NULL | snapshot before change |
| new_values | JSONB | NULL | snapshot after change |
| ip_address | VARCHAR(50) | NULL | |
| user_agent | TEXT | NULL | |
| created_at | TIMESTAMP | NOT NULL DEFAULT NOW() | |

## 3. Entity Relationships

### Main relationships

- one `role` to many `users`
- one `department` to many `doctors`
- one `user` to one `doctor`
- one `doctor` to many `doctor_availability`
- one `patient` to many `appointments`
- one `doctor` to many `appointments`
- one `appointment` to many `prescriptions` if revisions are allowed
- one `prescription` to many `prescription_items`
- many-to-many style dependent linkage through `patient_dependents`

## 4. Text ERD

```text
roles 1---* users
users 1---1 doctors
departments 1---* doctors
doctors 1---* doctor_availability
patients 1---* appointments
doctors 1---* appointments
departments 1---* appointments
appointments 1---* prescriptions
patients 1---* prescriptions
doctors 1---* prescriptions
prescriptions 1---* prescription_items
patients 1---* patient_dependents
patients 1---* patient_dependents (as dependent)
users 1---* audit_logs
```

## 5. Recommended Indexes

Create indexes on:

- `users(role_id, is_active)`
- `doctors(department_id, is_active)`
- `doctor_availability(doctor_id, day_of_week, is_active)`
- `patients(mrn)`
- `patients(phone)`
- `patients(last_name, first_name)`
- `appointments(patient_id, appointment_date)`
- `appointments(doctor_id, appointment_date, status)`
- `appointments(department_id, appointment_date, status)`
- `prescriptions(patient_id, issued_at)`
- `audit_logs(entity_type, entity_id)`
- `audit_logs(user_id, created_at)`

## 6. Sample SQL DDL Skeleton

```sql
CREATE TABLE roles (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    role_id BIGINT NOT NULL REFERENCES roles(id),
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(150) UNIQUE,
    phone VARCHAR(30),
    last_login_at TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by BIGINT REFERENCES users(id),
    updated_by BIGINT REFERENCES users(id)
);

CREATE TABLE departments (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by BIGINT REFERENCES users(id),
    updated_by BIGINT REFERENCES users(id)
);

CREATE TABLE doctors (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL UNIQUE REFERENCES users(id),
    department_id BIGINT NOT NULL REFERENCES departments(id),
    license_number VARCHAR(100) NOT NULL UNIQUE,
    specialization VARCHAR(150),
    consultation_fee NUMERIC(10,2),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by BIGINT REFERENCES users(id),
    updated_by BIGINT REFERENCES users(id)
);

CREATE TABLE patients (
    id BIGSERIAL PRIMARY KEY,
    mrn VARCHAR(50) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    gender VARCHAR(20) NOT NULL,
    phone VARCHAR(30),
    email VARCHAR(150),
    address TEXT,
    blood_group VARCHAR(10),
    emergency_contact_name VARCHAR(150),
    emergency_contact_phone VARCHAR(30),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by BIGINT REFERENCES users(id),
    updated_by BIGINT REFERENCES users(id)
);
```

## 7. Design Notes

- `users` handles authentication and role assignment
- `doctors` extends `users` with doctor-specific attributes
- `patients` and `users` are separate because patients do not log in in the current scope
- `patient_dependents` avoids duplicating patient demographic records
- `prescription_items` is separated from `prescriptions` because one prescription contains multiple medicines
- `audit_logs` stores before and after values for traceability

## 8. Future Extensions

These can be added later without major redesign:

- billing and invoices
- insurance providers and claims
- lab tests and reports
- file attachments for prescriptions or reports
- multi-branch hospital support using `clinic_id` on transactional tables
