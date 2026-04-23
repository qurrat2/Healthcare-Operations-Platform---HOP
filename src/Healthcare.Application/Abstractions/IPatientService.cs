using Healthcare.Contracts.Common;
using Healthcare.Contracts.History;
using Healthcare.Contracts.Patients;
using Healthcare.Contracts.Prescriptions;

namespace Healthcare.Application.Abstractions;

public interface IPatientService
{
    Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PatientResponse>> ListAsync(PatientListFilter filter, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<PatientResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PatientResponse?> UpdateAsync(long id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default);
    Task<DependentResponse> AddDependentAsync(long patientId, AddDependentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DependentResponse>> ListDependentsAsync(long patientId, CancellationToken cancellationToken = default);
    Task<bool> RemoveDependentAsync(long patientId, long dependentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PrescriptionResponse>> GetPrescriptionsAsync(long patientId, CancellationToken cancellationToken = default);
    Task<PatientHistoryResponse?> GetHistoryAsync(long patientId, CancellationToken cancellationToken = default);
}
