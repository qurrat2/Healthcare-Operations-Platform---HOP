using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions", table => table.HasTrigger("trg_prescriptions_updated_at"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.AppointmentId).HasColumnName("appointment_id");
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.Diagnosis).HasColumnName("diagnosis");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.PatientId, x.IssuedAt }).HasDatabaseName("IX_prescriptions_patient_issued_at");

        builder.HasOne(x => x.Appointment)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
