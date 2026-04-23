using Healthcare.Domain.Common;

namespace Healthcare.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
