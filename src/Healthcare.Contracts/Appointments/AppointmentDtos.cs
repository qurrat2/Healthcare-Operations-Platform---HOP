using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Appointments;

public sealed record CreateAppointmentRequest(
    [property: Range(1, long.MaxValue)] long PatientId,
    [property: Range(1, long.MaxValue)] long DoctorId,
    [property: Range(1, long.MaxValue)] long DepartmentId,
    [property: Required] DateOnly AppointmentDate,
    [property: Required] TimeOnly StartTime,
    [property: Required] TimeOnly EndTime,
    [property: StringLength(500)] string? Reason);

public sealed record UpdateAppointmentRequest(
    [property: Range(1, long.MaxValue)] long DoctorId,
    [property: Range(1, long.MaxValue)] long DepartmentId,
    [property: Required] DateOnly AppointmentDate,
    [property: Required] TimeOnly StartTime,
    [property: Required] TimeOnly EndTime,
    [property: StringLength(500)] string? Reason,
    [property: StringLength(500)] string? Remarks);

public sealed record UpdateAppointmentStatusRequest(
    [property: Required, RegularExpression("^(SCHEDULED|COMPLETED|CANCELLED|NO_SHOW|scheduled|completed|cancelled|no_show)$", ErrorMessage = "Status must be SCHEDULED, COMPLETED, CANCELLED, or NO_SHOW")] string Status);

public sealed record AppointmentListFilter(
    DateOnly? Date,
    DateOnly? FromDate,
    DateOnly? ToDate,
    long? PatientId,
    long? DoctorId,
    long? DepartmentId,
    string? Status);

public sealed record AppointmentResponse(
    long Id,
    long PatientId,
    long DoctorId,
    long DepartmentId,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    string? Reason,
    string? Remarks,
    bool IsActive);

public sealed record AvailabilityResponse(
    long DoctorId,
    DateOnly Date,
    IReadOnlyCollection<TimeOnly> AvailableSlots);
