using System.Text.Json;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Domain.Entities;

namespace Healthcare.Infrastructure.Services;

internal sealed class AuditWriter(
    IAuditLogRepository auditLogRepository,
    ICurrentUserContext currentUserContext,
    IRequestContext requestContext,
    IUnitOfWork unitOfWork) : IAuditWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task WriteAsync(
        string action,
        string entityType,
        long entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = currentUserContext.UserId,
            Action = action.Trim().ToUpperInvariant(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            OldValues = Serialize(oldValues),
            NewValues = Serialize(newValues),
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent
        };

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
}
