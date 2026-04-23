using Healthcare.Api.Tests.Common;

namespace Healthcare.Api.Tests.Patients;

public sealed class PatientContractTests
{
    [Fact(DisplayName = "Create patient request exposes core demographic fields")]
    [Trait("Category", TestCategories.Patients)]
    public void Create_patient_request_contract_is_available()
    {
        var requestType = typeof(Healthcare.Contracts.Patients.CreatePatientRequest);
        var propertyNames = requestType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("Mrn", propertyNames);
        Assert.Contains("FirstName", propertyNames);
        Assert.Contains("LastName", propertyNames);
        Assert.Contains("DateOfBirth", propertyNames);
        Assert.Contains("Gender", propertyNames);
    }

    [Fact(DisplayName = "Patient list filter exposes broader search fields")]
    [Trait("Category", TestCategories.Patients)]
    public void Patient_list_filter_contract_is_available()
    {
        var filterType = typeof(Healthcare.Contracts.Patients.PatientListFilter);
        var propertyNames = filterType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("Search", propertyNames);
        Assert.Contains("Phone", propertyNames);
        Assert.Contains("Mrn", propertyNames);
        Assert.Contains("Email", propertyNames);
        Assert.Contains("IsActive", propertyNames);
    }
}
