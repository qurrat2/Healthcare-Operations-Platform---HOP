using Healthcare.Contracts.Auth;

namespace Healthcare.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
