using Healthcare.Contracts.Reference;

namespace Healthcare.Application.Abstractions;

public interface IReferenceDataService
{
    Task<IReadOnlyCollection<ReferenceItemResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReferenceItemResponse>> GetAppointmentStatusesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReferenceItemResponse>> GetGendersAsync(CancellationToken cancellationToken = default);
}
