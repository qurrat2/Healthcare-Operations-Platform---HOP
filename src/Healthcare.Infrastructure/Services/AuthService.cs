using System.Net;
using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Application.Abstractions.Security;
using Healthcare.Application.Exceptions;
using Healthcare.Contracts.Auth;
using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Healthcare.Infrastructure.Auth;

namespace Healthcare.Infrastructure.Services;

internal sealed class AuthService(
    IUserRepository userRepository,
    ICurrentUserContext currentUserContext,
    IAuditWriter auditWriter,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUserContext.UserId
            ?? throw new ApiException(HttpStatusCode.Unauthorized, "Authenticated user context is missing");

        var user = await userRepository.Query()
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.DoctorProfile!)
                .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
            ?? throw new ApiException(HttpStatusCode.NotFound, "User not found");

        DepartmentSummary? department = null;
        if (user.DoctorProfile?.Department is not null)
        {
            department = new DepartmentSummary(user.DoctorProfile.Department.Id, user.DoctorProfile.Department.Name);
        }

        return new CurrentUserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role?.Name ?? string.Empty,
            department);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var user = await userRepository.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null || !user.IsActive || user.Role is null)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Invalid username or password");
        }

        var isValidPassword = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Invalid username or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            "LOGIN",
            nameof(User),
            user.Id,
            newValues: new
            {
                user.Username,
                user.LastLoginAt
            },
            cancellationToken: cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(user, user.Role.Name);
        var profile = new CurrentUserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role.Name,
            null);

        return new AuthResponse(
            token,
            "Bearer",
            _jwtOptions.ExpiryMinutes * 60,
            profile);
    }
}
