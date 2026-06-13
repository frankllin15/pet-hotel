using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Registry.Domain.Packs;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Pack"/> (Fluent API, docs/04).</summary>
public sealed class PackEntityConfiguration : IEntityTypeConfiguration<Pack>
{
    public void Configure(EntityTypeBuilder<Pack> builder)
    {
        builder.ToTable("packs");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PackId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(1000);

        // Membros (referências por Id) persistidos como JSON dentro da linha (coleção owned).
        builder.OwnsMany(p => p.Members, owned => owned.ToJson());

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(p => p.TenantId);

        builder.Ignore(p => p.DomainEvents);
    }
}
