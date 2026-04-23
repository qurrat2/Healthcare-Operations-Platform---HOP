using Healthcare.Contracts.Common;
using Healthcare.Contracts.Users;

namespace Healthcare.Application.Abstractions;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<UserResponse>> ListAsync(string? role, bool? isActive, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default);
}
