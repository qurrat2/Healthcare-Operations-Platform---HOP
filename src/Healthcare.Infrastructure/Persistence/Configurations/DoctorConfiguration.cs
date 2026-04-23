using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctors", table => table.HasTrigger("trg_doctors_updated_at"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.DepartmentId).HasColumnName("department_id");
        builder.Property(x => x.LicenseNumber).HasColumnName("license_number").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Specialization).HasColumnName("specialization").HasMaxLength(150);
        builder.Property(x => x.ConsultationFee).HasColumnName("consultation_fee").HasColumnType("decimal(10,2)");

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.HasIndex(x => new { x.DepartmentId, x.IsActive }).HasDatabaseName("IX_doctors_department_id_is_active");

        builder.HasOne(x => x.User)
            .WithOne(x => x.DoctorProfile)
            .HasForeignKey<Doctor>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Doctors)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
