using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Audit;
using Healthcare.Contracts.Common;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("api/v1/audit-logs")]
public sealed class AuditLogsController(IAuditLogService auditLogService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "entity_type")] string? entityType, [FromQuery(Name = "entity_id")] long? entityId, [FromQuery(Name = "user_id")] long? userId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await auditLogService.ListAsync(entityType, entityId, userId, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogResponse>>.Ok(result));
    }
}
