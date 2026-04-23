using Healthcare.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static void ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
        => ConfigureAuditable(builder, includeUserAuditColumns: true);

    public static void ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> builder, bool includeUserAuditColumns)
        where TEntity : AuditableEntity
    {
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasQueryFilter(x => x.IsActive);

        if (includeUserAuditColumns)
        {
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        }
    }

    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.Property(x => x.Id).HasColumnName("id");
    }
}
