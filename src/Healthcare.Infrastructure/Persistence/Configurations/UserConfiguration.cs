using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table => table.HasTrigger("trg_users_updated_at"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoleId).HasColumnName("role_id");
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        builder.Property(x => x.LastLoginAt).HasColumnName("last_login_at").HasColumnType("datetime2");

        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        builder.HasIndex(x => new { x.RoleId, x.IsActive }).HasDatabaseName("IX_users_role_id_is_active");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
