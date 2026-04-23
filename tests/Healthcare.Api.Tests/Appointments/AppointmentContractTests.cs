using Healthcare.Api.Tests.Common;

namespace Healthcare.Api.Tests.Appointments;

public sealed class AppointmentContractTests
{
    [Fact(DisplayName = "Create appointment request exposes scheduling fields")]
    [Trait("Category", TestCategories.Appointments)]
    public void Create_appointment_request_contract_is_available()
    {
        var requestType = typeof(Healthcare.Contracts.Appointments.CreateAppointmentRequest);
        var propertyNames = requestType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("PatientId", propertyNames);
        Assert.Contains("DoctorId", propertyNames);
        Assert.Contains("DepartmentId", propertyNames);
        Assert.Contains("AppointmentDate", propertyNames);
        Assert.Contains("StartTime", propertyNames);
        Assert.Contains("EndTime", propertyNames);
    }

    [Fact(DisplayName = "Appointment list filter exposes scheduling and ownership filters")]
    [Trait("Category", TestCategories.Appointments)]
    public void Appointment_list_filter_contract_is_available()
    {
        var filterType = typeof(Healthcare.Contracts.Appointments.AppointmentListFilter);
        var propertyNames = filterType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("Date", propertyNames);
        Assert.Contains("FromDate", propertyNames);
        Assert.Contains("ToDate", propertyNames);
        Assert.Contains("PatientId", propertyNames);
        Assert.Contains("DoctorId", propertyNames);
        Assert.Contains("DepartmentId", propertyNames);
        Assert.Contains("Status", propertyNames);
    }
}
