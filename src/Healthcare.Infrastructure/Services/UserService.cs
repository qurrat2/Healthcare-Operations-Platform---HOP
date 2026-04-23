using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Application.Abstractions.Security;
using Healthcare.Application.Exceptions;
using Healthcare.Contracts.Common;
using Healthcare.Contracts.Users;
using Healthcare.Domain.Constants;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Services.ServiceHelpers;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Services;

internal sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var exists = await userRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Username == username, cancellationToken);

        if (exists)
        {
            throw new ApiException(HttpStatusCode.Conflict, "Username already exists");
        }

        var normalizedRole = request.Role.Trim().ToUpperInvariant();
        var role = await roleRepository.Query()
            .FirstOrDefaultAsync(x => x.Name == normalizedRole && x.IsActive, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.BadRequest, $"Invalid role: {request.Role}");

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            RoleId = role.Id
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("CREATE", nameof(User), user.Id, newValues: ToAuditModel(user, role.Name), cancellationToken: cancellationToken);

        return user.ToResponse(role.Name);
    }

    public async Task<PagedResult<UserResponse>> ListAsync(string? role, bool? isActive, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = PaginationHelper.NormalizePage(pagination.Page);
        var limit = PaginationHelper.NormalizeLimit(pagination.Limit);

        var query = userRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.Trim().ToUpperInvariant();
            query = query.Where(x => x.Role != null && x.Role.Name == normalizedRole);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        query = (pagination.SortBy?.ToLowerInvariant(), pagination.SortOrder?.ToLowerInvariant()) switch
        {
            ("username", "desc") => query.OrderByDescending(x => x.Username),
            ("username", _) => query.OrderBy(x => x.Username),
            ("created_at", "asc") => query.OrderBy(x => x.CreatedAt),
            ("created_at", _) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Username)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserResponse>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            Items = users.Select(x => x.ToResponse(x.Role?.Name ?? string.Empty)).ToList()
        };
    }

    public async Task<UserResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user?.ToResponse(user.Role?.Name ?? string.Empty);
    }

    public async Task<UserResponse?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query()
            .IgnoreQueryFilters()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var oldValues = ToAuditModel(user, user.Role?.Name ?? string.Empty);

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim();
        user.Phone = request.Phone?.Trim();
        user.IsActive = request.IsActive;

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("UPDATE", nameof(User), user.Id, oldValues, ToAuditModel(user, user.Role?.Name ?? string.Empty), cancellationToken);

        return user.ToResponse(user.Role?.Name ?? string.Empty);
    }

    public async Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query()
            .IgnoreQueryFilters()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var oldValues = ToAuditModel(user, user.Role?.Name ?? string.Empty);
        user.IsActive = false;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("DELETE", nameof(User), user.Id, oldValues, ToAuditModel(user, user.Role?.Name ?? string.Empty), cancellationToken);
        return true;
    }

    private static object ToAuditModel(User user, string roleName) => new
    {
        user.Id,
        user.Username,
        user.FullName,
        Role = roleName,
        user.Email,
        user.Phone,
        user.IsActive
    };
}
