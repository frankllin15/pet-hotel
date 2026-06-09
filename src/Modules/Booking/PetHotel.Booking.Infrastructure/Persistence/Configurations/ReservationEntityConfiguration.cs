using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Reservation"/> (xmin + DateRange, docs/04).</summary>
public sealed class ReservationEntityConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ReservationId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(r => r.Pet)
            .HasConversion(pet => pet.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(r => r.AccommodationId)
            .HasConversion(id => id.Value, value => new AccommodationId(value))
            .IsRequired();

        // Período como value object inline (mesma tabela).
        builder.OwnsOne(r => r.Period, period =>
        {
            period.Property(p => p.Start).HasColumnName("check_in").IsRequired();
            period.Property(p => p.End).HasColumnName("check_out").IsRequired();
        });

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(200);
        builder.Property(r => r.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(r => new { r.TenantId, r.AccommodationId });

        // Concorrência otimista via xmin (impede overbooking sob confirmações concorrentes).
        builder.UseXminConcurrencyToken();

        builder.Ignore(r => r.DomainEvents);
    }
}
