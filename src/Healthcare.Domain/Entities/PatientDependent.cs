using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class PatientDependent : AuditableEntity
{
    public long PrimaryPatientId { get; set; }
    public long DependentPatientId { get; set; }
    public string Relationship { get; set; } = string.Empty;

    public Patient? PrimaryPatient { get; set; }
    public Patient? DependentPatient { get; set; }
}
