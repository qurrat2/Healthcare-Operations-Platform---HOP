using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Contracts.Audit;
using Healthcare.Contracts.Common;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task<PagedResult<AuditLogResponse>> ListAsync(string? entityType, long? entityId, long? userId, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);

        var query = auditLogRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalizedEntityType = entityType.Trim();
            query = query.Where(x => x.EntityType == normalizedEntityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("created_at", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var entries = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = entries.Select(x => new AuditLogResponse(
                x.Id,
                x.UserId,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.OldValues,
                x.NewValues,
                x.IpAddress,
                x.CreatedAt)).ToList()
        };
    }
}
