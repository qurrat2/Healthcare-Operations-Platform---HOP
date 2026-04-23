using System.Security.Claims;
using Healthcare.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Healthcare.Infrastructure.Auth;

internal sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public long? UserId
    {
        get
        {
            var rawValue = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(rawValue, out var parsed) ? parsed : null;
        }
    }

    public string? Username => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? Role => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
