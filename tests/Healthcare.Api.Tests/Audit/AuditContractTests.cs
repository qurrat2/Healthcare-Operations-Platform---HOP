using Healthcare.Api.Tests.Common;

namespace Healthcare.Api.Tests.Audit;

public sealed class AuditContractTests
{
    [Fact(DisplayName = "Audit log response exposes core tracking fields")]
    [Trait("Category", TestCategories.Audit)]
    public void Audit_log_response_contract_is_available()
    {
        var responseType = typeof(Healthcare.Contracts.Audit.AuditLogResponse);
        var propertyNames = responseType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("UserId", propertyNames);
        Assert.Contains("Action", propertyNames);
        Assert.Contains("EntityType", propertyNames);
        Assert.Contains("EntityId", propertyNames);
        Assert.Contains("OldValues", propertyNames);
        Assert.Contains("NewValues", propertyNames);
        Assert.Contains("IpAddress", propertyNames);
        Assert.Contains("CreatedAt", propertyNames);
    }
}
