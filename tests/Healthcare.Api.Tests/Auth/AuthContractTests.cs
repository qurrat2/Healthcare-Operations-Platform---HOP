using Healthcare.Api.Tests.Common;

namespace Healthcare.Api.Tests.Auth;

public sealed class AuthContractTests
{
    [Fact(DisplayName = "Login request exposes username and password properties")]
    [Trait("Category", TestCategories.Auth)]
    public void Login_request_contract_is_available()
    {
        var requestType = typeof(Healthcare.Contracts.Auth.LoginRequest);
        var propertyNames = requestType.GetProperties().Select(x => x.Name).ToArray();

        Assert.Contains("Username", propertyNames);
        Assert.Contains("Password", propertyNames);
    }
}
