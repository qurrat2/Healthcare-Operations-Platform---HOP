using Healthcare.Application.Abstractions;
using Healthcare.Domain.Common;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Persistence;

public sealed class HealthcareDbContext : DbContext
{
    private readonly ICurrentUserContext? _currentUserContext;

    public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options)
        : base(options)
    {
    }

    public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options, ICurrentUserContext currentUserContext)
        : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientDependent> PatientDependents => Set<PatientDependent>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorAvailabilityConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new PatientDependentConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionItemConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;
        var currentUserId = _currentUserContext?.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.UpdatedBy = currentUserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Property(x => x.CreatedBy).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }
}
