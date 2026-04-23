using Healthcare.Contracts.Appointments;
using Healthcare.Contracts.Common;

namespace Healthcare.Application.Abstractions;

public interface IAppointmentService
{
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentResponse>> ListAsync(AppointmentListFilter filter, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<AppointmentResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<AppointmentResponse?> UpdateAsync(long id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentResponse?> UpdateStatusAsync(long id, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken = default);
    Task<AvailabilityResponse> GetAvailabilityAsync(long doctorId, DateOnly date, CancellationToken cancellationToken = default);
}
