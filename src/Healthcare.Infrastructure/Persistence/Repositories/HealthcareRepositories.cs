using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Domain.Entities;

namespace Healthcare.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(HealthcareDbContext dbContext) : Repository<Role>(dbContext), IRoleRepository;
internal sealed class UserRepository(HealthcareDbContext dbContext) : Repository<User>(dbContext), IUserRepository;
internal sealed class DepartmentRepository(HealthcareDbContext dbContext) : Repository<Department>(dbContext), IDepartmentRepository;
internal sealed class DoctorRepository(HealthcareDbContext dbContext) : Repository<Doctor>(dbContext), IDoctorRepository;
internal sealed class DoctorAvailabilityRepository(HealthcareDbContext dbContext) : Repository<DoctorAvailability>(dbContext), IDoctorAvailabilityRepository;
internal sealed class PatientRepository(HealthcareDbContext dbContext) : Repository<Patient>(dbContext), IPatientRepository;
internal sealed class PatientDependentRepository(HealthcareDbContext dbContext) : Repository<PatientDependent>(dbContext), IPatientDependentRepository;
internal sealed class AppointmentRepository(HealthcareDbContext dbContext) : Repository<Appointment>(dbContext), IAppointmentRepository;
internal sealed class PrescriptionRepository(HealthcareDbContext dbContext) : Repository<Prescription>(dbContext), IPrescriptionRepository;
internal sealed class PrescriptionItemRepository(HealthcareDbContext dbContext) : Repository<PrescriptionItem>(dbContext), IPrescriptionItemRepository;
internal sealed class AuditLogRepository(HealthcareDbContext dbContext) : Repository<AuditLog>(dbContext), IAuditLogRepository;
