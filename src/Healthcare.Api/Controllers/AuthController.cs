using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Auth;
using Healthcare.Contracts.Common;
using Healthcare.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Receptionist}")]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await authService.GetCurrentUserAsync(cancellationToken);
        return Ok(ApiResponse<CurrentUserResponse>.Ok(result));
    }
}
