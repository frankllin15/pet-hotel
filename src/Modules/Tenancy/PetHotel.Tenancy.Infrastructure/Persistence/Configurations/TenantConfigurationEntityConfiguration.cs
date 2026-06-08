using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Configuration;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="TenantConfiguration"/> (docs/04).</summary>
public sealed class TenantConfigurationEntityConfiguration : IEntityTypeConfiguration<TenantConfiguration>
{
    public void Configure(EntityTypeBuilder<TenantConfiguration> builder)
    {
        builder.ToTable("tenant_configurations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new TenantConfigurationId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(c => c.TenantId).IsUnique();

        builder.Property(c => c.CheckInTime).IsRequired();
        builder.Property(c => c.CheckOutTime).IsRequired();
        builder.Property(c => c.SetupCompleted).IsRequired();

        // Coleções da configuração persistidas como JSON dentro da linha.
        builder.PrimitiveCollection(c => c.RequiredVaccines);
        builder.OwnsMany(c => c.AccommodationTypes, owned => owned.ToJson());

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);

        builder.Ignore(c => c.DomainEvents);
    }
}
