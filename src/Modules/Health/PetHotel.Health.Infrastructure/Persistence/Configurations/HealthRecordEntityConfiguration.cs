using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="HealthRecord"/> e da coleção de vacinações (docs/04).</summary>
public sealed class HealthRecordEntityConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("health_records");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, value => new HealthRecordId(value))
            .ValueGeneratedNever();

        builder.Property(h => h.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(h => h.Pet)
            .HasConversion(pet => pet.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.CreatedBy).HasMaxLength(200);
        builder.Property(h => h.UpdatedBy).HasMaxLength(200);

        // Uma ficha por pet dentro do tenant.
        builder.HasIndex(h => new { h.TenantId, h.Pet }).IsUnique();

        // Coleção de vacinações como parte do agregado (acesso via campo privado).
        builder.HasMany(h => h.Vaccinations)
            .WithOne()
            .HasForeignKey("HealthRecordId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(HealthRecord.Vaccinations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Coleção de controles de parasitas como parte do agregado (acesso via campo privado).
        builder.HasMany(h => h.ParasiteTreatments)
            .WithOne()
            .HasForeignKey("HealthRecordId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(HealthRecord.ParasiteTreatments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Veterinário particular persistido como JSON dentro da linha (mesmo padrão das coleções do Tutor).
        builder.OwnsOne(h => h.VetContact, owned => owned.ToJson());

        builder.Ignore(h => h.DomainEvents);
    }
}
