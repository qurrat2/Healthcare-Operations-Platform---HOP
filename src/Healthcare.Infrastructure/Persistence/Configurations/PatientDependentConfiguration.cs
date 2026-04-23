using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class PatientDependentConfiguration : IEntityTypeConfiguration<PatientDependent>
{
    public void Configure(EntityTypeBuilder<PatientDependent> builder)
    {
        builder.ToTable("patient_dependents", table =>
        {
            table.HasTrigger("trg_patient_dependents_updated_at");
            table.HasCheckConstraint("CK_patient_dependents_not_same", "[primary_patient_id] <> [dependent_patient_id]");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrimaryPatientId).HasColumnName("primary_patient_id");
        builder.Property(x => x.DependentPatientId).HasColumnName("dependent_patient_id");
        builder.Property(x => x.Relationship).HasColumnName("relationship").HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.PrimaryPatientId, x.DependentPatientId }).IsUnique();

        builder.HasOne(x => x.PrimaryPatient)
            .WithMany(x => x.PrimaryLinks)
            .HasForeignKey(x => x.PrimaryPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DependentPatient)
            .WithMany(x => x.Dependents)
            .HasForeignKey(x => x.DependentPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
