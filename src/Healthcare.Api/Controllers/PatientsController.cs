using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.History;
using Healthcare.Contracts.Patients;
using Healthcare.Contracts.Prescriptions;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/patients")]
public sealed class PatientsController(IPatientService patientService) : BaseApiController
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await patientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PatientResponse>.Ok(result, "Patient created successfully"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? phone,
        [FromQuery] string? mrn,
        [FromQuery] string? email,
        [FromQuery(Name = "is_active")] bool? isActive,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var filter = new PatientListFilter(search, phone, mrn, email, isActive);
        var result = await patientService.ListAsync(filter, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<PatientResponse>>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await patientService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Patient not found")) : Ok(ApiResponse<PatientResponse>.Ok(result));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await patientService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Patient not found")) : Ok(ApiResponse<PatientResponse>.Ok(result, "Patient updated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        var deleted = await patientService.DeactivateAsync(id, cancellationToken);
        return deleted ? Ok(ApiResponse<object>.Ok(null, "Patient deactivated successfully")) : NotFound(ApiResponse<object>.Fail("Patient not found"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpPost("{id:long}/dependents")]
    public async Task<IActionResult> AddDependent(long id, [FromBody] AddDependentRequest request, CancellationToken cancellationToken)
    {
        var result = await patientService.AddDependentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DependentResponse>.Ok(result, "Dependent linked successfully"));
    }

    [Authorize]
    [HttpGet("{id:long}/dependents")]
    public async Task<IActionResult> ListDependents(long id, CancellationToken cancellationToken)
    {
        var result = await patientService.ListDependentsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<DependentResponse>>.Ok(result));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpDelete("{id:long}/dependents/{dependentId:long}")]
    public async Task<IActionResult> RemoveDependent(long id, long dependentId, CancellationToken cancellationToken)
    {
        var deleted = await patientService.RemoveDependentAsync(id, dependentId, cancellationToken);
        return deleted ? Ok(ApiResponse<object>.Ok(null, "Dependent link removed successfully")) : NotFound(ApiResponse<object>.Fail("Dependent link not found"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
    [HttpGet("{id:long}/prescriptions")]
    public async Task<IActionResult> GetPrescriptions(long id, CancellationToken cancellationToken)
    {
        var result = await patientService.GetPrescriptionsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PrescriptionResponse>>.Ok(result));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> GetHistory(long id, CancellationToken cancellationToken)
    {
        var result = await patientService.GetHistoryAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Patient history not found")) : Ok(ApiResponse<PatientHistoryResponse>.Ok(result));
    }
}
