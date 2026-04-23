using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Users;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("api/v1/users")]
public sealed class UsersController(IUserService userService) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UserResponse>.Ok(result, "User created successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? role, [FromQuery(Name = "is_active")] bool? isActive, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await userService.ListAsync(role, isActive, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("User not found")) : Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound(ApiResponse<object>.Fail("User not found")) : Ok(ApiResponse<UserResponse>.Ok(result, "User updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        var deleted = await userService.DeactivateAsync(id, cancellationToken);
        return deleted ? Ok(ApiResponse<object>.Ok(null, "User deactivated successfully")) : NotFound(ApiResponse<object>.Fail("User not found"));
    }
}
