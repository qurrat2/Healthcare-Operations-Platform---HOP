using Healthcare.Contracts.Appointments;
using Healthcare.Contracts.Patients;
using Healthcare.Contracts.Prescriptions;

namespace Healthcare.Contracts.History;

public sealed record PatientHistoryResponse(
    PatientResponse Patient,
    IReadOnlyCollection<AppointmentResponse> Appointments,
    IReadOnlyCollection<PrescriptionResponse> Prescriptions,
    IReadOnlyCollection<DependentResponse> Dependents);
