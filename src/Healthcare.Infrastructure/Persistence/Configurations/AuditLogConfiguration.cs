using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs", table =>
        {
            table.HasCheckConstraint("CK_audit_logs_action", "[action] IN ('CREATE','UPDATE','DELETE','LOGIN','STATUS_CHANGE')");
        });

        builder.HasKey(x => x.Id);
        builder.ConfigureBaseEntity();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.OldValues).HasColumnName("old_values");
        builder.Property(x => x.NewValues).HasColumnName("new_values");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasColumnName("user_agent");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("IX_audit_logs_entity");
        builder.HasIndex(x => new { x.UserId, x.CreatedAt }).HasDatabaseName("IX_audit_logs_user_created_at");

    }
}
