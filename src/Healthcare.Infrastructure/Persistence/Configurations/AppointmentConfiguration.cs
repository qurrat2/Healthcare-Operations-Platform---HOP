using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments", table =>
        {
            table.HasTrigger("trg_appointments_updated_at");
            table.HasCheckConstraint("CK_appointments_time", "[end_time] > [start_time]");
            table.HasCheckConstraint("CK_appointments_status", "[status] IN ('SCHEDULED','COMPLETED','CANCELLED','NO_SHOW')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.DepartmentId).HasColumnName("department_id");
        builder.Property(x => x.AppointmentDate).HasColumnName("appointment_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired().HasDefaultValue("SCHEDULED");
        builder.Property(x => x.Reason).HasColumnName("reason");
        builder.Property(x => x.Remarks).HasColumnName("remarks");

        builder.HasIndex(x => new { x.PatientId, x.AppointmentDate }).HasDatabaseName("IX_appointments_patient_date");
        builder.HasIndex(x => new { x.DoctorId, x.AppointmentDate, x.Status }).HasDatabaseName("IX_appointments_doctor_date_status");
        builder.HasIndex(x => new { x.DepartmentId, x.AppointmentDate, x.Status }).HasDatabaseName("IX_appointments_department_date_status");
        builder.HasIndex(x => new { x.DoctorId, x.AppointmentDate, x.StartTime })
            .HasDatabaseName("UX_appointments_doctor_date_start_active")
            .IsUnique()
            .HasFilter("[is_active] = 1 AND [status] = 'SCHEDULED'");

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
