using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class PrescriptionItem : AuditableEntity
{
    public long PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }

    public Prescription? Prescription { get; set; }
}
