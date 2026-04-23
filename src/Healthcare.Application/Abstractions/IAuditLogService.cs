using Healthcare.Contracts.Audit;
using Healthcare.Contracts.Common;

namespace Healthcare.Application.Abstractions;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogResponse>> ListAsync(string? entityType, long? entityId, long? userId, PaginationRequest pagination, CancellationToken cancellationToken = default);
}
