using Healthcare.Api.Tests.Common;

namespace Healthcare.Api.Tests.Common;

public sealed class TestingStrategyTests
{
    [Fact]
    public void Test_project_references_the_api_project()
    {
        var apiAssembly = typeof(Program).Assembly;

        Assert.Equal("Healthcare.Api", apiAssembly.GetName().Name);
    }
}
