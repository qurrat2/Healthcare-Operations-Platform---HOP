using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Application.Exceptions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Prescriptions;
using Healthcare.Domain.Constants;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class PrescriptionService(
    IPrescriptionRepository prescriptionRepository,
    IPrescriptionItemRepository prescriptionItemRepository,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    ICurrentUserContext currentUserContext,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IPrescriptionService
{
    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.NotFound, "Appointment not found");

        await ValidatePrescriptionRequestAsync(request.PatientId, appointment, cancellationToken);

        var prescription = new Prescription
        {
            AppointmentId = request.AppointmentId,
            PatientId = request.PatientId,
            DoctorId = appointment.DoctorId,
            Notes = request.Notes?.Trim(),
            Diagnosis = request.Diagnosis?.Trim()
        };

        foreach (var medicine in request.Medicines)
        {
            prescription.Items.Add(new PrescriptionItem
            {
                MedicineName = medicine.MedicineName.Trim(),
                Dosage = medicine.Dosage.Trim(),
                Frequency = medicine.Frequency.Trim(),
                DurationDays = medicine.DurationDays,
                Instructions = medicine.Instructions?.Trim()
            });
        }

        await prescriptionRepository.AddAsync(prescription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("CREATE", nameof(Prescription), prescription.Id, newValues: ToAuditModel(prescription), cancellationToken: cancellationToken);
        return prescription.ToResponse();
    }

    public async Task<PrescriptionResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptionRepository.Query()
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (prescription is not null)
        {
            await EnsureDoctorCanAccessPrescriptionAsync(prescription.DoctorId, cancellationToken);
        }

        return prescription?.ToResponse();
    }

    public async Task<PagedResult<PrescriptionResponse>> ListAsync(long? patientId, long? appointmentId, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);

        var query = prescriptionRepository.Query()
            .AsNoTracking()
            .Include(x => x.Items)
            .AsQueryable();

        if (patientId.HasValue)
        {
            query = query.Where(x => x.PatientId == patientId.Value);
        }

        if (appointmentId.HasValue)
        {
            query = query.Where(x => x.AppointmentId == appointmentId.Value);
        }

        if (string.Equals(currentUserContext.Role, AppRoles.Doctor, StringComparison.OrdinalIgnoreCase))
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync(cancellationToken);
            query = query.Where(x => x.DoctorId == currentDoctorId);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("issued_at", "asc") => query.OrderBy(x => x.IssuedAt),
            _ => query.OrderByDescending(x => x.IssuedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var prescriptions = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<PrescriptionResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = prescriptions.Select(x => x.ToResponse()).ToList()
        };
    }

    public async Task<PrescriptionResponse?> UpdateAsync(long id, UpdatePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptionRepository.Query()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (prescription is null)
        {
            return null;
        }

        await EnsureDoctorOwnsPrescriptionAsync(prescription.DoctorId, cancellationToken);

        var oldValues = ToAuditModel(prescription);
        prescription.Notes = request.Notes?.Trim();
        prescription.Diagnosis = request.Diagnosis?.Trim();

        var existingItems = prescription.Items.ToList();
        foreach (var item in existingItems)
        {
            item.IsActive = false;
            prescriptionItemRepository.Update(item);
        }

        foreach (var medicine in request.Medicines)
        {
            prescription.Items.Add(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineName = medicine.MedicineName.Trim(),
                Dosage = medicine.Dosage.Trim(),
                Frequency = medicine.Frequency.Trim(),
                DurationDays = medicine.DurationDays,
                Instructions = medicine.Instructions?.Trim()
            });
        }

        prescriptionRepository.Update(prescription);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("UPDATE", nameof(Prescription), prescription.Id, oldValues, ToAuditModel(prescription), cancellationToken);
        return prescription.ToResponse();
    }

    private static object ToAuditModel(Prescription prescription) => new
    {
        prescription.Id,
        prescription.AppointmentId,
        prescription.PatientId,
        prescription.DoctorId,
        prescription.Notes,
        prescription.Diagnosis,
        prescription.IsActive,
        Items = prescription.Items
            .Where(x => x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.MedicineName,
                x.Dosage,
                x.Frequency,
                x.DurationDays,
                x.Instructions,
                x.IsActive
            })
            .ToList()
    };

    private async Task ValidatePrescriptionRequestAsync(long patientId, Appointment appointment, CancellationToken cancellationToken)
    {
        if (!appointment.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Prescription must be linked to an active appointment");
        }

        if (appointment.Status == AppointmentStatuses.Cancelled)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Cancelled appointments cannot have active prescriptions");
        }

        if (appointment.PatientId != patientId)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Prescription patient must match the appointment patient");
        }

        var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken);
        if (patient is null || !patient.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Patient must exist and be active");
        }

        await EnsureDoctorOwnsPrescriptionAsync(appointment.DoctorId, cancellationToken);
    }

    private async Task EnsureDoctorOwnsPrescriptionAsync(long doctorId, CancellationToken cancellationToken)
    {
        var currentDoctorId = await GetCurrentDoctorIdAsync(cancellationToken);
        if (currentDoctorId != doctorId)
        {
            throw new ApiException(HttpStatusCode.Forbidden, "Doctor can only manage prescriptions for their own appointments");
        }
    }

    private async Task EnsureDoctorCanAccessPrescriptionAsync(long doctorId, CancellationToken cancellationToken)
    {
        if (!string.Equals(currentUserContext.Role, AppRoles.Doctor, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await EnsureDoctorOwnsPrescriptionAsync(doctorId, cancellationToken);
    }

    private async Task<long> GetCurrentDoctorIdAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId
            ?? throw new ApiException(HttpStatusCode.Unauthorized, "Authenticated user context is missing");

        var doctor = await doctorRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        if (doctor is null)
        {
            throw new ApiException(HttpStatusCode.Forbidden, "Doctor profile not found for authenticated user");
        }

        return doctor.Id;
    }
}
