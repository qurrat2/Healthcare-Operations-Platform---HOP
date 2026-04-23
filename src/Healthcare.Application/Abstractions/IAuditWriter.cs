namespace Healthcare.Application.Abstractions;

public interface IAuditWriter
{
    Task WriteAsync(
        string action,
        string entityType,
        long entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}
