using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class Patient : AuditableEntity
{
    public string Mrn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public ICollection<PatientDependent> Dependents { get; set; } = [];
    public ICollection<PatientDependent> PrimaryLinks { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
