using Healthcare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Infrastructure.Persistence.Configurations;

internal sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items", table =>
        {
            table.HasTrigger("trg_prescription_items_updated_at");
            table.HasCheckConstraint("CK_prescription_items_duration", "[duration_days] IS NULL OR [duration_days] > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrescriptionId).HasColumnName("prescription_id");
        builder.Property(x => x.MedicineName).HasColumnName("medicine_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Dosage).HasColumnName("dosage").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Frequency).HasColumnName("frequency").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DurationDays).HasColumnName("duration_days");
        builder.Property(x => x.Instructions).HasColumnName("instructions");

        builder.HasOne(x => x.Prescription)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditable();
    }
}
