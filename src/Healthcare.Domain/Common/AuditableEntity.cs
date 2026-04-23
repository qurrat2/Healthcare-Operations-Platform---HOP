namespace Healthcare.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
