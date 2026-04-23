using Healthcare.Domain.Entities;

namespace Healthcare.Application.Abstractions.Persistence;

public interface IRoleRepository : IRepository<Role>;
public interface IUserRepository : IRepository<User>;
public interface IDepartmentRepository : IRepository<Department>;
public interface IDoctorRepository : IRepository<Doctor>;
public interface IDoctorAvailabilityRepository : IRepository<DoctorAvailability>;
public interface IPatientRepository : IRepository<Patient>;
public interface IPatientDependentRepository : IRepository<PatientDependent>;
public interface IAppointmentRepository : IRepository<Appointment>;
public interface IPrescriptionRepository : IRepository<Prescription>;
public interface IPrescriptionItemRepository : IRepository<PrescriptionItem>;
public interface IAuditLogRepository : IRepository<AuditLog>;
