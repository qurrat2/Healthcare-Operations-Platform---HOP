using Healthcare.Contracts.Appointments;
using Healthcare.Contracts.Departments;
using Healthcare.Contracts.Patients;
using Healthcare.Contracts.Prescriptions;
using Healthcare.Contracts.Doctors;
using Healthcare.Contracts.Users;
using Healthcare.Domain.Entities;

namespace Healthcare.Infrastructure.Services.ServiceHelpers;

internal static class MappingExtensions
{
    public static UserResponse ToResponse(this User entity, string roleName) =>
        new(entity.Id, entity.Username, entity.FullName, roleName, entity.Email, entity.Phone, entity.IsActive);

    public static DepartmentResponse ToResponse(this Department entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.IsActive);

    public static PatientResponse ToResponse(this Patient entity) =>
        new(
            entity.Id,
            entity.Mrn,
            entity.FirstName,
            entity.LastName,
            entity.DateOfBirth,
            entity.Gender,
            entity.Phone,
            entity.Email,
            entity.IsActive);

    public static DependentResponse ToResponse(this PatientDependent entity) =>
        new(
            entity.Id,
            entity.PrimaryPatientId,
            entity.DependentPatientId,
            entity.Relationship,
            entity.IsActive);

    public static AppointmentResponse ToResponse(this Appointment entity) =>
        new(
            entity.Id,
            entity.PatientId,
            entity.DoctorId,
            entity.DepartmentId,
            entity.AppointmentDate,
            entity.StartTime,
            entity.EndTime,
            entity.Status,
            entity.Reason,
            entity.Remarks,
            entity.IsActive);

    public static DoctorResponse ToResponse(this Doctor entity) =>
        new(
            entity.Id,
            entity.UserId,
            entity.DepartmentId,
            entity.User?.FullName ?? string.Empty,
            entity.LicenseNumber,
            entity.Specialization,
            entity.ConsultationFee,
            entity.IsActive);

    public static DoctorAvailabilityResponse ToResponse(this DoctorAvailability entity) =>
        new(
            entity.Id,
            entity.DoctorId,
            entity.DayOfWeek,
            entity.StartTime,
            entity.EndTime,
            entity.SlotDurationMinutes,
            entity.IsActive);

    public static PrescriptionItemResponse ToResponse(this PrescriptionItem entity) =>
        new(entity.Id, entity.MedicineName, entity.Dosage, entity.Frequency, entity.DurationDays, entity.Instructions);

    public static PrescriptionResponse ToResponse(this Prescription entity) =>
        new(
            entity.Id,
            entity.AppointmentId,
            entity.PatientId,
            entity.DoctorId,
            entity.Diagnosis,
            entity.Notes,
            entity.IssuedAt,
            entity.Items.Select(x => x.ToResponse()).ToArray());
}
