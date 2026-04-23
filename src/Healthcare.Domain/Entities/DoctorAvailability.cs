using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class DoctorAvailability : AuditableEntity
{
    public long DoctorId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }

    public Doctor? Doctor { get; set; }
}
