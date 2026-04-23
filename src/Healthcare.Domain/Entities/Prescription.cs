using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class Prescription : AuditableEntity
{
    public long AppointmentId { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Appointment? Appointment { get; set; }
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = [];
}
