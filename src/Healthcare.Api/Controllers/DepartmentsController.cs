using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Departments;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/departments")]
public sealed class DepartmentsController(IDepartmentService departmentService) : BaseApiController
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await departmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DepartmentResponse>.Ok(result, "Department created successfully"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await departmentService.ListAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<DepartmentResponse>>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await departmentService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Department not found")) : Ok(ApiResponse<DepartmentResponse>.Ok(result));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await departmentService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("Department not found")) : Ok(ApiResponse<DepartmentResponse>.Ok(result, "Department updated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        var deleted = await departmentService.DeactivateAsync(id, cancellationToken);
        return deleted ? Ok(ApiResponse<object>.Ok(null, "Department deactivated successfully")) : NotFound(ApiResponse<object>.Fail("Department not found"));
    }
}
