using Healthcare.Application.Abstractions.Persistence;

namespace Healthcare.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(HealthcareDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
