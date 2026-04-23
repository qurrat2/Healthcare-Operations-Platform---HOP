using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Contracts.Appointments;
using Healthcare.Contracts.Common;
using Healthcare.Application.Exceptions;
using Healthcare.Domain.Constants;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IDepartmentRepository departmentRepository,
    IDoctorAvailabilityRepository doctorAvailabilityRepository,
    IPrescriptionRepository prescriptionRepository,
    ICurrentUserContext currentUserContext,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IAppointmentService
{
    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAppointmentRequestAsync(request.PatientId, request.DoctorId, request.DepartmentId, request.AppointmentDate, request.StartTime, request.EndTime, null, cancellationToken);

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DepartmentId = request.DepartmentId,
            AppointmentDate = request.AppointmentDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason?.Trim(),
            Status = AppointmentStatuses.Scheduled
        };

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("CREATE", nameof(Appointment), appointment.Id, newValues: ToAuditModel(appointment), cancellationToken: cancellationToken);

        return appointment.ToResponse();
    }

    public async Task<AvailabilityResponse> GetAvailabilityAsync(long doctorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var doctor = await doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor is null || !doctor.IsActive)
        {
            throw new ApiException(HttpStatusCode.NotFound, "Doctor not found");
        }

        var dayName = date.DayOfWeek.ToString().ToUpperInvariant();
        var availability = await doctorAvailabilityRepository.Query()
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.DayOfWeek == dayName && x.IsActive)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        var scheduledAppointments = await appointmentRepository.Query()
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId &&
                        x.AppointmentDate == date &&
                        x.IsActive &&
                        x.Status == AppointmentStatuses.Scheduled)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        var availableSlots = new List<TimeOnly>();
        foreach (var slot in availability)
        {
            var cursor = slot.StartTime;
            while (cursor.AddMinutes(slot.SlotDurationMinutes) <= slot.EndTime)
            {
                var proposedEnd = cursor.AddMinutes(slot.SlotDurationMinutes);
                var overlaps = scheduledAppointments.Any(x => Overlaps(cursor, proposedEnd, x.StartTime, x.EndTime));
                if (!overlaps)
                {
                    availableSlots.Add(cursor);
                }

                cursor = proposedEnd;
            }
        }

        return new AvailabilityResponse(doctorId, date, availableSlots);
    }

    public async Task<AppointmentResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (appointment is not null)
        {
            await EnsureDoctorCanAccessAppointmentAsync(appointment, cancellationToken);
        }

        return appointment?.ToResponse();
    }

    public async Task<PagedResult<AppointmentResponse>> ListAsync(AppointmentListFilter filter, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);
        var normalizedFilter = NormalizeFilter(filter);

        var query = appointmentRepository.Query().AsNoTracking();

        if (normalizedFilter.FromDate.HasValue)
        {
            query = query.Where(x => x.AppointmentDate >= normalizedFilter.FromDate.Value);
        }

        if (normalizedFilter.ToDate.HasValue)
        {
            query = query.Where(x => x.AppointmentDate <= normalizedFilter.ToDate.Value);
        }

        if (normalizedFilter.PatientId.HasValue)
        {
            query = query.Where(x => x.PatientId == normalizedFilter.PatientId.Value);
        }

        if (normalizedFilter.DoctorId.HasValue)
        {
            query = query.Where(x => x.DoctorId == normalizedFilter.DoctorId.Value);
        }

        if (normalizedFilter.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == normalizedFilter.DepartmentId.Value);
        }

        if (string.Equals(currentUserContext.Role, AppRoles.Doctor, StringComparison.OrdinalIgnoreCase))
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync(cancellationToken);

            if (normalizedFilter.DoctorId.HasValue && normalizedFilter.DoctorId.Value != currentDoctorId)
            {
                throw new ApiException(HttpStatusCode.Forbidden, "Doctors can only view their own appointments");
            }

            query = query.Where(x => x.DoctorId == currentDoctorId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Status))
        {
            query = query.Where(x => x.Status == normalizedFilter.Status);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("appointment_date", "asc") => query.OrderBy(x => x.AppointmentDate).ThenBy(x => x.StartTime),
            ("appointment_date", _) => query.OrderByDescending(x => x.AppointmentDate).ThenByDescending(x => x.StartTime),
            _ => query.OrderByDescending(x => x.AppointmentDate).ThenBy(x => x.StartTime)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var appointments = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = appointments.Select(x => x.ToResponse()).ToList()
        };
    }

    public async Task<AppointmentResponse?> UpdateAsync(long id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return null;
        }

        var oldValues = ToAuditModel(appointment);
        await ValidateAppointmentRequestAsync(appointment.PatientId, request.DoctorId, request.DepartmentId, request.AppointmentDate, request.StartTime, request.EndTime, id, cancellationToken);

        appointment.DoctorId = request.DoctorId;
        appointment.DepartmentId = request.DepartmentId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;
        appointment.Reason = request.Reason?.Trim();
        appointment.Remarks = request.Remarks?.Trim();

        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("UPDATE", nameof(Appointment), appointment.Id, oldValues, ToAuditModel(appointment), cancellationToken);
        return appointment.ToResponse();
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(long id, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return null;
        }

        await EnsureDoctorCanAccessAppointmentAsync(appointment, cancellationToken);

        var oldValues = ToAuditModel(appointment);
        var normalizedStatus = request.Status.Trim().ToUpperInvariant();
        if (!AppointmentStatuses.All.Contains(normalizedStatus))
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid appointment status");
        }

        if (normalizedStatus == AppointmentStatuses.Cancelled)
        {
            var hasActivePrescriptions = await prescriptionRepository.Query()
                .AnyAsync(x => x.AppointmentId == id && x.IsActive, cancellationToken);

            if (hasActivePrescriptions)
            {
                throw new ApiException(HttpStatusCode.Conflict, "Cancelled appointments cannot have active prescriptions");
            }
        }

        appointment.Status = normalizedStatus;
        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("STATUS_CHANGE", nameof(Appointment), appointment.Id, oldValues, ToAuditModel(appointment), cancellationToken);
        return appointment.ToResponse();
    }

    private async Task ValidateAppointmentRequestAsync(
        long patientId,
        long doctorId,
        long departmentId,
        DateOnly appointmentDate,
        TimeOnly startTime,
        TimeOnly endTime,
        long? existingAppointmentId,
        CancellationToken cancellationToken)
    {
        if (endTime <= startTime)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Appointment end time must be greater than start time");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (appointmentDate < today)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Appointment date cannot be in the past");
        }

        if (appointmentDate == today)
        {
            var nowTime = TimeOnly.FromDateTime(DateTime.UtcNow);
            if (startTime <= nowTime)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Appointment start time cannot be in the past");
            }
        }

        var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken);
        if (patient is null || !patient.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Patient must exist and be active");
        }

        var doctor = await doctorRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctor is null || !doctor.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Doctor must exist and be active");
        }

        var department = await departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null || !department.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Department must exist and be active");
        }

        if (doctor.DepartmentId != departmentId)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Doctor is not assigned to the selected department");
        }

        var dayName = appointmentDate.DayOfWeek.ToString().ToUpperInvariant();
        var withinAvailability = await doctorAvailabilityRepository.Query()
            .AnyAsync(x => x.DoctorId == doctorId &&
                           x.DayOfWeek == dayName &&
                           x.IsActive &&
                           x.StartTime <= startTime &&
                           x.EndTime >= endTime, cancellationToken);

        if (!withinAvailability)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Appointment must fit within doctor availability");
        }

        var overlaps = await appointmentRepository.Query()
            .AnyAsync(x => x.Id != existingAppointmentId &&
                           x.DoctorId == doctorId &&
                           x.AppointmentDate == appointmentDate &&
                           x.IsActive &&
                           x.Status == AppointmentStatuses.Scheduled &&
                           startTime < x.EndTime &&
                           endTime > x.StartTime, cancellationToken);

        if (overlaps)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Doctor is not available for the selected time slot");
        }
    }

    private static bool Overlaps(TimeOnly startA, TimeOnly endA, TimeOnly startB, TimeOnly endB) =>
        startA < endB && endA > startB;

    private static object ToAuditModel(Appointment appointment) => new
    {
        appointment.Id,
        appointment.PatientId,
        appointment.DoctorId,
        appointment.DepartmentId,
        appointment.AppointmentDate,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status,
        appointment.Reason,
        appointment.Remarks,
        appointment.IsActive
    };

    private static AppointmentListFilter NormalizeFilter(AppointmentListFilter filter)
    {
        var fromDate = filter.Date ?? filter.FromDate;
        var toDate = filter.Date ?? filter.ToDate;

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "'from_date' cannot be later than 'to_date'");
        }

        string? normalizedStatus = null;
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            normalizedStatus = filter.Status.Trim().ToUpperInvariant();
            if (!AppointmentStatuses.All.Contains(normalizedStatus))
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Invalid appointment status");
            }
        }

        return filter with
        {
            FromDate = fromDate,
            ToDate = toDate,
            Status = normalizedStatus
        };
    }

    private async Task<long> GetCurrentDoctorIdAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId
            ?? throw new ApiException(HttpStatusCode.Unauthorized, "Authenticated user context is missing");

        var doctorId = await doctorRepository.Query()
            .Where(x => x.UserId == currentUserId && x.IsActive)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (doctorId == 0)
        {
            throw new ApiException(HttpStatusCode.Forbidden, "Doctor profile not found for authenticated user");
        }

        return doctorId;
    }

    private async Task EnsureDoctorCanAccessAppointmentAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        if (!string.Equals(currentUserContext.Role, AppRoles.Doctor, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var currentDoctorId = await GetCurrentDoctorIdAsync(cancellationToken);
        if (appointment.DoctorId != currentDoctorId)
        {
            throw new ApiException(HttpStatusCode.Forbidden, "Doctors can only access their own appointments");
        }
    }
}
