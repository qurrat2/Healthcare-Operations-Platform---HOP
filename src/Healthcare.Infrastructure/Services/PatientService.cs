using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.History;
using Healthcare.Contracts.Patients;
using Healthcare.Contracts.Prescriptions;
using Healthcare.Application.Exceptions;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Healthcare.Infrastructure.Services;

internal sealed class PatientService(
    IPatientRepository patientRepository,
    IPatientDependentRepository patientDependentRepository,
    IPrescriptionRepository prescriptionRepository,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork,
    HealthcareDbContext dbContext) : IPatientService
{
    public async Task<DependentResponse> AddDependentAsync(long patientId, AddDependentRequest request, CancellationToken cancellationToken = default)
    {
        if (patientId == request.DependentPatientId)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "A patient cannot be their own dependent");
        }

        var primaryPatient = await patientRepository.GetByIdAsync(patientId, cancellationToken);
        var dependentPatient = await patientRepository.GetByIdAsync(request.DependentPatientId, cancellationToken);

        if (primaryPatient is null || dependentPatient is null)
        {
            throw new ApiException(HttpStatusCode.NotFound, "One or both patients were not found");
        }

        var exists = await patientDependentRepository.Query()
            .AnyAsync(x => x.PrimaryPatientId == patientId && x.DependentPatientId == request.DependentPatientId, cancellationToken);

        if (exists)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Dependent link already exists");
        }

        var dependent = new PatientDependent
        {
            PrimaryPatientId = patientId,
            DependentPatientId = request.DependentPatientId,
            Relationship = request.Relationship.Trim()
        };

        await patientDependentRepository.AddAsync(dependent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return dependent.ToResponse();
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await patientRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Mrn == request.Mrn.Trim(), cancellationToken);

        if (exists)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Patient MRN already exists");
        }

        var patient = new Patient
        {
            Mrn = request.Mrn.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender.Trim().ToUpperInvariant(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            BloodGroup = request.BloodGroup?.Trim()
        };

        await patientRepository.AddAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("CREATE", nameof(Patient), patient.Id, newValues: ToAuditModel(patient), cancellationToken: cancellationToken);

        return patient.ToResponse();
    }

    public async Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        var oldValues = ToAuditModel(patient);
        patient.IsActive = false;
        patientRepository.Update(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("DELETE", nameof(Patient), patient.Id, oldValues, ToAuditModel(patient), cancellationToken);
        return true;
    }

    public async Task<PatientResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return patient?.ToResponse();
    }

    public async Task<PatientHistoryResponse?> GetHistoryAsync(long patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == patientId, cancellationToken);

        if (patient is null)
        {
            return null;
        }

        var appointments = await dbContext.Appointments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.StartTime)
            .ToListAsync(cancellationToken);

        var prescriptions = await dbContext.Prescriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.IssuedAt)
            .ToListAsync(cancellationToken);

        var dependents = await dbContext.PatientDependents
            .AsNoTracking()
            .Where(x => x.PrimaryPatientId == patientId)
            .ToListAsync(cancellationToken);

        return new PatientHistoryResponse(
            patient.ToResponse(),
            appointments.Select(x => x.ToResponse()).ToList(),
            prescriptions.Select(x => x.ToResponse()).ToList(),
            dependents.Select(x => x.ToResponse()).ToList());
    }

    public async Task<IReadOnlyCollection<DependentResponse>> ListDependentsAsync(long patientId, CancellationToken cancellationToken = default)
    {
        var dependents = await patientDependentRepository.Query()
            .AsNoTracking()
            .Where(x => x.PrimaryPatientId == patientId && x.IsActive)
            .ToListAsync(cancellationToken);

        return dependents.Select(x => x.ToResponse()).ToList();
    }

    public async Task<PagedResult<PatientResponse>> ListAsync(PatientListFilter filter, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);
        var normalizedFilter = NormalizeFilter(filter);

        var query = patientRepository.Query().AsNoTracking();

        // When the caller explicitly asks for inactive records, bypass the soft-delete filter.
        if (normalizedFilter.IsActive.HasValue)
        {
            query = query.IgnoreQueryFilters();
        }

        if (normalizedFilter.SearchTerms.Count > 0)
        {
            foreach (var term in normalizedFilter.SearchTerms)
            {
                var phoneDigitsTerm = ExtractDigits(term);
                query = query.Where(x =>
                    x.FirstName.Contains(term) ||
                    x.LastName.Contains(term) ||
                    (x.FirstName + " " + x.LastName).Contains(term) ||
                    x.Mrn.Contains(term) ||
                    (x.Email != null && x.Email.Contains(term)) ||
                    (x.Phone != null && x.Phone.Contains(term)) ||
                    (phoneDigitsTerm.Length > 0 && x.Phone != null &&
                     x.Phone.Replace(" ", string.Empty)
                         .Replace("-", string.Empty)
                         .Replace("(", string.Empty)
                         .Replace(")", string.Empty)
                         .Replace("+", string.Empty)
                         .Contains(phoneDigitsTerm)));
            }
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Phone))
        {
            query = query.Where(x => x.Phone != null && x.Phone.Contains(normalizedFilter.Phone));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.PhoneDigits))
        {
            query = query.Where(x => x.Phone != null &&
                x.Phone.Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Replace("+", string.Empty)
                    .Contains(normalizedFilter.PhoneDigits));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Mrn))
        {
            query = query.Where(x => x.Mrn.Contains(normalizedFilter.Mrn));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Email))
        {
            query = query.Where(x => x.Email != null && x.Email.Contains(normalizedFilter.Email));
        }

        if (normalizedFilter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == normalizedFilter.IsActive.Value);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("created_at", "asc") => query.OrderBy(x => x.CreatedAt),
            ("created_at", _) => query.OrderByDescending(x => x.CreatedAt),
            ("first_name", "desc") => query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName),
            _ => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var patients = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<PatientResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = patients.Select(x => x.ToResponse()).ToList()
        };
    }

    private static NormalizedPatientListFilter NormalizeFilter(PatientListFilter filter)
    {
        var normalizedSearch = filter.Search?.Trim();
        var searchTerms = string.IsNullOrWhiteSpace(normalizedSearch)
            ? []
            : normalizedSearch
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var normalizedPhone = filter.Phone?.Trim();
        var normalizedMrn = filter.Mrn?.Trim();
        var normalizedEmail = filter.Email?.Trim();
        var phoneDigits = string.IsNullOrWhiteSpace(normalizedPhone) ? null : ExtractDigits(normalizedPhone);

        return new NormalizedPatientListFilter(
            searchTerms,
            normalizedPhone,
            phoneDigits,
            normalizedMrn,
            normalizedEmail,
            filter.IsActive);
    }

    private static string ExtractDigits(string value) =>
        Regex.Replace(value, "[^0-9]", string.Empty);

    private sealed record NormalizedPatientListFilter(
        IReadOnlyCollection<string> SearchTerms,
        string? Phone,
        string? PhoneDigits,
        string? Mrn,
        string? Email,
        bool? IsActive);

    public async Task<IReadOnlyCollection<PrescriptionResponse>> GetPrescriptionsAsync(long patientId, CancellationToken cancellationToken = default)
    {
        var prescriptions = await prescriptionRepository.Query()
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.PatientId == patientId && x.IsActive)
            .OrderByDescending(x => x.IssuedAt)
            .ToListAsync(cancellationToken);

        return prescriptions.Select(x => x.ToResponse()).ToList();
    }

    public async Task<bool> RemoveDependentAsync(long patientId, long dependentId, CancellationToken cancellationToken = default)
    {
        var dependent = await patientDependentRepository.Query()
            .FirstOrDefaultAsync(x => x.PrimaryPatientId == patientId && x.DependentPatientId == dependentId && x.IsActive, cancellationToken);

        if (dependent is null)
        {
            return false;
        }

        dependent.IsActive = false;
        patientDependentRepository.Update(dependent);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PatientResponse?> UpdateAsync(long id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await patientRepository.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        var oldValues = ToAuditModel(patient);
        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender.Trim().ToUpperInvariant();
        patient.Phone = request.Phone?.Trim();
        patient.Email = request.Email?.Trim();
        patient.Address = request.Address?.Trim();
        patient.BloodGroup = request.BloodGroup?.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        patient.IsActive = request.IsActive;

        patientRepository.Update(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("UPDATE", nameof(Patient), patient.Id, oldValues, ToAuditModel(patient), cancellationToken);

        return patient.ToResponse();
    }

    private static object ToAuditModel(Patient patient) => new
    {
        patient.Id,
        patient.Mrn,
        patient.FirstName,
        patient.LastName,
        patient.DateOfBirth,
        patient.Gender,
        patient.Phone,
        patient.Email,
        patient.Address,
        patient.BloodGroup,
        patient.EmergencyContactName,
        patient.EmergencyContactPhone,
        patient.IsActive
    };
}
