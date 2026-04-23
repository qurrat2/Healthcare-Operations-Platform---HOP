using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Doctors;

public sealed record CreateDoctorRequest(
    [property: Range(1, long.MaxValue)] long UserId,
    [property: Range(1, long.MaxValue)] long DepartmentId,
    [property: Required, StringLength(50, MinimumLength = 1)] string LicenseNumber,
    [property: StringLength(150)] string? Specialization,
    [property: Range(0, 1_000_000)] decimal? ConsultationFee);

public sealed record UpdateDoctorRequest(
    [property: Range(1, long.MaxValue)] long DepartmentId,
    [property: StringLength(150)] string? Specialization,
    [property: Range(0, 1_000_000)] decimal? ConsultationFee,
    bool IsActive);

public sealed record DoctorResponse(
    long Id,
    long UserId,
    long DepartmentId,
    string FullName,
    string LicenseNumber,
    string? Specialization,
    decimal? ConsultationFee,
    bool IsActive);

public sealed record UpsertDoctorAvailabilityRequest(
    [property: Required, RegularExpression("^(MONDAY|TUESDAY|WEDNESDAY|THURSDAY|FRIDAY|SATURDAY|SUNDAY|monday|tuesday|wednesday|thursday|friday|saturday|sunday)$", ErrorMessage = "DayOfWeek must be a valid weekday name")] string DayOfWeek,
    [property: Required] TimeOnly StartTime,
    [property: Required] TimeOnly EndTime,
    [property: Range(5, 240)] int SlotDurationMinutes);

public sealed record DoctorAvailabilityResponse(
    long Id,
    long DoctorId,
    string DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    bool IsActive);
