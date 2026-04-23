using Healthcare.Contracts.Common;
using Healthcare.Contracts.Prescriptions;

namespace Healthcare.Application.Abstractions;

public interface IPrescriptionService
{
    Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PrescriptionResponse>> ListAsync(long? patientId, long? appointmentId, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse?> UpdateAsync(long id, UpdatePrescriptionRequest request, CancellationToken cancellationToken = default);
}
