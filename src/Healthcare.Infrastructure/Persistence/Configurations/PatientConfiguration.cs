using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients", table =>
        {
            table.HasTrigger("trg_patients_updated_at");
            table.HasCheckConstraint("CK_patients_gender", "[gender] IN ('MALE','FEMALE','OTHER')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Mrn).HasColumnName("mrn").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date").IsRequired();
        builder.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Address).HasColumnName("address");
        builder.Property(x => x.BloodGroup).HasColumnName("blood_group").HasMaxLength(10);
        builder.Property(x => x.EmergencyContactName).HasColumnName("emergency_contact_name").HasMaxLength(150);
        builder.Property(x => x.EmergencyContactPhone).HasColumnName("emergency_contact_phone").HasMaxLength(30);

        builder.HasIndex(x => x.Mrn).IsUnique();
        builder.HasIndex(x => x.Mrn).HasDatabaseName("IX_patients_mrn");
        builder.HasIndex(x => x.Phone).HasDatabaseName("IX_patients_phone");
        builder.HasIndex(x => new { x.LastName, x.FirstName }).HasDatabaseName("IX_patients_last_first_name");

        builder.ConfigureAuditable();
    }
}
