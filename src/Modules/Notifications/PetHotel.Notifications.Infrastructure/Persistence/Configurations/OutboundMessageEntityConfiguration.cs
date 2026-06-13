using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Notifications.Domain.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="OutboundMessage"/> (Fluent API, docs/04).</summary>
public sealed class OutboundMessageEntityConfiguration : IEntityTypeConfiguration<OutboundMessage>
{
    public void Configure(EntityTypeBuilder<OutboundMessage> builder)
    {
        builder.ToTable("outbound_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => new OutboundMessageId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(m => m.Tutor)
            .HasConversion(t => t.Value, value => new TutorReference(value))
            .HasColumnName("tutor_id")
            .IsRequired();

        builder.Property(m => m.Pet)
            .HasConversion(p => p.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(m => m.ReservationId).HasColumnName("reservation_id").IsRequired();
        builder.Property(m => m.ReportDate).IsRequired();
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(10000).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.SentAt);

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(m => new { m.TenantId, m.Tutor, m.CreatedAt });
        builder.HasIndex(m => new { m.TenantId, m.ReservationId, m.CreatedAt });

        builder.Ignore(m => m.DomainEvents);
    }
}
