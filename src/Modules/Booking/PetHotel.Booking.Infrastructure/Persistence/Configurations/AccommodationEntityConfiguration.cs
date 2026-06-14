using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Accommodation"/> (xmin p/ overbooking, docs/04).</summary>
public sealed class AccommodationEntityConfiguration : IEntityTypeConfiguration<Accommodation>
{
    public void Configure(EntityTypeBuilder<Accommodation> builder)
    {
        builder.ToTable("accommodations");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AccommodationId(value))
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(a => a.Name).HasMaxLength(120).IsRequired();
        builder.Property(a => a.DailyRate).HasColumnName("daily_rate").HasPrecision(10, 2).IsRequired();
        builder.Property(a => a.Capacity).HasDefaultValue(1).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.LastBookedAt);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.CreatedBy).HasMaxLength(200);
        builder.Property(a => a.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(a => a.TenantId);

        // Concorrência otimista via coluna de sistema xmin (sem coluna extra).
        builder.UseXminConcurrencyToken();

        builder.Ignore(a => a.DomainEvents);
    }
}
