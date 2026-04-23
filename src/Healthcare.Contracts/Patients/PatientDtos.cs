using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Patients;

public sealed record CreatePatientRequest(
    [property: Required, StringLength(50, MinimumLength = 1)] string Mrn,
    [property: Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [property: Required, StringLength(100, MinimumLength = 1)] string LastName,
    [property: Required] DateOnly DateOfBirth,
    [property: Required, RegularExpression("^(MALE|FEMALE|OTHER|male|female|other)$", ErrorMessage = "Gender must be MALE, FEMALE, or OTHER")] string Gender,
    [property: Phone, StringLength(30)] string? Phone,
    [property: EmailAddress, StringLength(150)] string? Email,
    [property: StringLength(500)] string? Address,
    [property: StringLength(10)] string? BloodGroup);

public sealed record UpdatePatientRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [property: Required, StringLength(100, MinimumLength = 1)] string LastName,
    [property: Required] DateOnly DateOfBirth,
    [property: Required, RegularExpression("^(MALE|FEMALE|OTHER|male|female|other)$", ErrorMessage = "Gender must be MALE, FEMALE, or OTHER")] string Gender,
    [property: Phone, StringLength(30)] string? Phone,
    [property: EmailAddress, StringLength(150)] string? Email,
    [property: StringLength(500)] string? Address,
    [property: StringLength(10)] string? BloodGroup,
    [property: StringLength(150)] string? EmergencyContactName,
    [property: Phone, StringLength(30)] string? EmergencyContactPhone,
    bool IsActive);

public sealed record PatientListFilter(
    string? Search,
    string? Phone,
    string? Mrn,
    string? Email,
    bool? IsActive);

public sealed record PatientResponse(
    long Id,
    string Mrn,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string? Phone,
    string? Email,
    bool IsActive);

public sealed record AddDependentRequest(
    [property: Range(1, long.MaxValue)] long DependentPatientId,
    [property: Required, StringLength(50, MinimumLength = 1)] string Relationship);

public sealed record DependentResponse(
    long Id,
    long PrimaryPatientId,
    long DependentPatientId,
    string Relationship,
    bool IsActive);
