using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class Doctor : AuditableEntity
{
    public long UserId { get; set; }
    public long DepartmentId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public decimal? ConsultationFee { get; set; }

    public User? User { get; set; }
    public Department? Department { get; set; }
    public ICollection<DoctorAvailability> AvailabilitySlots { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
