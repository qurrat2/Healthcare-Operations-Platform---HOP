using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Auth;

internal sealed class ScaffoldAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue("X-User-Role", out var roleValues)
            ? roleValues.ToString()
            : "ADMIN";

        var userId = Request.Headers.TryGetValue("X-User-Id", out var userIdValues)
            ? userIdValues.ToString()
            : "1";

        var username = Request.Headers.TryGetValue("X-Username", out var usernameValues)
            ? usernameValues.ToString()
            : "scaffold.user";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
