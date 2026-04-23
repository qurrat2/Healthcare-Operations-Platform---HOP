using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class User : AuditableEntity
{
    public long RoleId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Role? Role { get; set; }
    public Doctor? DoctorProfile { get; set; }
}
