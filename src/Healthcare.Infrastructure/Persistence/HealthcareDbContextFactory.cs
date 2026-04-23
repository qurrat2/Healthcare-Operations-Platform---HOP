using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Healthcare.Infrastructure.Persistence;

public sealed class HealthcareDbContextFactory : IDesignTimeDbContextFactory<HealthcareDbContext>
{
    public HealthcareDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var apiPath = ResolveApiPath(basePath);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HealthcareDbContext>();
        var connectionString = configuration.GetConnectionString("HealthcareDb");

        optionsBuilder.UseSqlServer(connectionString);

        return new HealthcareDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiPath(string basePath)
    {
        var candidates = new[]
        {
            Path.Combine(basePath, "src", "Healthcare.Api"),
            Path.Combine(basePath, "..", "Healthcare.Api"),
            Path.Combine(basePath, "..", "..", "Healthcare.Api"),
            Path.Combine(basePath, "..", "..", "..", "src", "Healthcare.Api")
        };

        var apiPath = candidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(path => File.Exists(Path.Combine(path, "appsettings.json")));

        if (apiPath is null)
        {
            throw new InvalidOperationException("Could not locate src/Healthcare.Api for design-time configuration.");
        }

        return apiPath;
    }
}
