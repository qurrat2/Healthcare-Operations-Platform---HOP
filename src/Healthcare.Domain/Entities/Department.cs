using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
