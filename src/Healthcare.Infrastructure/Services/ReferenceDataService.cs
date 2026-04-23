using Healthcare.Application.Abstractions;
using Healthcare.Contracts.Reference;
using Healthcare.Domain.Constants;

namespace Healthcare.Infrastructure.Services;

internal sealed class ReferenceDataService : IReferenceDataService
{
    public Task<IReadOnlyCollection<ReferenceItemResponse>> GetAppointmentStatusesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ReferenceItemResponse>>(
        [
            new(AppointmentStatuses.Scheduled, "Scheduled"),
            new(AppointmentStatuses.Completed, "Completed"),
            new(AppointmentStatuses.Cancelled, "Cancelled"),
            new(AppointmentStatuses.NoShow, "No Show")
        ]);

    public Task<IReadOnlyCollection<ReferenceItemResponse>> GetGendersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ReferenceItemResponse>>(
        [
            new(Genders.Male, "Male"),
            new(Genders.Female, "Female"),
            new(Genders.Other, "Other")
        ]);

    public Task<IReadOnlyCollection<ReferenceItemResponse>> GetRolesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ReferenceItemResponse>>(
        [
            new(AppRoles.Admin, "Admin"),
            new(AppRoles.Doctor, "Doctor"),
            new(AppRoles.Receptionist, "Receptionist")
        ]);
}
