using Healthcare.Application.Abstractions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Application.Abstractions.Security;
using Healthcare.Infrastructure.Auth;
using Healthcare.Infrastructure.Persistence;
using Healthcare.Infrastructure.Persistence.Repositories;
using Healthcare.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Healthcare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddDbContext<HealthcareDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("HealthcareDb"),
                sql => sql.MigrationsAssembly(typeof(HealthcareDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IPatientDependentRepository, PatientDependentRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReferenceDataService, ReferenceDataService>();

        return services;
    }
}
