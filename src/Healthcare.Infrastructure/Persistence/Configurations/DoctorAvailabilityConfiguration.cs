using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("doctor_availability", table =>
        {
            table.HasTrigger("trg_doctor_availability_updated_at");
            table.HasCheckConstraint("CK_doctor_availability_day", "[day_of_week] IN ('MONDAY','TUESDAY','WEDNESDAY','THURSDAY','FRIDAY','SATURDAY','SUNDAY')");
            table.HasCheckConstraint("CK_doctor_availability_time", "[end_time] > [start_time]");
            table.HasCheckConstraint("CK_doctor_availability_slot", "[slot_duration_minutes] > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.DayOfWeek).HasColumnName("day_of_week").HasMaxLength(20).IsRequired();
        builder.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.SlotDurationMinutes).HasColumnName("slot_duration_minutes").IsRequired();

        builder.HasIndex(x => new { x.DoctorId, x.DayOfWeek, x.IsActive })
            .HasDatabaseName("IX_doctor_availability_doctor_day_active");

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditable();
    }
}
