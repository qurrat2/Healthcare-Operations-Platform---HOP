namespace Healthcare.Contracts.Audit;

public sealed record AuditLogResponse(
    long Id,
    long? UserId,
    string Action,
    string EntityType,
    long EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime CreatedAt);
