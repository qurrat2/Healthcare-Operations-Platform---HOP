using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Reference;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("api/v1/roles")]
public sealed class RolesController(IReferenceDataService referenceDataService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await referenceDataService.GetRolesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ReferenceItemResponse>>.Ok(result));
    }
}
