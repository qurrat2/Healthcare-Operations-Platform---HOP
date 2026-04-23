using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Appointments;
using Healthcare.Contracts.Common;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/appointments")]
public sealed class AppointmentsController(IAppointmentService appointmentService) : BaseApiController
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AppointmentResponse>.Ok(result, "Appointment created successfully"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DateOnly? date,
        [FromQuery(Name = "from_date")] DateOnly? fromDate,
        [FromQuery(Name = "to_date")] DateOnly? toDate,
        [FromQuery(Name = "patient_id")] long? patientId,
        [FromQuery(Name = "doctor_id")] long? doctorId,
        [FromQuery(Name = "department_id")] long? departmentId,
        [FromQuery] string? status,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var filter = new AppointmentListFilter(date, fromDate, toDate, patientId, doctorId, departmentId, status);
        var result = await appointmentService.ListAsync(filter, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<AppointmentResponse>>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Appointment not found")) : Ok(ApiResponse<AppointmentResponse>.Ok(result));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await appointmentService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Appointment not found")) : Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment updated successfully"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist},{AppRoles.Doctor}")]
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await appointmentService.UpdateStatusAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Appointment not found")) : Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment status updated successfully"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Receptionist}")]
    [HttpGet("availability")]
    public async Task<IActionResult> Availability([FromQuery(Name = "doctor_id")] long doctorId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetAvailabilityAsync(doctorId, date, cancellationToken);
        return Ok(ApiResponse<AvailabilityResponse>.Ok(result));
    }
}
