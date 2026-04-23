using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Prescriptions;

public sealed record CreatePrescriptionRequest(
    [property: Range(1, long.MaxValue)] long AppointmentId,
    [property: Range(1, long.MaxValue)] long PatientId,
    [property: StringLength(2000)] string? Notes,
    [property: StringLength(1000)] string? Diagnosis,
    [property: Required, MinLength(1, ErrorMessage = "At least one medicine is required")] IReadOnlyCollection<CreatePrescriptionItemRequest> Medicines);

public sealed record CreatePrescriptionItemRequest(
    [property: Required, StringLength(150, MinimumLength = 1)] string MedicineName,
    [property: Required, StringLength(100, MinimumLength = 1)] string Dosage,
    [property: Required, StringLength(100, MinimumLength = 1)] string Frequency,
    [property: Range(1, 365)] int? DurationDays,
    [property: StringLength(500)] string? Instructions);

public sealed record UpdatePrescriptionRequest(
    [property: StringLength(2000)] string? Notes,
    [property: StringLength(1000)] string? Diagnosis,
    [property: Required, MinLength(1, ErrorMessage = "At least one medicine is required")] IReadOnlyCollection<CreatePrescriptionItemRequest> Medicines);

public sealed record PrescriptionItemResponse(
    long Id,
    string MedicineName,
    string Dosage,
    string Frequency,
    int? DurationDays,
    string? Instructions);

public sealed record PrescriptionResponse(
    long Id,
    long AppointmentId,
    long PatientId,
    long DoctorId,
    string? Diagnosis,
    string? Notes,
    DateTime IssuedAt,
    IReadOnlyCollection<PrescriptionItemResponse> Medicines);
