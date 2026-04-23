using System.Linq.Expressions;
using Healthcare.Application.Abstractions.Persistence;
using Healthcare.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Infrastructure.Persistence.Repositories;

internal class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly HealthcareDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(HealthcareDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        DbSet.AddAsync(entity, cancellationToken).AsTask();

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public IQueryable<TEntity> Query() => DbSet.AsQueryable();

    public void Update(TEntity entity) => DbSet.Update(entity);
}
