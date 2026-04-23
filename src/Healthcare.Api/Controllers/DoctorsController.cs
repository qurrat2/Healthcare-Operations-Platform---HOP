using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Doctors;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/doctors")]
public sealed class DoctorsController(IDoctorService doctorService) : BaseApiController
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DoctorResponse>.Ok(result, "Doctor created successfully"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "department_id")] long? departmentId, [FromQuery(Name = "is_active")] bool? isActive, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await doctorService.ListAsync(departmentId, isActive, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<DoctorResponse>>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await doctorService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Doctor not found")) : Ok(ApiResponse<DoctorResponse>.Ok(result));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Doctor not found")) : Ok(ApiResponse<DoctorResponse>.Ok(result, "Doctor updated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{id:long}/availability")]
    public async Task<IActionResult> AddAvailability(long id, [FromBody] UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.AddAvailabilityAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DoctorAvailabilityResponse>.Ok(result, "Doctor availability created successfully"));
    }

    [Authorize]
    [HttpGet("{id:long}/availability")]
    public async Task<IActionResult> ListAvailability(long id, CancellationToken cancellationToken)
    {
        var result = await doctorService.ListAvailabilityAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<DoctorAvailabilityResponse>>.Ok(result));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:long}/availability/{availabilityId:long}")]
    public async Task<IActionResult> UpdateAvailability(long id, long availabilityId, [FromBody] UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.UpdateAvailabilityAsync(id, availabilityId, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Doctor availability not found")) : Ok(ApiResponse<DoctorAvailabilityResponse>.Ok(result, "Doctor availability updated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:long}/availability/{availabilityId:long}")]
    public async Task<IActionResult> RemoveAvailability(long id, long availabilityId, CancellationToken cancellationToken)
    {
        var deleted = await doctorService.RemoveAvailabilityAsync(id, availabilityId, cancellationToken);
        return deleted ? Ok(ApiResponse<object>.Ok(null, "Doctor availability removed successfully")) : NotFound(ApiResponse<object>.Fail("Doctor availability not found"));
    }
}
