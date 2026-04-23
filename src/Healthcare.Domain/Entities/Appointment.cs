using Healthcare.Domain.Common;
using Healthcare.Domain.Constants;

namespace Healthcare.Domain.Entities;

public sealed class Appointment : AuditableEntity
{
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long DepartmentId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = AppointmentStatuses.Scheduled;
    public string? Reason { get; set; }
    public string? Remarks { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public Department? Department { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
