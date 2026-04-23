using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Prescriptions;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/prescriptions")]
public sealed class PrescriptionsController(IPrescriptionService prescriptionService) : BaseApiController
{
    [Authorize(Roles = AppRoles.Doctor)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await prescriptionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PrescriptionResponse>.Ok(result, "Prescription created successfully"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "patient_id")] long? patientId, [FromQuery(Name = "appointment_id")] long? appointmentId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await prescriptionService.ListAsync(patientId, appointmentId, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<PrescriptionResponse>>.Ok(result));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await prescriptionService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Prescription not found")) : Ok(ApiResponse<PrescriptionResponse>.Ok(result));
    }

    [Authorize(Roles = AppRoles.Doctor)]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await prescriptionService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Prescription not found")) : Ok(ApiResponse<PrescriptionResponse>.Ok(result, "Prescription updated successfully"));
    }
}
