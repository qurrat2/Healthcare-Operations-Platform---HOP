using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Authorize]
[Route("api/v1/reference")]
public sealed class ReferenceController(IReferenceDataService referenceDataService) : BaseApiController
{
    [HttpGet("appointment-statuses")]
    public async Task<IActionResult> AppointmentStatuses(CancellationToken cancellationToken)
    {
        var result = await referenceDataService.GetAppointmentStatusesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ReferenceItemResponse>>.Ok(result));
    }

    [HttpGet("genders")]
    public async Task<IActionResult> Genders(CancellationToken cancellationToken)
    {
        var result = await referenceDataService.GetGendersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ReferenceItemResponse>>.Ok(result));
    }
}
