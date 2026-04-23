using Healthcare.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Healthcare.Infrastructure.Auth;

internal sealed class RequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
