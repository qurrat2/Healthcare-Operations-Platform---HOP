using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Departments;
using Healthcare.Application.Exceptions;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class DepartmentService(
    IDepartmentRepository departmentRepository,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IDepartmentService
{
    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        var exists = await departmentRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name == normalizedName, cancellationToken);

        if (exists)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Department already exists");
        }

        var department = new Department
        {
            Name = normalizedName,
            Description = request.Description?.Trim()
        };

        await departmentRepository.AddAsync(department, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("CREATE", nameof(Department), department.Id, newValues: ToAuditModel(department), cancellationToken: cancellationToken);

        return department.ToResponse();
    }

    public async Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (department is null)
        {
            return false;
        }

        var oldValues = ToAuditModel(department);
        department.IsActive = false;
        departmentRepository.Update(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            "DELETE",
            nameof(Department),
            department.Id,
            oldValues: oldValues,
            newValues: ToAuditModel(department),
            cancellationToken: cancellationToken);
        return true;
    }

    public async Task<DepartmentResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return department?.ToResponse();
    }

    public async Task<PagedResult<DepartmentResponse>> ListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);

        var query = departmentRepository.Query().AsNoTracking();
        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("created_at", "asc") => query.OrderBy(x => x.CreatedAt),
            ("created_at", _) => query.OrderByDescending(x => x.CreatedAt),
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = entities.Select(x => x.ToResponse()).ToList()
        };
    }

    public async Task<DepartmentResponse?> UpdateAsync(long id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (department is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();
        var duplicateExists = await departmentRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.Name == normalizedName, cancellationToken);

        if (duplicateExists)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Another department already uses this name");
        }

        var oldValues = ToAuditModel(department);
        department.Name = normalizedName;
        department.Description = request.Description?.Trim();
        department.IsActive = request.IsActive;

        departmentRepository.Update(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("UPDATE", nameof(Department), department.Id, oldValues, ToAuditModel(department), cancellationToken);

        return department.ToResponse();
    }

    private static object ToAuditModel(Department department) => new
    {
        department.Id,
        department.Name,
        department.Description,
        department.IsActive
    };
}
