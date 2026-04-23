using Healthcare.Contracts.Common;
using Healthcare.Contracts.Departments;

namespace Healthcare.Application.Abstractions;

public interface IDepartmentService
{
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<DepartmentResponse>> ListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<DepartmentResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<DepartmentResponse?> UpdateAsync(long id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default);
}
