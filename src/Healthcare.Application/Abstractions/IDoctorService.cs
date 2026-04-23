using Healthcare.Contracts.Common;
using Healthcare.Contracts.Doctors;

namespace Healthcare.Application.Abstractions;

public interface IDoctorService
{
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<DoctorResponse>> ListAsync(long? departmentId, bool? isActive, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<DoctorResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<DoctorResponse?> UpdateAsync(long id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<DoctorAvailabilityResponse> AddAvailabilityAsync(long doctorId, UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DoctorAvailabilityResponse>> ListAvailabilityAsync(long doctorId, CancellationToken cancellationToken = default);
    Task<DoctorAvailabilityResponse?> UpdateAvailabilityAsync(long doctorId, long availabilityId, UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveAvailabilityAsync(long doctorId, long availabilityId, CancellationToken cancellationToken = default);
}
