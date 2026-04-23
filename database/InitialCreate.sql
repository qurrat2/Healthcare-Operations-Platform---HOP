IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [audit_logs] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NULL,
    [Action] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] bigint NOT NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(50) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_audit_logs] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_audit_logs_action] CHECK ([Action] IN ('CREATE','UPDATE','DELETE','LOGIN','STATUS_CHANGE'))
);
GO

CREATE TABLE [departments] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_departments] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [patients] (
    [Id] bigint NOT NULL IDENTITY,
    [Mrn] nvarchar(50) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [DateOfBirth] date NOT NULL,
    [Gender] nvarchar(20) NOT NULL,
    [Phone] nvarchar(30) NULL,
    [Email] nvarchar(150) NULL,
    [Address] nvarchar(max) NULL,
    [BloodGroup] nvarchar(10) NULL,
    [EmergencyContactName] nvarchar(150) NULL,
    [EmergencyContactPhone] nvarchar(30) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_patients] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_patients_gender] CHECK ([Gender] IN ('MALE','FEMALE','OTHER'))
);
GO

CREATE TABLE [roles] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [patient_dependents] (
    [Id] bigint NOT NULL IDENTITY,
    [PrimaryPatientId] bigint NOT NULL,
    [DependentPatientId] bigint NOT NULL,
    [Relationship] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_patient_dependents] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_patient_dependents_not_same] CHECK ([PrimaryPatientId] <> [DependentPatientId]),
    CONSTRAINT [FK_patient_dependents_patients_DependentPatientId] FOREIGN KEY ([DependentPatientId]) REFERENCES [patients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_patient_dependents_patients_PrimaryPatientId] FOREIGN KEY ([PrimaryPatientId]) REFERENCES [patients] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [users] (
    [Id] bigint NOT NULL IDENTITY,
    [RoleId] bigint NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Email] nvarchar(150) NULL,
    [Phone] nvarchar(30) NULL,
    [LastLoginAt] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_users_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [roles] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [doctors] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [DepartmentId] bigint NOT NULL,
    [LicenseNumber] nvarchar(100) NOT NULL,
    [Specialization] nvarchar(150) NULL,
    [ConsultationFee] decimal(10,2) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_doctors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_doctors_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_doctors_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [appointments] (
    [Id] bigint NOT NULL IDENTITY,
    [PatientId] bigint NOT NULL,
    [DoctorId] bigint NOT NULL,
    [DepartmentId] bigint NOT NULL,
    [AppointmentDate] date NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'SCHEDULED',
    [Reason] nvarchar(max) NULL,
    [Remarks] nvarchar(max) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_appointments_status] CHECK ([Status] IN ('SCHEDULED','COMPLETED','CANCELLED','NO_SHOW')),
    CONSTRAINT [CK_appointments_time] CHECK ([EndTime] > [StartTime]),
    CONSTRAINT [FK_appointments_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_appointments_doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [doctors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_appointments_patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [patients] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [doctor_availability] (
    [Id] bigint NOT NULL IDENTITY,
    [DoctorId] bigint NOT NULL,
    [DayOfWeek] nvarchar(20) NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [SlotDurationMinutes] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_doctor_availability] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_doctor_availability_day] CHECK ([DayOfWeek] IN ('MONDAY','TUESDAY','WEDNESDAY','THURSDAY','FRIDAY','SATURDAY','SUNDAY')),
    CONSTRAINT [CK_doctor_availability_slot] CHECK ([SlotDurationMinutes] > 0),
    CONSTRAINT [CK_doctor_availability_time] CHECK ([EndTime] > [StartTime]),
    CONSTRAINT [FK_doctor_availability_doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [doctors] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [prescriptions] (
    [Id] bigint NOT NULL IDENTITY,
    [AppointmentId] bigint NOT NULL,
    [PatientId] bigint NOT NULL,
    [DoctorId] bigint NOT NULL,
    [Diagnosis] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [IssuedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_prescriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_prescriptions_appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [appointments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_prescriptions_doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [doctors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_prescriptions_patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [patients] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [prescription_items] (
    [Id] bigint NOT NULL IDENTITY,
    [PrescriptionId] bigint NOT NULL,
    [MedicineName] nvarchar(150) NOT NULL,
    [Dosage] nvarchar(100) NOT NULL,
    [Frequency] nvarchar(100) NOT NULL,
    [DurationDays] int NULL,
    [Instructions] nvarchar(max) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] bigint NULL,
    [UpdatedBy] bigint NULL,
    CONSTRAINT [PK_prescription_items] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_prescription_items_duration] CHECK ([DurationDays] IS NULL OR [DurationDays] > 0),
    CONSTRAINT [FK_prescription_items_prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [prescriptions] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_appointments_department_date_status] ON [appointments] ([DepartmentId], [AppointmentDate], [Status]);
GO

CREATE INDEX [IX_appointments_doctor_date_status] ON [appointments] ([DoctorId], [AppointmentDate], [Status]);
GO

CREATE INDEX [IX_appointments_patient_date] ON [appointments] ([PatientId], [AppointmentDate]);
GO

CREATE UNIQUE INDEX [UX_appointments_doctor_date_start_active] ON [appointments] ([DoctorId], [AppointmentDate], [StartTime]) WHERE [IsActive] = 1 AND [Status] = 'SCHEDULED';
GO

CREATE INDEX [IX_audit_logs_entity] ON [audit_logs] ([EntityType], [EntityId]);
GO

CREATE INDEX [IX_audit_logs_user_created_at] ON [audit_logs] ([UserId], [CreatedAt]);
GO

CREATE UNIQUE INDEX [IX_departments_Name] ON [departments] ([Name]);
GO

CREATE INDEX [IX_doctor_availability_doctor_day_active] ON [doctor_availability] ([DoctorId], [DayOfWeek], [IsActive]);
GO

CREATE INDEX [IX_doctors_department_id_is_active] ON [doctors] ([DepartmentId], [IsActive]);
GO

CREATE UNIQUE INDEX [IX_doctors_LicenseNumber] ON [doctors] ([LicenseNumber]);
GO

CREATE UNIQUE INDEX [IX_doctors_UserId] ON [doctors] ([UserId]);
GO

CREATE INDEX [IX_patient_dependents_DependentPatientId] ON [patient_dependents] ([DependentPatientId]);
GO

CREATE UNIQUE INDEX [IX_patient_dependents_PrimaryPatientId_DependentPatientId] ON [patient_dependents] ([PrimaryPatientId], [DependentPatientId]);
GO

CREATE INDEX [IX_patients_last_first_name] ON [patients] ([LastName], [FirstName]);
GO

CREATE UNIQUE INDEX [IX_patients_mrn] ON [patients] ([Mrn]);
GO

CREATE INDEX [IX_patients_phone] ON [patients] ([Phone]);
GO

CREATE INDEX [IX_prescription_items_PrescriptionId] ON [prescription_items] ([PrescriptionId]);
GO

CREATE INDEX [IX_prescriptions_AppointmentId] ON [prescriptions] ([AppointmentId]);
GO

CREATE INDEX [IX_prescriptions_DoctorId] ON [prescriptions] ([DoctorId]);
GO

CREATE INDEX [IX_prescriptions_patient_issued_at] ON [prescriptions] ([PatientId], [IssuedAt]);
GO

CREATE UNIQUE INDEX [IX_roles_Name] ON [roles] ([Name]);
GO

CREATE UNIQUE INDEX [IX_users_Email] ON [users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE INDEX [IX_users_role_id_is_active] ON [users] ([RoleId], [IsActive]);
GO

CREATE UNIQUE INDEX [IX_users_Username] ON [users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406083354_InitialCreate', N'8.0.12');
GO

COMMIT;
GO

