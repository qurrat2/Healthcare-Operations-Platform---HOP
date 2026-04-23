using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Application.Exceptions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Doctors;
using Healthcare.Domain.Constants;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class DoctorService(
    IDoctorRepository doctorRepository,
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository,
    IDoctorAvailabilityRepository doctorAvailabilityRepository,
    IUnitOfWork unitOfWork) : IDoctorService
{
    public async Task<DoctorAvailabilityResponse> AddAvailabilityAsync(long doctorId, UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureDoctorExistsAsync(doctorId, cancellationToken);
        ValidateAvailability(request);

        var availability = new DoctorAvailability
        {
            DoctorId = doctorId,
            DayOfWeek = request.DayOfWeek.Trim().ToUpperInvariant(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes
        };

        await doctorAvailabilityRepository.AddAsync(availability, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return availability.ToResponse();
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.NotFound, "Doctor user not found");
        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.NotFound, "Department not found");

        if (!department.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Department must be active");
        }

        if (!string.Equals(user.Role?.Name, AppRoles.Doctor, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Selected user must have the DOCTOR role");
        }

        var duplicateUser = await doctorRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.UserId == request.UserId, cancellationToken);
        if (duplicateUser)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Doctor profile already exists for this user");
        }

        var duplicateLicense = await doctorRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.LicenseNumber == request.LicenseNumber.Trim(), cancellationToken);
        if (duplicateLicense)
        {
            throw new ApiException(HttpStatusCode.Conflict, "License number already exists");
        }

        var doctor = new Doctor
        {
            UserId = request.UserId,
            DepartmentId = request.DepartmentId,
            LicenseNumber = request.LicenseNumber.Trim(),
            Specialization = request.Specialization?.Trim(),
            ConsultationFee = request.ConsultationFee
        };

        await doctorRepository.AddAsync(doctor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        doctor.User = user;
        return doctor.ToResponse();
    }

    public async Task<DoctorResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var doctor = await doctorRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return doctor?.ToResponse();
    }

    public async Task<IReadOnlyCollection<DoctorAvailabilityResponse>> ListAvailabilityAsync(long doctorId, CancellationToken cancellationToken = default)
    {
        var availability = await doctorAvailabilityRepository.Query()
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.IsActive)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        return availability.Select(x => x.ToResponse()).ToList();
    }

    public async Task<PagedResult<DoctorResponse>> ListAsync(long? departmentId, bool? isActive, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);

        var query = doctorRepository.Query()
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.IgnoreQueryFilters().Where(x => x.IsActive == isActive.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == departmentId.Value);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("created_at", "asc") => query.OrderBy(x => x.CreatedAt),
            ("created_at", _) => query.OrderByDescending(x => x.CreatedAt),
            ("full_name", "desc") => query.OrderByDescending(x => x.User!.FullName),
            _ => query.OrderBy(x => x.User!.FullName)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var doctors = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<DoctorResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = doctors.Select(x => x.ToResponse()).ToList()
        };
    }

    public async Task<bool> RemoveAvailabilityAsync(long doctorId, long availabilityId, CancellationToken cancellationToken = default)
    {
        var availability = await doctorAvailabilityRepository.Query()
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.Id == availabilityId && x.IsActive, cancellationToken);

        if (availability is null)
        {
            return false;
        }

        availability.IsActive = false;
        doctorAvailabilityRepository.Update(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DoctorResponse?> UpdateAsync(long id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var doctor = await doctorRepository.Query()
            .IgnoreQueryFilters()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (doctor is null)
        {
            return null;
        }

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.NotFound, "Department not found");

        if (!department.IsActive)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Department must be active");
        }

        doctor.DepartmentId = request.DepartmentId;
        doctor.Specialization = request.Specialization?.Trim();
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.IsActive = request.IsActive;

        doctorRepository.Update(doctor);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return doctor.ToResponse();
    }

    public async Task<DoctorAvailabilityResponse?> UpdateAvailabilityAsync(long doctorId, long availabilityId, UpsertDoctorAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var availability = await doctorAvailabilityRepository.Query()
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.Id == availabilityId, cancellationToken);

        if (availability is null)
        {
            return null;
        }

        ValidateAvailability(request);

        availability.DayOfWeek = request.DayOfWeek.Trim().ToUpperInvariant();
        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;
        availability.SlotDurationMinutes = request.SlotDurationMinutes;

        doctorAvailabilityRepository.Update(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return availability.ToResponse();
    }

    private async Task EnsureDoctorExistsAsync(long doctorId, CancellationToken cancellationToken)
    {
        var exists = await doctorRepository.Query().AnyAsync(x => x.Id == doctorId, cancellationToken);
        if (!exists)
        {
            throw new ApiException(HttpStatusCode.NotFound, "Doctor not found");
        }
    }

    private static void ValidateAvailability(UpsertDoctorAvailabilityRequest request)
    {
        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out _))
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid day_of_week");
        }

        if (request.EndTime <= request.StartTime)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "Availability end time must be greater than start time");
        }

        if (request.SlotDurationMinutes <= 0)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "slot_duration_minutes must be greater than zero");
        }
    }
}
